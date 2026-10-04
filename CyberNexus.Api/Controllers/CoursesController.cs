using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

/// <summary>
/// Reading the catalogue is public so the marketing/landing pages work signed
/// out, but changing it is admin-only. Without the Authorize attribute anyone
/// could POST a course or a learning path.
/// </summary>
[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CoursesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> GetAll([FromQuery] int? learningPathId)
    {
        var query = _db.Courses.AsQueryable();

        if (learningPathId.HasValue)
            query = query.Where(c => c.LearningPathId == learningPathId.Value);

        var courses = await query.OrderBy(c => c.Order).ToListAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Course>> GetById(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return NotFound(new { message = "Course not found." });
        return Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = AppUser.AdminRole)]
    public async Task<ActionResult<Course>> Create(Course course)
    {
        if (!await _db.LearningPaths.AnyAsync(p => p.Id == course.LearningPathId))
            return BadRequest(new { message = "Learning path not found." });

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return Ok(course);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppUser.AdminRole)]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound(new { message = "Course not found." });

        // Lessons and quizzes cascade via the FK, which is what we want.
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
