using CyberNexus.Api.Models;

namespace CyberNexus.Api.Services;

/// <summary>
/// Owns every XP / level / streak rule in one place, so the lesson, quiz and
/// challenge endpoints all reward progress the same way instead of each one
/// re-implementing it (and drifting).
/// </summary>
public class ProgressionService
{
    /// <summary>
    /// Total XP needed to *reach* a level. Triangular: L1=0, L2=100, L3=300,
    /// L4=600, L5=1000... so each level costs a bit more than the last.
    /// </summary>
    public static int XpFloorForLevel(int level)
    {
        if (level <= 1) return 0;
        return 50 * level * (level - 1);
    }

    /// <summary>Inverse of <see cref="XpFloorForLevel"/>: the level a given XP total lands on.</summary>
    public static int LevelForXp(int totalXp)
    {
        if (totalXp <= 0) return 1;
        var level = (int)Math.Floor((1 + Math.Sqrt(1 + 4.0 * totalXp / 50.0)) / 2);
        return Math.Max(1, level);
    }

    /// <summary>XP already banked inside the current level.</summary>
    public static int XpIntoLevel(int totalXp)
    {
        var level = LevelForXp(totalXp);
        return totalXp - XpFloorForLevel(level);
    }

    /// <summary>XP still needed before the next level. Equal to the current level's span at the cap.</summary>
    public static int XpToNextLevel(int totalXp)
    {
        var level = LevelForXp(totalXp);
        return XpFloorForLevel(level + 1) - XpFloorForLevel(level);
    }

    /// <summary>Adds XP, re-derives the level, and extends the daily streak. Returns the XP actually added.</summary>
    public int Award(AppUser user, int amount, DateTime utcNow)
    {
        if (amount <= 0) return 0;

        user.Xp += amount;
        user.Level = LevelForXp(user.Xp);
        TouchStreak(user, utcNow);
        return amount;
    }

    /// <summary>
    /// Extends the streak for <paramref name="utcNow"/>. Same day = no change,
    /// consecutive day = +1, any bigger gap = reset to 1.
    /// </summary>
    public void TouchStreak(AppUser user, DateTime utcNow)
    {
        var today = utcNow.Date;

        if (user.LastActivityDate is null)
        {
            user.CurrentStreak = 1;
        }
        else
        {
            var last = user.LastActivityDate.Value.Date;
            var gap = (today - last).Days;

            if (gap == 0)
            {
                // Already counted today, leave the streak alone.
            }
            else if (gap == 1)
            {
                user.CurrentStreak += 1;
            }
            else
            {
                user.CurrentStreak = 1;
            }
        }

        user.LastActivityDate = today;
    }
}
