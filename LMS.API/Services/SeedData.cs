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

    public static List<Course> Courses()
    {
        var start = DateTime.Today;

        return
        [
            CreateCourse("Course A", "COURSE-A", ".NET Fullstack", start,
                ["C# Basics", "Databases & EF Core", "ASP.NET Core API", "Blazor"]),
            CreateCourse("Course B", "COURSE-B", "Frontend Development", start,
                ["HTML & CSS", "JavaScript", "React"]),
            CreateCourse("Course C", "COURSE-C", "Cloud & DevOps", start.AddMonths(-1),
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
                day.AddHours(9), module.EndDate.AddHours(-7)));
        }

        return activities;
    }

    // Metadata only, the files do not exist on disk.
    public static List<Document> Documents(List<Course> courses, string teacherId)
    {
        var documents = new List<Document>();

        foreach (var course in courses)
        {
            documents.Add(new Document
            {
                Course = course,
                StoredFile = new StoredFile
                {
                    FileName = $"{course.Code}-schedule.pdf",
                    ContentType = "application/pdf",
                    StorageKey = $"uploads/seed/{course.Code}-schedule.pdf",
                },
                UploadAt = DateTime.Now.AddDays(-7),
                UploadedById = teacherId
            });

            foreach (var module in course.Modules)
            {
                documents.Add(new Document
                {
                    Module = module,
                    StoredFile = new StoredFile
                    {
                        FileName = $"{module.Name}-slides.pptx",
                        ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        StorageKey = $"uploads/seed/{course.Code}-{module.Id}-slides.pptx",
                    },
                    UploadAt = module.StartDate.AddDays(-1),
                    UploadedById = teacherId
                });
            }
        }

        return documents;
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

    // One graded submission per student. Submission has no ActivityId yet.
    public static List<FeedBack> SubmissionsWithFeedback(List<ApplicationUser> students, string teacherId)
    {
        return students.Select(student => new FeedBack
        {
            Submission = new Submission
            {
                UserId = student.Id,
                FileName = "intro-assignment.docx",
                FilePath = $"uploads/seed/{student.Id}-intro-assignment.docx",
                SubmittedAt = DateTime.Now.AddDays(-2),
                Status = SubmissionStatus.Submitted
            },
            UserId = teacherId,
            Comment = "Good work, clear structure.",
            Grade = 4,
            CreatedAt = DateTime.Now.AddDays(-1)
        }).ToList();
    }

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

    // Each module is two weeks long, one after another.
    private static Course CreateCourse(
        string name, string code, string description, DateTime start, string[] moduleNames)
    {
        var course = new Course
        {
            Name = name,
            Code = code,
            Description = description,
            StartDate = start,
            EndDate = start.AddDays(14 * moduleNames.Length)
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
