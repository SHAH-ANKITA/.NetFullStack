namespace GenerationPlanMLM.Models
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int TotalDirectReferrals { get; set; }
        public int TotalTeamMembers { get; set; }
        public decimal TotalIncome { get; set; }
        public List<GenerationLevelInfo> LevelDetails { get; set; } = new List<GenerationLevelInfo>();
        public List<ReferralNode> ReferralNetwork { get; set; } = new List<ReferralNode>();
    }

    public class GenerationLevelInfo
    {
        public int Level { get; set; }
        public int MemberCount { get; set; }
        public decimal Income { get; set; }
    }

    public class ReferralNode
    {
        public int UserDbId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<ReferralNode> Children { get; set; } = new List<ReferralNode>();
    }
}

