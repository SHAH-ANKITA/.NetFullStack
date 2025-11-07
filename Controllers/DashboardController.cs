using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GenerationPlanMLM.Data;
using GenerationPlanMLM.Models;
using GenerationPlanMLM.Services;
using System.Security.Claims;

namespace GenerationPlanMLM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GenerationService _generationService;

        public DashboardController(ApplicationDbContext context, GenerationService generationService)
        {
            _context = context;
            _generationService = generationService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var directReferrals = _generationService.GetDirectReferralsCount(userId);
            var totalTeamMembers = _generationService.GetTotalTeamMembers(userId);
            var totalIncome = _generationService.CalculateTotalIncome(userId);
            var levelDetails = _generationService.GetLevelDetails(userId);
            var referralNetwork = _generationService.GetReferralNetwork(userId);

            var viewModel = new DashboardViewModel
            {
                UserName = user.FullName,
                UserId = user.UserId,
                TotalDirectReferrals = directReferrals,
                TotalTeamMembers = totalTeamMembers,
                TotalIncome = totalIncome,
                LevelDetails = levelDetails,
                ReferralNetwork = referralNetwork
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Sponsor)
                .Include(u => u.Referrals)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                UserId = user.UserId,
                SponsorUserId = user.Sponsor?.UserId,
                JoinedOn = user.CreatedDate.ToString("dd MMM yyyy"),
                DirectReferrals = _generationService.GetDirectReferralsCount(userId),
                TeamMembers = _generationService.GetTotalTeamMembers(userId),
                TotalIncome = _generationService.CalculateTotalIncome(userId),
                IsActive = user.IsActive,
                IsAdmin = user.IsAdmin,
                RecentReferrals = user.Referrals
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(10)
                    .Select(r => new ProfileReferralInfo
                    {
                        UserId = r.UserId,
                        FullName = r.FullName,
                        JoinedOn = r.CreatedDate,
                        IsActive = r.IsActive
                    })
                    .ToList()
            };

            return View(profile);
        }
    }
}

