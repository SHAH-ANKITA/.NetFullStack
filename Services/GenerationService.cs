using GenerationPlanMLM.Data;
using GenerationPlanMLM.Models;
using Microsoft.EntityFrameworkCore;

namespace GenerationPlanMLM.Services
{
    public class GenerationService
    {
        private readonly ApplicationDbContext _context;

        public GenerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public int GetDirectReferralsCount(int userId)
        {
            return _context.Users.Count(u => u.SponsorId == userId && u.IsActive);
        }

        public int GetTotalTeamMembers(int userId, int maxLevel = 3)
        {
            return GetTeamMembersRecursive(userId, 1, maxLevel);
        }

        private int GetTeamMembersRecursive(int userId, int currentLevel, int maxLevel)
        {
            if (currentLevel > maxLevel)
                return 0;

            var directReferrals = _context.Users
                .Where(u => u.SponsorId == userId && u.IsActive)
                .Select(u => u.Id)
                .ToList();

            int count = directReferrals.Count;

            foreach (var referralId in directReferrals)
            {
                count += GetTeamMembersRecursive(referralId, currentLevel + 1, maxLevel);
            }

            return count;
        }

        public decimal CalculateTotalIncome(int userId)
        {
            return _context.IncomeRecords
                .Where(ir => ir.UserId == userId)
                .Sum(ir => ir.Amount);
        }

        public List<GenerationLevelInfo> GetLevelDetails(int userId)
        {
            var levelDetails = new List<GenerationLevelInfo>();

            for (int level = 1; level <= 3; level++)
            {
                var members = GetMembersAtLevel(userId, level);
                var income = _context.IncomeRecords
                    .Where(ir => ir.UserId == userId && ir.Level == level)
                    .Sum(ir => ir.Amount);

                levelDetails.Add(new GenerationLevelInfo
                {
                    Level = level,
                    MemberCount = members,
                    Income = income
                });
            }

            return levelDetails;
        }

        private int GetMembersAtLevel(int userId, int targetLevel)
        {
            return GetMembersAtLevelRecursive(userId, 1, targetLevel);
        }

        private int GetMembersAtLevelRecursive(int userId, int currentLevel, int targetLevel)
        {
            if (currentLevel > targetLevel)
                return 0;

            if (currentLevel == targetLevel)
            {
                return _context.Users.Count(u => u.SponsorId == userId && u.IsActive);
            }

            var directReferrals = _context.Users
                .Where(u => u.SponsorId == userId && u.IsActive)
                .Select(u => u.Id)
                .ToList();

            int count = 0;
            foreach (var referralId in directReferrals)
            {
                count += GetMembersAtLevelRecursive(referralId, currentLevel + 1, targetLevel);
            }

            return count;
        }

        public void ProcessIncomeForNewUser(int newUserId, int sponsorId)
        {
            var sponsor = _context.Users.FirstOrDefault(u => u.Id == sponsorId);
            if (sponsor == null) return;

            ProcessIncomeRecursive(sponsor.Id, newUserId, 1);
        }

        public List<ReferralNode> GetReferralNetwork(int userId, int maxLevel = 3)
        {
            return GetReferralNodes(userId, 1, maxLevel);
        }

        private List<ReferralNode> GetReferralNodes(int userId, int currentLevel, int maxLevel)
        {
            if (currentLevel > maxLevel)
            {
                return new List<ReferralNode>();
            }

            var directReferrals = _context.Users
                .Where(u => u.SponsorId == userId && u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new ReferralNode
                {
                    UserDbId = u.Id,
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    Level = currentLevel,
                    Children = new List<ReferralNode>()
                })
                .ToList();

            foreach (var referral in directReferrals)
            {
                referral.Children = GetReferralNodes(referral.UserDbId, currentLevel + 1, maxLevel);
            }

            return directReferrals;
        }

        private void ProcessIncomeRecursive(int userId, int fromUserId, int level)
        {
            if (level > 3) return;

            decimal amount = 0;
            if (level == 1) amount = 100;
            else if (level == 2) amount = 50;
            else if (level == 3) amount = 25;

            if (amount > 0)
            {
                var incomeRecord = new IncomeRecord
                {
                    UserId = userId,
                    FromUserId = fromUserId,
                    Level = level,
                    Amount = amount,
                    CreatedDate = DateTime.Now
                };

                _context.IncomeRecords.Add(incomeRecord);
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user?.SponsorId != null)
            {
                ProcessIncomeRecursive(user.SponsorId.Value, fromUserId, level + 1);
            }
        }
    }
}

