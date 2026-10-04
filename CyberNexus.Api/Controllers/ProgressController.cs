using CyberNexus.Api.Data;
using CyberNexus.Api.Dtos;
using CyberNexus.Api.Models;
using CyberNexus.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

[ApiController]
[Route("api/progress")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProgressionService _progression;

    public ProgressController(AppDbContext db, ProgressionService progression)
    {
        _db = db;
        _progression = progression;
    }

    private int? CurrentUserId() => User.GetUserId();

    /// <summary>Ids of the lessons this user has completed. Frontends use it to paint done/current/locked.</summary>
    [HttpGet]
    public async Task<ActionResult<List<int>>> GetCompletedLessonIds()
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var ids = await _db.UserLessonProgresses
            .Where(p => p.UserId == uid && p.IsCompleted)
            .Select(p => p.LessonId)
            .ToListAsync();

        return Ok(ids);
    }

    /// <summary>
    /// Everything the progress screen needs in one call, so the app doesn't have
    /// to stitch four endpoints together and can never show a half-updated mix.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ProgressSummaryResponse>> GetSummary()
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var completed = await _db.UserLessonProgresses
            .CountAsync(p => p.UserId == uid && p.IsCompleted);

        var total = await _db.Lessons.CountAsync();

        var quizzes = await _db.UserQuizAttempts
            .Where(a => a.UserId == uid && a.IsPassed)
            .Select(a => a.QuizId)
            .Distinct()
            .CountAsync();

        var challenges = await _db.UserChallengeResults
            .CountAsync(c => c.UserId == uid && c.IsSolved);

        var achievements = await _db.UserAchievements
            .CountAsync(a => a.UserId == uid);

        return Ok(new ProgressSummaryResponse
        {
            Xp = user.Xp,
            Level = user.Level,
            XpIntoLevel = ProgressionService.XpIntoLevel(user.Xp),
            LevelFloorXp = ProgressionService.XpFloorForLevel(user.Level),
            XpToNextLevel = ProgressionService.XpToNextLevel(user.Xp),
            CurrentStreak = user.CurrentStreak,
            LessonsCompleted = completed,
            LessonsTotal = total,
            PercentComplete = total > 0 ? (int)Math.Round(completed / (double)total * 100) : 0,
            QuizzesCompleted = quizzes,
            ChallengesCompleted = challenges,
            AchievementsUnlocked = achievements
        });
    }

    /// <summary>
    /// Marks a lesson done and pays its XP. Re-submitting a lesson already
    /// completed is a no-op: no second row, no second XP payout, no streak bump.
    /// </summary>
    [HttpPost("{lessonId:int}/complete")]
    public async Task<IActionResult> Complete(int lessonId)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var lesson = await _db.Lessons.FindAsync(lessonId);
        if (lesson == null)
            return NotFound(new { message = "Lesson not found." });

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var existing = await _db.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == uid && p.LessonId == lessonId);

        // Already completed: report it and change nothing.
        if (existing is { IsCompleted: true })
            return Ok(new { message = "Lesson already completed.", xpAwarded = 0, xp = user.Xp, level = user.Level });

        var now = DateTime.UtcNow;

        if (existing == null)
        {
            _db.UserLessonProgresses.Add(new UserLessonProgress
            {
                UserId = uid,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = now
            });
        }
        else
        {
            existing.IsCompleted = true;
            existing.CompletedAt = now;
        }

        var awarded = _progression.Award(user, lesson.XpReward, now);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Lesson completed.",
            xpAwarded = awarded,
            xp = user.Xp,
            level = user.Level,
            currentStreak = user.CurrentStreak
        });
    }
}
