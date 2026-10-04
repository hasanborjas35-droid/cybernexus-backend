namespace CyberNexus.Api.Models;

public class UserAchievement
{
    public int Id { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public int AchievementId { get; set; }
    public Achievement? Achievement { get; set; }
}
