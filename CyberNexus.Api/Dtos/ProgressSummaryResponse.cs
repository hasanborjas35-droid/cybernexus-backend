namespace CyberNexus.Api.Dtos;

/// <summary>Everything the web and the mobile app need to draw the progress screen.</summary>
public class ProgressSummaryResponse
{
    public int Xp { get; set; }
    public int Level { get; set; }

    /// <summary>XP banked inside the current level.</summary>
    public int XpIntoLevel { get; set; }

    /// <summary>Total XP that was needed to reach the current level.</summary>
    public int LevelFloorXp { get; set; }

    /// <summary>XP still needed to reach the next level.</summary>
    public int XpToNextLevel { get; set; }

    public int CurrentStreak { get; set; }

    public int LessonsCompleted { get; set; }
    public int LessonsTotal { get; set; }

    /// <summary>0-100, rounded.</summary>
    public int PercentComplete { get; set; }

    public int QuizzesCompleted { get; set; }
    public int ChallengesCompleted { get; set; }
    public int AchievementsUnlocked { get; set; }
}
