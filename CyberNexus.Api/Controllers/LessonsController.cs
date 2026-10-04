using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Controllers;

/// <summary>Public to read, admin-only to write. See CoursesController for the same rule.</summary>
[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LessonsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Lesson>>> GetAll([FromQuery] int? courseId)
    {
        var query = _db.Lessons.AsQueryable();

        if (courseId.HasValue)
            query = query.Where(l => l.CourseId == courseId.Value);

        var lessons = await query.OrderBy(l => l.Order).ToListAsync();
        return Ok(lessons);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Lesson>> GetById(int id)
    {
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return NotFound(new { message = "Lesson not found." });
        return Ok(lesson);
    }

    [HttpPost]
    [Authorize(Roles = AppUser.AdminRole)]
    public async Task<ActionResult<Lesson>> Create(Lesson lesson)
    {
        if (!await _db.Courses.AnyAsync(c => c.Id == lesson.CourseId))
            return BadRequest(new { message = "Course not found." });

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        return Ok(lesson);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppUser.AdminRole)]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return NotFound(new { message = "Lesson not found." });

        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
