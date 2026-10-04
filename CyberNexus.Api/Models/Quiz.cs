namespace CyberNexus.Api.Models;

public class Quiz
{
    public int Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public int Order { get; set; }

    /// <summary>Percent needed to pass.</summary>
    public int PassingScore { get; set; } = 70;

    /// <summary>XP awarded once, the first time the quiz is passed.</summary>
    public int XpReward { get; set; } = 300;

    public int? CourseId { get; set; }
    public Course? Course { get; set; }

    public List<QuizQuestion> Questions { get; set; } = new();
}
