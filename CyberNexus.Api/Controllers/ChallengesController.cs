using System.Text.Json;
using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using CyberNexus.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

[ApiController]
[Route("api/challenges")]
[Authorize]
public class ChallengesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProgressionService _progression;

    public ChallengesController(AppDbContext db, ProgressionService progression)
    {
        _db = db;
        _progression = progression;
    }

    private static string[] ParseOptions(string json)
    {
        try { return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>(); }
        catch (JsonException) { return Array.Empty<string>(); }
    }

    /// <summary>All challenges, each flagged locked/unlocked for this user. The answer key is never included.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var solved = await _db.UserChallengeResults
            .Where(r => r.UserId == uid && r.IsSolved)
            .Select(r => r.ChallengeId)
            .ToListAsync();

        var list = await _db.Challenges.OrderBy(c => c.Order).ToListAsync();

        return Ok(list.Select(c => new
        {
            c.Id,
            c.TitleEn,
            c.TitleAr,
            c.DescriptionEn,
            c.DescriptionAr,
            c.Category,
            c.Difficulty,
            c.XpReward,
            c.EstimatedMinutes,
            c.RequiredLevel,
            isSolved = solved.Contains(c.Id),
            isLocked = user.Level < c.RequiredLevel,
            options = ParseOptions(c.OptionsJson)
        }));
    }

    public class SolveRequest
    {
        public int AnswerIndex { get; set; }
    }

    /// <summary>
    /// Grading a challenge. The reward is paid only on the first correct solve,
    /// but a wrong attempt still records the attempt count and returns the
    /// explanation so the user learns from it.
    /// </summary>
    [HttpPost("{id:int}/solve")]
    public async Task<IActionResult> Solve(int id, [FromBody] SolveRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var challenge = await _db.Challenges.FindAsync(id);
        if (challenge == null) return NotFound(new { message = "Challenge not found." });

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        if (user.Level < challenge.RequiredLevel)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = $"Requires level {challenge.RequiredLevel}." });

        var isCorrect = request.AnswerIndex == challenge.CorrectIndex;
        var now = DateTime.UtcNow;

        var result = await _db.UserChallengeResults
            .FirstOrDefaultAsync(r => r.UserId == uid && r.ChallengeId == id);

        if (result == null)
        {
            result = new UserChallengeResult { UserId = uid, ChallengeId = id };
            _db.UserChallengeResults.Add(result);
        }

        result.Attempts += 1;

        var awarded = 0;
        if (isCorrect && !result.IsSolved)
        {
            result.IsSolved = true;
            result.SolvedAt = now;
            awarded = _progression.Award(user, challenge.XpReward, now);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            isCorrect,
            isSolved = result.IsSolved,
            attempts = result.Attempts,
            xpAwarded = awarded,
            xp = user.Xp,
            level = user.Level,
            correctIndex = challenge.CorrectIndex,
            options = ParseOptions(challenge.OptionsJson),
            explanationEn = challenge.ExplanationEn,
            explanationAr = challenge.ExplanationAr
        });
    }
}
