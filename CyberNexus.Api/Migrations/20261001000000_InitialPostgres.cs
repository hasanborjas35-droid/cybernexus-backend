using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberNexus.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id            = table.Column<int>(nullable: false),
                    Key           = table.Column<string>(maxLength: 100, nullable: false),
                    TitleEn       = table.Column<string>(maxLength: 200, nullable: false),
                    TitleAr       = table.Column<string>(maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(maxLength: 500, nullable: false),
                    DescriptionAr = table.Column<string>(maxLength: 500, nullable: false),
                    XpReward      = table.Column<int>(nullable: false),
                    RequiredLevel = table.Column<int>(nullable: false),
                    IconKey       = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningPaths",
                columns: table => new
                {
                    Id            = table.Column<int>(nullable: false),
                    TitleEn       = table.Column<string>(maxLength: 200, nullable: false),
                    TitleAr       = table.Column<string>(maxLength: 200, nullable: false),
                    DescriptionEn = table.Column<string>(maxLength: 1000, nullable: false),
                    DescriptionAr = table.Column<string>(maxLength: 1000, nullable: false),
                    Order         = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id               = table.Column<int>(nullable: false),
                    Username         = table.Column<string>(maxLength: 100, nullable: false),
                    Email            = table.Column<string>(maxLength: 200, nullable: false),
                    PasswordHash     = table.Column<string>(nullable: false),
                    Role             = table.Column<string>(maxLength: 50, nullable: false),
                    Xp               = table.Column<int>(nullable: false),
                    Level            = table.Column<int>(nullable: false),
                    CurrentStreak    = table.Column<int>(nullable: false),
                    LastActivityDate = table.Column<DateTime>(nullable: true),
                    CreatedAt        = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id               = table.Column<int>(nullable: false),
                    TitleEn          = table.Column<string>(maxLength: 200, nullable: false),
                    TitleAr          = table.Column<string>(maxLength: 200, nullable: false),
                    DescriptionEn    = table.Column<string>(maxLength: 2000, nullable: false),
                    DescriptionAr    = table.Column<string>(maxLength: 2000, nullable: false),
                    Difficulty       = table.Column<string>(maxLength: 50, nullable: false),
                    Category         = table.Column<string>(maxLength: 100, nullable: false),
                    XpReward         = table.Column<int>(nullable: false),
                    RequiredLevel    = table.Column<int>(nullable: false),
                    Order            = table.Column<int>(nullable: false),
                    CorrectIndex     = table.Column<int>(nullable: false),
                    Options          = table.Column<string>(nullable: false),
                    ExplanationEn    = table.Column<string>(maxLength: 1000, nullable: false),
                    ExplanationAr    = table.Column<string>(maxLength: 1000, nullable: false),
                    EstimatedMinutes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id              = table.Column<int>(nullable: false),
                    TitleEn         = table.Column<string>(maxLength: 200, nullable: false),
                    TitleAr         = table.Column<string>(maxLength: 200, nullable: false),
                    DescriptionEn   = table.Column<string>(maxLength: 1000, nullable: false),
                    DescriptionAr   = table.Column<string>(maxLength: 1000, nullable: false),
                    Order           = table.Column<int>(nullable: false),
                    LearningPathId  = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey("FK_Courses_LearningPaths_LearningPathId", x => x.LearningPathId,
                        "LearningPaths", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id            = table.Column<int>(nullable: false),
                    UserId        = table.Column<int>(nullable: false),
                    AchievementId = table.Column<int>(nullable: false),
                    UnlockedAt    = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey("FK_UserAchievements_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserAchievements_Achievements_AchievementId", x => x.AchievementId,
                        "Achievements", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChallengeResults",
                columns: table => new
                {
                    Id          = table.Column<int>(nullable: false),
                    UserId      = table.Column<int>(nullable: false),
                    ChallengeId = table.Column<int>(nullable: false),
                    AnswerIndex = table.Column<int>(nullable: false),
                    IsCorrect   = table.Column<bool>(nullable: false),
                    SolvedAt    = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChallengeResults", x => x.Id);
                    table.ForeignKey("FK_UserChallengeResults_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserChallengeResults_Challenges_ChallengeId", x => x.ChallengeId,
                        "Challenges", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id        = table.Column<int>(nullable: false),
                    TitleEn   = table.Column<string>(maxLength: 200, nullable: false),
                    TitleAr   = table.Column<string>(maxLength: 200, nullable: false),
                    ContentEn = table.Column<string>(maxLength: 2000, nullable: false),
                    ContentAr = table.Column<string>(maxLength: 2000, nullable: false),
                    Order     = table.Column<int>(nullable: false),
                    XpReward  = table.Column<int>(nullable: false),
                    CourseId  = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                    table.ForeignKey("FK_Lessons_Courses_CourseId", x => x.CourseId,
                        "Courses", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id           = table.Column<int>(nullable: false),
                    TitleEn      = table.Column<string>(maxLength: 200, nullable: false),
                    TitleAr      = table.Column<string>(maxLength: 200, nullable: false),
                    PassingScore = table.Column<int>(nullable: false),
                    XpReward     = table.Column<int>(nullable: false),
                    CourseId     = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey("FK_Quizzes_Courses_CourseId", x => x.CourseId,
                        "Courses", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserLessonProgresses",
                columns: table => new
                {
                    Id          = table.Column<int>(nullable: false),
                    UserId      = table.Column<int>(nullable: false),
                    LessonId    = table.Column<int>(nullable: false),
                    CompletedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLessonProgresses", x => x.Id);
                    table.ForeignKey("FK_UserLessonProgresses_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserLessonProgresses_Lessons_LessonId", x => x.LessonId,
                        "Lessons", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id            = table.Column<int>(nullable: false),
                    QuizId        = table.Column<int>(nullable: false),
                    QuestionEn    = table.Column<string>(maxLength: 1000, nullable: false),
                    QuestionAr    = table.Column<string>(maxLength: 1000, nullable: false),
                    Options       = table.Column<string>(nullable: false),
                    CorrectIndex  = table.Column<int>(nullable: false),
                    ExplanationEn = table.Column<string>(maxLength: 1000, nullable: false),
                    ExplanationAr = table.Column<string>(maxLength: 1000, nullable: false),
                    Order         = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey("FK_QuizQuestions_Quizzes_QuizId", x => x.QuizId,
                        "Quizzes", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizAttempts",
                columns: table => new
                {
                    Id          = table.Column<int>(nullable: false),
                    UserId      = table.Column<int>(nullable: false),
                    QuizId      = table.Column<int>(nullable: false),
                    Score       = table.Column<int>(nullable: false),
                    Passed      = table.Column<bool>(nullable: false),
                    AttemptedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAttempts", x => x.Id);
                    table.ForeignKey("FK_UserQuizAttempts_Users_UserId", x => x.UserId,
                        "Users", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserQuizAttempts_Quizzes_QuizId", x => x.QuizId,
                        "Quizzes", "Id", onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex("IX_Users_Email",    "Users",    "Email",    unique: true);
            migrationBuilder.CreateIndex("IX_Users_Username", "Users",    "Username", unique: true);
            migrationBuilder.CreateIndex("IX_Courses_LearningPathId",   "Courses",  "LearningPathId");
            migrationBuilder.CreateIndex("IX_Lessons_CourseId",         "Lessons",  "CourseId");
            migrationBuilder.CreateIndex("IX_Quizzes_CourseId",         "Quizzes",  "CourseId");
            migrationBuilder.CreateIndex("IX_QuizQuestions_QuizId",     "QuizQuestions", "QuizId");
            migrationBuilder.CreateIndex("IX_Challenges_Order",         "Challenges", "Order");
            migrationBuilder.CreateIndex("IX_Achievements_Key",         "Achievements", "Key", unique: true);
            migrationBuilder.CreateIndex("IX_UserLessonProgresses_UserId_LessonId",    "UserLessonProgresses",   new[] { "UserId", "LessonId" },   unique: true);
            migrationBuilder.CreateIndex("IX_UserChallengeResults_UserId_ChallengeId", "UserChallengeResults",   new[] { "UserId", "ChallengeId" }, unique: true);
            migrationBuilder.CreateIndex("IX_UserAchievements_UserId_AchievementId",   "UserAchievements",       new[] { "UserId", "AchievementId" }, unique: true);
            migrationBuilder.CreateIndex("IX_UserQuizAttempts_UserId_QuizId",          "UserQuizAttempts",       new[] { "UserId", "QuizId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("UserQuizAttempts");
            migrationBuilder.DropTable("UserLessonProgresses");
            migrationBuilder.DropTable("UserChallengeResults");
            migrationBuilder.DropTable("UserAchievements");
            migrationBuilder.DropTable("QuizQuestions");
            migrationBuilder.DropTable("Quizzes");
            migrationBuilder.DropTable("Lessons");
            migrationBuilder.DropTable("Challenges");
            migrationBuilder.DropTable("Courses");
            migrationBuilder.DropTable("LearningPaths");
            migrationBuilder.DropTable("Achievements");
            migrationBuilder.DropTable("Users");
        }
    }
}
