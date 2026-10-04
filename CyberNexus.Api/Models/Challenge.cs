namespace CyberNexus.Api.Models;

public class Challenge
{
    public int Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    /// <summary>Threat category, e.g. "phishing", "network".</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>"beginner" | "intermediate" | "advanced".</summary>
    public string Difficulty { get; set; } = "beginner";

    public int XpReward { get; set; } = 250;
    public int EstimatedMinutes { get; set; } = 5;
    public int Order { get; set; }

    /// <summary>Minimum user level required before the challenge is playable.</summary>
    public int RequiredLevel { get; set; } = 1;

    /// <summary>Answer choices, stored as a JSON array of strings.</summary>
    public string OptionsJson { get; set; } = "[]";

    /// <summary>Index into OptionsJson. Never sent to clients — see the challenges controller.</summary>
    public int CorrectIndex { get; set; }

    public string ExplanationEn { get; set; } = string.Empty;
    public string ExplanationAr { get; set; } = string.Empty;
}
