namespace GenerationPlanMLM.Models
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? SponsorUserId { get; set; }
        public string JoinedOn { get; set; } = string.Empty;
        public int DirectReferrals { get; set; }
        public int TeamMembers { get; set; }
        public decimal TotalIncome { get; set; }
        public bool IsActive { get; set; }
        public string Role => IsAdmin ? "Admin" : "Member";
        public bool IsAdmin { get; set; }
        public List<ProfileReferralInfo> RecentReferrals { get; set; } = new List<ProfileReferralInfo>();
    }

    public class ProfileReferralInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime JoinedOn { get; set; }
        public bool IsActive { get; set; }
    }
}

