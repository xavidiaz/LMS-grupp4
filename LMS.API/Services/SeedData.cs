using System.Text;

namespace LMS.API.Services;

// Development test data used by DataSeedService.
internal static class SeedData
{
    public const string TeacherEmail = "Teacher@Lms.com";

    public static readonly (string Email, string FirstName, string LastName, string CourseCode)[] Teachers =
    [
        (TeacherEmail, "Test", "Teacher", "COURSE-A"),
        ("Anna.Larsson@Lms.com", "Anna", "Larsson", "COURSE-B"),
    ];

    // CourseCode must match a code in Courses().
    public static readonly (string Email, string FirstName, string LastName, string CourseCode)[] Students =
    [
        ("Student@Lms.com", "Test", "Student", "COURSE-A"),
        ("Student2@Lms.com", "Test", "Student2", "COURSE-B"),
        ("Erik.Johansson@Lms.com", "Erik", "Johansson", "COURSE-A"),
        ("Sara.Nilsson@Lms.com", "Sara", "Nilsson", "COURSE-A"),
        ("Omar.Hassan@Lms.com", "Omar", "Hassan", "COURSE-A"),
        ("Lina.Berg@Lms.com", "Lina", "Berg", "COURSE-B"),
        ("Johan.Svensson@Lms.com", "Johan", "Svensson", "COURSE-B"),
        ("Maria.Garcia@Lms.com", "Maria", "Garcia", "COURSE-C"),
        ("David.Lind@Lms.com", "David", "Lind", "COURSE-C"),
    ];

    // Students with a submission to the first assignment in their course.
    // Student@Lms.com has none, so the demo can show a live upload.
    public static readonly (string Email, bool WithFeedback)[] SubmittingStudents =
    [
        ("Erik.Johansson@Lms.com", true),
        ("Sara.Nilsson@Lms.com", false),
        ("Lina.Berg@Lms.com", true),
        ("Student2@Lms.com", false),
        ("Maria.Garcia@Lms.com", true),
    ];

    public static List<Course> Courses()
    {
        // A week in, so the first assignment is open and classmates have submitted
        var start = DateTime.Today.AddDays(-7);

        return
        [
            // Two free weeks at the end, so a module can be created in the demo
            CreateCourse("Course A", "COURSE-A", ".NET Fullstack", start,
                ["C# Basics", "Databases & EF Core", "ASP.NET Core API", "Blazor"], freeWeeks: 2),
            CreateCourse("Course B", "COURSE-B", "Frontend Development", start,
                ["HTML & CSS", "JavaScript", "React"]),
            CreateCourse("Course C", "COURSE-C", "Cloud & DevOps", DateTime.Today.AddMonths(-1),
                ["Git & GitHub", "Azure", "CI/CD"]),
        ];
    }

    public static List<ActionType> ActionTypes() =>
    [
        new() { Name = "Lecture" },
        new() { Name = "E-learning" },
        new() { Name = "Exercise" },
        new() { Name = "Assignment" },
    ];

    // Per module: a lecture day 1, e-learning day 2, exercises day 3 and an
    // assignment with its deadline at the end of the module.
    public static List<Activity> Activities(List<Course> courses, List<ActionType> types)
    {
        var activities = new List<Activity>();

        foreach (var module in courses.SelectMany(c => c.Modules))
        {
            var day = module.StartDate;

            activities.Add(CreateActivity(module, types[0], $"{module.Name} - Lecture",
                day.AddHours(9), day.AddHours(12)));
            activities.Add(CreateActivity(module, types[1], $"{module.Name} - E-learning",
                day.AddDays(1).AddHours(9), day.AddDays(1).AddHours(16)));
            activities.Add(CreateActivity(module, types[2], $"{module.Name} - Exercises",
                day.AddDays(2).AddHours(13), day.AddDays(2).AddHours(16)));
            activities.Add(CreateActivity(module, types[3], $"{module.Name} - Assignment",
                day.AddDays(3).AddHours(9), module.EndDate.AddHours(-7)));
        }

        return activities;
    }

    // DataSeedService writes FileContent(FileName) to each StorageKey.
    public static List<Document> Documents(List<Course> courses, string teacherId)
    {
        var documents = new List<Document>();

        foreach (var course in courses)
        {
            documents.Add(CreateDocument(
                course, null, $"{course.Code}-schedule.pdf",
                $"seed/{course.Code}-schedule.pdf",
                course.StartDate.AddDays(-7), teacherId));

            foreach (var module in course.Modules)
            {
                documents.Add(CreateDocument(
                    null, module, $"{module.Name}-slides.pdf",
                    $"seed/{course.Code}-{module.Id}-slides.pdf",
                    module.StartDate.AddDays(-1), teacherId));
            }
        }

        return documents;
    }

