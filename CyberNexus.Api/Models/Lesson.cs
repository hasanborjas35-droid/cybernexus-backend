namespace CyberNexus.Api.Models;

public class Lesson
{
    public int Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string ContentEn { get; set; } = string.Empty;
    public string ContentAr { get; set; } = string.Empty;
    public int Order { get; set; }

    /// <summary>XP awarded the first time a user completes this lesson. Re-completing never pays out again.</summary>
    public int XpReward { get; set; } = 100;

    public int CourseId { get; set; }
    public Course? Course { get; set; }
}