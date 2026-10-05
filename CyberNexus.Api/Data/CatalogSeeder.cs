using System.Text.Json;
using CyberNexus.Api.Data;
using CyberNexus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberNexus.Api.Data;

/// <summary>
/// Fills in the quiz / challenge / achievement catalogue on startup. Every
/// insert is skipped when the row already exists (matched on a stable key), so
/// running it on every boot is safe and never duplicates content.
/// </summary>
public static class CatalogSeeder
{
    public static void Seed(AppDbContext db)
    {
        SeedLearningPaths(db);
        SeedAchievements(db);
        SeedQuizzes(db);
        SeedChallenges(db);
        db.SaveChanges();
    }

    // ── Learning Paths + Courses + Lessons ───────────────────────────────────
    private static void SeedLearningPaths(AppDbContext db)
    {
        if (db.LearningPaths.Any()) return;

        var path = new LearningPath
        {
            TitleEn = "Network Security",
            TitleAr = "أمن الشبكات",
            DescriptionEn = "Master the fundamentals of network security from scratch.",
            DescriptionAr = "أتقن أساسيات أمن الشبكات من الصفر.",
            Category = "Network",
            Difficulty = "beginner"
        };

        var c1 = new Course
        {
            TitleEn = "Cybersecurity Fundamentals",
            TitleAr = "أساسيات الأمن السيبراني",
            DescriptionEn = "Core concepts every security professional must know.",
            DescriptionAr = "المفاهيم الأساسية التي يجب أن يعرفها كل محترف أمني.",
            Order = 1
        };
        c1.Lessons.AddRange(new[]
        {
            new Lesson { TitleEn="Introduction to Cybersecurity", TitleAr="مقدمة في الأمن السيبراني",
                ContentEn="Learn what cybersecurity is and why it matters.", ContentAr="تعلم ما هو الأمن السيبراني ولماذا يهم.", Order=1, XpReward=50 },
            new Lesson { TitleEn="CIA Triad", TitleAr="مثلث CIA",
                ContentEn="Confidentiality, Integrity, and Availability explained.", ContentAr="شرح السرية والنزاهة والتوفر.", Order=2, XpReward=50 },
            new Lesson { TitleEn="Types of Threats", TitleAr="أنواع التهديدات",
                ContentEn="Malware, phishing, DDoS and more.", ContentAr="البرامج الخبيثة والتصيد وهجمات DDoS والمزيد.", Order=3, XpReward=50 },
        });

        var c2 = new Course
        {
            TitleEn = "Network Security Basics",
            TitleAr = "أساسيات أمن الشبكات",
            DescriptionEn = "Firewalls, protocols, and network defense strategies.",
            DescriptionAr = "جدران الحماية والبروتوكولات واستراتيجيات الدفاع عن الشبكة.",
            Order = 2
        };
        c2.Lessons.AddRange(new[]
        {
            new Lesson { TitleEn="TCP/IP Fundamentals", TitleAr="أساسيات TCP/IP",
                ContentEn="How the internet protocols work together.", ContentAr="كيف تعمل بروتوكولات الإنترنت معاً.", Order=1, XpReward=75 },
            new Lesson { TitleEn="Firewalls and IDS", TitleAr="جدران الحماية وأنظمة الكشف",
                ContentEn="Protecting networks with firewalls and intrusion detection.", ContentAr="حماية الشبكات بجدران الحماية وأنظمة كشف التسلل.", Order=2, XpReward=75 },
        });

        var c3 = new Course
        {
            TitleEn = "Ethical Hacking",
            TitleAr = "الاختراق الأخلاقي",
            DescriptionEn = "Understand attacker techniques to defend better.",
            DescriptionAr = "افهم تقنيات المهاجمين للدفاع بشكل أفضل.",
            Order = 3
        };
        c3.Lessons.AddRange(new[]
        {
            new Lesson { TitleEn="SQL Injection", TitleAr="حقن SQL",
                ContentEn="How SQL injection works and how to prevent it.", ContentAr="كيف يعمل حقن SQL وكيف تمنعه.", Order=1, XpReward=100 },
            new Lesson { TitleEn="Cross-Site Scripting (XSS)", TitleAr="البرمجة النصية عبر المواقع",
                ContentEn="Understanding and preventing XSS attacks.", ContentAr="فهم هجمات XSS والوقاية منها.", Order=2, XpReward=100 },
            new Lesson { TitleEn="Social Engineering", TitleAr="الهندسة الاجتماعية",
                ContentEn="Human-based attacks and how to recognize them.", ContentAr="الهجمات البشرية وكيفية التعرف عليها.", Order=3, XpReward=100 },
        });

        path.Courses.AddRange(new[] { c1, c2, c3 });
        db.LearningPaths.Add(path);
        db.SaveChanges(); // save here so courseId is available for quiz seeding
    }

