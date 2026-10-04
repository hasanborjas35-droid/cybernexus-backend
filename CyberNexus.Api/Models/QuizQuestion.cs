namespace CyberNexus.Api.Models;

public class QuizQuestion
{
    public int Id { get; set; }
    public string QuestionEn { get; set; } = string.Empty;
    public string QuestionAr { get; set; } = string.Empty;

    /// <summary>Answer choices, stored as a JSON array of strings.</summary>
    public string OptionsJson { get; set; } = "[]";

    /// <summary>Index into OptionsJson. Never sent to clients — see the quiz controller.</summary>
    public int CorrectIndex { get; set; }

    public string ExplanationEn { get; set; } = string.Empty;
    public string ExplanationAr { get; set; } = string.Empty;
    public int Order { get; set; }

    public int QuizId { get; set; }
    public Quiz? Quiz { get; set; }
}