    // A one-page PDF that shows the file name, so downloads open in a viewer.
    public static byte[] FileContent(string fileName)
    {
        var text = fileName.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var content = $"BT /F1 18 Tf 72 720 Td ({text}) Tj ET";

        string[] objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
        ];

        // ASCII only, so the string length equals the byte offset
        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int>();

        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(pdf.Length);
            pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        var xref = pdf.Length;
        pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");

        foreach (var offset in offsets)
        {
            pdf.Append($"{offset:D10} 00000 n \n");
        }

        pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    public static List<Notification> Notifications(List<ApplicationUser> students)
    {
        var notifications = new List<Notification>();

        foreach (var student in students)
        {
            notifications.Add(new Notification
            {
                UserId = student.Id,
                Message = "Welcome to Lexicon LMS!",
                CreatedAt = DateTime.Now.AddDays(-3),
                IsRead = true
            });
            notifications.Add(new Notification
            {
                UserId = student.Id,
                Message = "A new document has been uploaded to your course.",
                CreatedAt = DateTime.Now.AddHours(-2),
                IsRead = false
            });
        }

        return notifications;
    }

    // A submission to the first assignment in the student's course, submitted
    // on time. Feedback comes from the course teacher, or the first teacher.
    public static (List<Submission> Submissions, List<FeedBack> Feedbacks) SubmissionsWithFeedback(
        List<ApplicationUser> students, List<ApplicationUser> teachers, List<Activity> activities)
    {
        var submissions = new List<Submission>();
        var feedbacks = new List<FeedBack>();

        foreach (var (email, withFeedback) in SubmittingStudents)
        {
            var student = students.Single(s => s.Email == email);
            var assignment = activities
                .Where(a => a.Module!.CourseId == student.CourseId && a.ActionType.Name == "Assignment")
                .OrderBy(a => a.StartDate)
                .First();

            var submittedAt = Min(DateTime.Now.AddDays(-1), assignment.EndDate.AddDays(-1));
            var submission = new Submission
            {
                UserId = student.Id,
                Activity = assignment,
                FileName = "assignment-answer.pdf",
                FilePath = $"seed/{student.Id}-assignment-answer.pdf",
                SubmittedAt = submittedAt,
                Status = SubmissionStatus.Submitted
            };
            submissions.Add(submission);

            if (withFeedback)
            {
                var teacher = teachers.FirstOrDefault(t => t.CourseId == student.CourseId) ?? teachers[0];

                feedbacks.Add(new FeedBack
                {
                    Submission = submission,
                    UserId = teacher.Id,
                    Comment = "Good work, clear structure.",
                    Grade = 4,
                    CreatedAt = submittedAt.AddHours(20)
                });
            }
        }

        return (submissions, feedbacks);
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static Document CreateDocument(
        Course? course, Module? module, string fileName, string storageKey,
        DateTime uploadAt, string teacherId) => new()
        {
            Course = course,
            Module = module,
            StoredFile = new StoredFile
            {
                FileName = fileName,
                ContentType = "application/pdf",
                StorageKey = storageKey,
                Size = FileContent(fileName).Length
            },
            UploadAt = uploadAt,
            UploadedById = teacherId
        };

    private static Activity CreateActivity(
        Module module, ActionType type, string title, DateTime start, DateTime end) => new()
        {
            Module = module,
            ActionType = type,
            Title = title,
            Description = $"{type.Name} in {module.Name}",
            StartDate = start,
            EndDate = end
        };

    // Each module is two weeks long, one after another. freeWeeks are left
    // without modules at the end of the course.
    private static Course CreateCourse(
        string name, string code, string description, DateTime start, string[] moduleNames,
        int freeWeeks = 0)
    {
        var course = new Course
        {
            Name = name,
            Code = code,
            Description = description,
            StartDate = start,
            EndDate = start.AddDays(14 * moduleNames.Length + 7 * freeWeeks)
        };

        for (int i = 0; i < moduleNames.Length; i++)
        {
            course.Modules.Add(new Module
            {
                Name = moduleNames[i],
                Description = $"{moduleNames[i]} in {description}",
                StartDate = start.AddDays(14 * i),
                EndDate = start.AddDays(14 * (i + 1))
            });
        }

        return course;
    }
}
