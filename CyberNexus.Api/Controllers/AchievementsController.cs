using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using CyberNexus.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

[ApiController]
[Route("api/achievements")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProgressionService _progression;

    public AchievementsController(AppDbContext db, ProgressionService progression)
    {
        _db = db;
        _progression = progression;
    }

    /// <summary>Every achievement with its unlocked/locked state for this user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var unlocked = await _db.UserAchievements
            .Where(a => a.UserId == uid)
            .Select(a => new { a.AchievementId, a.UnlockedAt })
            .ToDictionaryAsync(a => a.AchievementId, a => a.UnlockedAt);

        var all = await _db.Achievements.OrderBy(a => a.Id).ToListAsync();

        return Ok(all.Select(a => new
        {
            a.Id,
            a.Key,
            a.TitleEn,
            a.TitleAr,
            a.DescriptionEn,
            a.DescriptionAr,
            a.Icon,
            a.RequiredLevel,
            a.XpReward,
            isUnlocked = unlocked.ContainsKey(a.Id),
            unlockedAt = unlocked.TryGetValue(a.Id, out var at) ? at : (DateTime?)null
        }));
    }

    /// <summary>
    /// Re-checks every achievement and unlocks the ones whose conditions are now
    /// met, paying each reward once. Safe to call after any progress action; it
    /// is the single place that decides what counts as earned.
    /// Returns only the achievements that were newly unlocked by this call, so
    /// the caller can show a "you unlocked X" popup.
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var all = await _db.Achievements.ToListAsync();
        var already = await _db.UserAchievements
            .Where(a => a.UserId == uid)
            .Select(a => a.AchievementId)
            .ToListAsync();

        var lessonsDone = await _db.UserLessonProgresses
            .CountAsync(p => p.UserId == uid && p.IsCompleted);
        var quizzesPassed = await _db.UserQuizAttempts
            .Where(a => a.UserId == uid && a.IsPassed)
            .Select(a => a.QuizId)
            .Distinct()
            .CountAsync();
        var challengesSolved = await _db.UserChallengeResults
            .CountAsync(r => r.UserId == uid && r.IsSolved);
        var totalLessons = await _db.Lessons.CountAsync();

        var now = DateTime.UtcNow;
        var newlyUnlocked = new List<object>();

        foreach (var a in all)
        {
            if (already.Contains(a.Id)) continue;
            if (user.Level < a.RequiredLevel) continue;
            if (!MeetsCondition(a.Key, lessonsDone, quizzesPassed, challengesSolved, user.CurrentStreak, totalLessons)) continue;

            _db.UserAchievements.Add(new UserAchievement
            {
                UserId = uid,
                AchievementId = a.Id,
                UnlockedAt = now
            });

            // Achievement XP feeds back into XP, so re-derive the level and let
            // a reward push the user over a level boundary.
            _progression.Award(user, a.XpReward, now);

            newlyUnlocked.Add(new
            {
                a.Id,
                a.Key,
                a.TitleEn,
                a.TitleAr,
                a.Icon,
                a.XpReward
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            newlyUnlocked,
            xp = user.Xp,
            level = user.Level,
            totalUnlocked = already.Count + newlyUnlocked.Count,
            totalAvailable = all.Count
        });
    }

    /// <summary>
    /// Progress rules per achievement key. Unknown keys never unlock — a
    /// mistyped key in the seed data stays locked instead of silently firing.
    /// </summary>
    private static bool MeetsCondition(string key, int lessons, int quizzes, int challenges, int streak, int totalLessons) => key switch
    {
        "first_lesson"      => lessons >= 1,
        "five_lessons"      => lessons >= 5,
        // Only unlockable once there is actually something to finish, otherwise
        // a brand-new user with 0 lessons would earn it for free.
        "all_lessons"       => totalLessons > 0 && lessons >= totalLessons,
        "first_quiz"        => quizzes >= 1,
        "quiz_master"       => quizzes >= 5,
        "first_challenge"   => challenges >= 1,
        "challenge_veteran" => challenges >= 10,
        "streak_3"          => streak >= 3,
        "streak_7"          => streak >= 7,
        _ => false
    };
}
