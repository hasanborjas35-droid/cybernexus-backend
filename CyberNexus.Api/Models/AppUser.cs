namespace CyberNexus.Api.Models;

public class AppUser
{
    /// <summary>"Student" or "Admin". Admins may create/modify catalogue content.</summary>
    public const string StudentRole = "Student";
    public const string AdminRole = "Admin";

    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = StudentRole;
    public int Xp { get; set; }
    public int Level { get; set; } = 1;
    public int CurrentStreak { get; set; }

    /// <summary>Day the streak was last extended, so one lesson per day counts once.</summary>
    public DateTime? LastActivityDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UserLessonProgress> LessonProgress { get; set; } = new();
}