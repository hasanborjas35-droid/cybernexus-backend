namespace CyberNexus.Api.Models;

/// <summary>One finished run of a quiz by one user. Kept so a user can retake, and history survives.</summary>
public class UserQuizAttempt
{
    public int Id { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public int ScorePercent { get; set; }
    public bool IsPassed { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
}
