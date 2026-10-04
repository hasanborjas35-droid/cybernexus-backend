using System.Text.Json;
using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using CyberNexus.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProgressionService _progression;

    public QuizzesController(AppDbContext db, ProgressionService progression)
    {
        _db = db;
        _progression = progression;
    }

    private static string[] ParseOptions(string json)
    {
        try { return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>(); }
        catch (JsonException) { return Array.Empty<string>(); }
    }

    /// <summary>
    /// Returns the questions with the answer key stripped out. Sending
    /// CorrectIndex here would let anyone read the answer from devtools.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetQuiz(int id)
    {
        var quiz = await _db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null) return NotFound(new { message = "Quiz not found." });

        return Ok(new
        {
            quiz.Id,
            quiz.TitleEn,
            quiz.TitleAr,
            quiz.DescriptionEn,
            quiz.DescriptionAr,
            quiz.PassingScore,
            totalXp = quiz.XpReward,
            questions = quiz.Questions
                .OrderBy(q => q.Order)
                .Select(q => new
                {
                    q.Id,
                    q.QuestionEn,
                    q.QuestionAr,
                    options = ParseOptions(q.OptionsJson)
                })
        });
    }

    /// <summary>Indexes of the answers the user picked, in question order.</summary>
    public class SubmitRequest
    {
        public List<int> Answers { get; set; } = new();
    }

    /// <summary>
    /// Grades a run, stores the attempt, and pays XP the first time the quiz is
    /// passed. Returns the per-question explanations only after grading, so the
    /// learning feedback is tied to actually answering.
    /// </summary>
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var uid = userId.Value;

        var quiz = await _db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null) return NotFound(new { message = "Quiz not found." });

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var questions = quiz.Questions.OrderBy(q => q.Order).ToList();

        if (request.Answers.Count != questions.Count)
            return BadRequest(new { message = $"Expected {questions.Count} answers, got {request.Answers.Count}." });

        var now = DateTime.UtcNow;
        var correct = 0;
        var results = new List<object>();

        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var picked = request.Answers[i];
            var isCorrect = picked == q.CorrectIndex;
            if (isCorrect) correct++;

            results.Add(new
            {
                questionId = q.Id,
                yourAnswer = picked,
                correctIndex = q.CorrectIndex,
                isCorrect,
                options = ParseOptions(q.OptionsJson),
                explanationEn = q.ExplanationEn,
                explanationAr = q.ExplanationAr
            });
        }

        var scorePercent = questions.Count > 0
            ? (int)Math.Round(correct / (double)questions.Count * 100)
            : 0;

        var passed = scorePercent >= quiz.PassingScore;

        _db.UserQuizAttempts.Add(new UserQuizAttempt
        {
            UserId = uid,
            QuizId = id,
            CorrectCount = correct,
            TotalQuestions = questions.Count,
            ScorePercent = scorePercent,
            IsPassed = passed,
            CompletedAt = now
        });

        // XP is paid once per quiz, on the first pass only.
        var alreadyPassed = await _db.UserQuizAttempts
            .AnyAsync(a => a.UserId == uid && a.QuizId == id && a.IsPassed);

        var awarded = 0;
        if (passed && !alreadyPassed)
        {
            awarded = _progression.Award(user, quiz.XpReward, now);
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            scorePercent,
            correct,
            total = questions.Count,
            passed,
            passingScore = quiz.PassingScore,
            xpAwarded = awarded,
            xp = user.Xp,
            level = user.Level,
            results
        });
    }

    /// <summary>Every quiz id the current user has already passed, so the UI can badge them.</summary>
    [HttpGet("completed")]
    public async Task<ActionResult<List<int>>> GetPassedQuizIds()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var ids = await _db.UserQuizAttempts
            .Where(a => a.UserId == userId && a.IsPassed)
            .Select(a => a.QuizId)
            .Distinct()
            .ToListAsync();

        return Ok(ids);
    }
}
