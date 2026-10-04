namespace CyberNexus.Api.Models;

public class UserLessonProgress
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}