    private static void SeedAchievements(AppDbContext db)
    {
        // key, titleEn, titleAr, descEn, descAr, icon, xp, requiredLevel
        var rows = new (string Key, string En, string Ar, string DEn, string DAr, string Icon, int Xp, int Level)[]
        {
            ("first_lesson", "First Step", "الخطوة الأولى",
                "Completed your first lesson.", "أكملت أول درس.", "footprint", 50, 1),
            ("five_lessons", "Getting Serious", "بدا الجدية",
                "Completed five lessons.", "أكملت خمسة دروس.", "school", 100, 2),
            ("all_lessons", "Path Master", "سيد المسار",
                "Completed every lesson in the catalogue.", "أكملت كل الدروس.", "workspace_premium", 500, 1),
            ("first_quiz", "Quiz Taker", "خوض الاختبار",
                "Passed your first quiz.", "جحت أول اختبار.", "quiz", 100, 1),
            ("quiz_master", "Quiz Master", "سيد الاختبارات",
                "Passed five quizzes.", "جحت خمسة اختبارات.", "military_tech", 300, 3),
            ("first_challenge", "Lab Rookie", "مبتدئ المختبر",
                "Solved your first challenge.", "حللت أول تحدي.", "bug_report", 100, 1),
            ("challenge_veteran", "Lab Veteran", "محترف المختبر",
                "Solved ten challenges.", "حللت عشرة تحديات.", "shield", 400, 4),
            ("streak_3", "Warming Up", "تسخين",
                "Kept a three day streak.", "حافظت على سلسلة 3 أيام.", "local_fire_department", 100, 1),
            ("streak_7", "Unbreakable", "لا يُكسر",
                "Kept a seven day streak.", "حافظت على سلسلة 7 أيام.", "whatshot", 250, 2)
        };

        foreach (var r in rows)
        {
            if (db.Achievements.Any(a => a.Key == r.Key)) continue;
            db.Achievements.Add(new Achievement
            {
                Key = r.Key,
                TitleEn = r.En,
                TitleAr = r.Ar,
                DescriptionEn = r.DEn,
                DescriptionAr = r.DAr,
                Icon = r.Icon,
                XpReward = r.Xp,
                RequiredLevel = r.Level
            });
        }
    }

    private static void SeedQuizzes(AppDbContext db)
    {
        // Find the course to hang the quizzes off, so the seed works on a real DB.
        var courseId = db.Courses.OrderBy(c => c.Order).Select(c => (int?)c.Id).FirstOrDefault();
        if (courseId == null) return;

        if (db.Quizzes.Any()) return;

        var quiz = new Quiz
        {
            TitleEn = "Network Security Basics",
            TitleAr = "أساسيات أمن الشبكات",
            DescriptionEn = "Five questions covering the fundamentals.",
            DescriptionAr = "خمسة أسئلة تغطي الأساسيات.",
            Order = 1,
            PassingScore = 60,
            XpReward = 300,
            CourseId = courseId.Value
        };

        // questionEn, questionAr, options[], correctIndex, expEn, expAr
        quiz.Questions.AddRange(
            NewQ(1, "What does the CIA Triad stand for?",
                     "ماذا يعني مثلث CIA؟",
                     new[] { "Confidentiality, Integrity, Availability", "Control, Integrity, Access", "Cyber, Intel, Audit", "Compute, Integrate, Access" }, 0,
                     "C-I-A is the foundation of all security goals.", "C-I-A هو أساس كل أهداف الأمن."),
            NewQ(2, "Which port does HTTPS use by default?",
                     "أي بورت يستخدمه HTTPS افتراضياً؟",
                     new[] { "21", "80", "443", "8080" }, 2,
                     "HTTPS runs over TCP 443.", "HTTPS يعمل على المنفذ 443."),
            NewQ(3, "What is a zero-day vulnerability?",
                     "ما هي ثغرة Zero-Day؟",
                     new[] { "A bug fixed the same day", "A known flaw with no patch available", "A bug from legacy systems", "A bug found at release" }, 1,
                     "Zero-day means the vendor has no fix ready yet.", "Zero-day تعني أن الشركة لا تملك إصلاحاً بعد."),
            NewQ(4, "SQL injection targets which part of a system?",
                     "يستهدف حقن SQL أي جزء من النظام؟",
                     new[] { "The client CSS", "The database query layer", "The image CDN", "The DNS resolver" }, 1,
                     "It works by getting user input treated as SQL.", "يعمل عندما يُعامل مُدخل المستخدم كـ SQL."),
            NewQ(5, "Which is a network layer protocol?",
                     "أي بروتوكول يعمل في طبقة الشبكة؟",
                     new[] { "HTTP", "TCP", "JPEG", "HTML" }, 1,
                     "TCP operates at the transport layer.", "TCP يعمل في طبقة النقل.")
        );

        db.Quizzes.Add(quiz);
    }

