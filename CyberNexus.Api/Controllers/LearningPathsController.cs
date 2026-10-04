using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

/// <summary>Public to read, admin-only to write. See CoursesController for the same rule.</summary>
[ApiController]
[Route("api/learningpaths")]
public class LearningPathsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LearningPathsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<LearningPath>>> GetAll()
    {
        var paths = await _db.LearningPaths.ToListAsync();
        return Ok(paths);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LearningPath>> GetById(int id)
    {
        var path = await _db.LearningPaths
            .Include(p => p.Courses)
                .ThenInclude(c => c.Lessons)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (path == null) return NotFound(new { message = "Learning path not found." });
        return Ok(path);
    }

    [HttpPost]
    [Authorize(Roles = AppUser.AdminRole)]
    public async Task<ActionResult<LearningPath>> Create(LearningPath path)
    {
        _db.LearningPaths.Add(path);
        await _db.SaveChangesAsync();
        return Ok(path);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppUser.AdminRole)]
    public async Task<IActionResult> Delete(int id)
    {
        var path = await _db.LearningPaths.FindAsync(id);
        if (path == null) return NotFound(new { message = "Learning path not found." });

        // Courses, lessons and quizzes all cascade from here.
        _db.LearningPaths.Remove(path);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
