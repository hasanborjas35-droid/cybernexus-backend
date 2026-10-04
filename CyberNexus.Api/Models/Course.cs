namespace CyberNexus.Api.Models;

public class Course
{
    public int Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public int Order { get; set; }

    public int LearningPathId { get; set; }
    public LearningPath? LearningPath { get; set; }

    public List<Lesson> Lessons { get; set; } = new();
    public List<Quiz> Quizzes { get; set; } = new();
}