    private static QuizQuestion NewQ(int order, string en, string ar, string[] opts, int correct, string expEn, string expAr) => new()
    {
        Order = order,
        QuestionEn = en,
        QuestionAr = ar,
        OptionsJson = JsonSerializer.Serialize(opts),
        CorrectIndex = correct,
        ExplanationEn = expEn,
        ExplanationAr = expAr
    };

    private static void SeedChallenges(AppDbContext db)
    {
        if (db.Challenges.Any()) return;

        // titleEn, titleAr, descEn, descAr, category, difficulty, xp, minutes, order, reqLevel, options, correct, expEn, expAr
        var rows = new (string En, string Ar, string DEn, string DAr, string Cat, string Diff, int Xp, int Min, int Order, int Req, string[] Opts, int Correct, string ExpEn, string ExpAr)[]
        {
            ("Phishing Detector", "كاشف التصيّد",
                "Spot the social engineering red flags in a suspicious message.",
                "اكشف علامات الهندسة الاجتماعية في رسالة مشبوهة.",
                "phishing", "beginner", 200, 5, 1, 1,
                new[] { "Check the sender domain carefully", "Ignore it if it looks official", "Reply asking for details", "Forward to everyone" }, 0,
                "Always verify the real sender domain, not the display name.", "تحقق دائماً من نطاق المرسل الحقيقي، لا الاسم الظاهر."),

            ("Weak Password Audit", "تدقيق كلمات المرور",
                "Identify why a password list is vulnerable.",
                "حدّد سبب ضعف قائمة كلمات المرور.",
                "crypto", "beginner", 250, 5, 2, 1,
                new[] { "They reuse one password", "They are hashed with bcrypt", "They are long", "They include symbols" }, 0,
                "Reuse across services is the real risk, not the length itself.", "إعادة الاستخدام بين الخدمات هي الخطر، لا الطول بحد ذاته."),

            ("Firewall Under Attack", "جدار ناري تحت الهجوم",
                "Pick the mitigation that keeps critical services alive during a flood.",
                "اختر التخفيف الذي يبقي الخدمات الحرجة أثناء الفيضان.",
                "network", "intermediate", 500, 10, 3, 2,
                new[] { "Block all inbound traffic", "Rate-limit and drop known-bad signatures", "Disable DNS", "Open the firewall fully" }, 1,
                "Targeted rate-limiting beats a blanket block that also kills real traffic.", "تحديد المعدل أفضل من حظر شامل يقتل الترافيك الشرعي أيضاً."),

            ("Ransomware Triage", "فرز برامج الفدية",
                "Choose the safest first move on an infected host.",
                "اختر أأمن خطوة أولى على جهاز مصاب.",
                "malware", "intermediate", 600, 10, 4, 3,
                new[] { "Pay the ransom", "Isolate the host from the network", "Reboot immediately", "Delete all temp files" }, 1,
                "Isolate first to stop lateral spread; preserve evidence before wiping.", "اعزل الجهاز أولاً لمنع الانتشار، واحتفظ بالأدلة قبل المسح.")
        };

        foreach (var r in rows)
        {
            db.Challenges.Add(new Challenge
            {
                TitleEn = r.En,
                TitleAr = r.Ar,
                DescriptionEn = r.DEn,
                DescriptionAr = r.DAr,
                Category = r.Cat,
                Difficulty = r.Diff,
                XpReward = r.Xp,
                EstimatedMinutes = r.Min,
                Order = r.Order,
                RequiredLevel = r.Req,
                OptionsJson = JsonSerializer.Serialize(r.Opts),
                CorrectIndex = r.Correct,
                ExplanationEn = r.ExpEn,
                ExplanationAr = r.ExpAr
            });
        }
    }
}
