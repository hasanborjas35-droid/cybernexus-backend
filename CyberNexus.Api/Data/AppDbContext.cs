using CyberNexus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<UserLessonProgress> UserLessonProgresses => Set<UserLessonProgress>();

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<UserQuizAttempt> UserQuizAttempts => Set<UserQuizAttempt>();

    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<UserChallengeResult> UserChallengeResults => Set<UserChallengeResult>();

    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        // A user can only hold one progress row per lesson, so a re-complete is
        // guaranteed to update the existing row instead of inserting a duplicate.
        modelBuilder.Entity<UserLessonProgress>(entity =>
        {
            entity.HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            // A quiz is optional on a course, so the FK must stay nullable or
            // EF would create an unnullable shadow property.
            entity.HasOne(q => q.Course)
                  .WithMany(c => c.Quizzes)
                  .HasForeignKey(q => q.CourseId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasOne(q => q.Quiz)
                  .WithMany(qz => qz.Questions)
                  .HasForeignKey(qz => qz.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserQuizAttempt>(entity =>
        {
            entity.HasIndex(a => new { a.UserId, a.QuizId });
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasIndex(c => c.Order);
        });

        modelBuilder.Entity<UserChallengeResult>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.ChallengeId }).IsUnique();
        });

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasIndex(a => a.Key).IsUnique();
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            // Unlocking the same achievement twice is a no-op, not a duplicate.
            entity.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();
        });
    }
}
