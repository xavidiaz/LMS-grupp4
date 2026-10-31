using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

//ToDo: Add in UserSecrets (you can change password and secretkey):
//{
//  "password": "abc",
//  "JwtSettings": {
//    "secretkey" :  "ThisMustNeReallyLong!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"
//  }
//}

internal class DataSeedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DataSeedService> logger;
    private UserManager<ApplicationUser> userManager = null!;
    private RoleManager<IdentityRole> roleManager = null!;
    private string _password = null!;

    private const string DemoRole = "Demo";
    private const string DefaultUserEmail = "DemoUser@Lms.com";
    private const string TeacherEmail = SeedData.TeacherEmail;

    public DataSeedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DataSeedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (!env.IsDevelopment())
            return;

        ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            ?? throw new ArgumentNullException();

        // Start with: dotnet run --project LMS.API -- --reseed=true
        // Creates the schema from the current model, not from migrations.
        if (configuration.GetValue<bool>("reseed"))
        {
            logger.LogWarning("Reseed: dropping and recreating the database");
            await context.Database.EnsureDeletedAsync(cancellationToken);
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (await context.Users.AnyAsync(u => u.Email == TeacherEmail, cancellationToken) &&
            await context.Roles.CountAsync(cancellationToken) >= 3)
            return;

        userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
            ?? throw new ArgumentNullException();

        roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()
            ?? throw new ArgumentNullException();

        _password = configuration["password"]!;

        ArgumentNullException.ThrowIfNull(_password, nameof(_password));

        try
        {
            await CreateRolesAsync([DemoRole, "Student", "Teacher"]);
            await CreateDefaultUserAsync();
            await CreateCoursesAndUsersAsync(context);

            logger.LogInformation("Seed complete");
        }
        catch (Exception ex)
        {
            logger.LogError(
                $"Data seed fail with message: {ex.Message}. Exception: {ex.InnerException}");

            throw;
        }
    }

    private async Task CreateRolesAsync(string[] rolenames)
    {
        foreach (string rolename in rolenames)
        {
            if (await roleManager.RoleExistsAsync(rolename))
                continue;

            var role = new IdentityRole
            {
                Name = rolename
            };

            var res = await roleManager.CreateAsync(role);

            if (!res.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        "\n",
                        res.Errors.Select(e => $"{e.Code}: {e.Description}")));
            }
        }
    }

    private async Task CreateDefaultUserAsync()
    {
        var existingUser =
            await userManager.FindByEmailAsync(DefaultUserEmail);

        if (existingUser is not null)
            return;

        var user = new ApplicationUser
        {
            Email = DefaultUserEmail,
            UserName = DefaultUserEmail,
            CourseId = null,
            FirstName = "Demo",
            LastName = "User",
            Course = null
        };

        await CreateUserAsync(user, DemoRole);
    }

    private async Task CreateCoursesAndUsersAsync(ApplicationDbContext context)
    {
        if (await userManager.FindByEmailAsync(TeacherEmail) is not null)
            return;

        var courses = SeedData.Courses();
        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        var teachers = new List<ApplicationUser>();
        foreach (var (email, firstName, lastName, courseCode) in SeedData.Teachers)
        {
            var teacher = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                CourseId = courses.Single(c => c.Code == courseCode).Id
            };
            await CreateUserAsync(teacher, "Teacher");
            teachers.Add(teacher);
        }

        var students = new List<ApplicationUser>();
        foreach (var (email, firstName, lastName, courseCode) in SeedData.Students)
        {
            var student = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                CourseId = courses.Single(c => c.Code == courseCode).Id
            };
            await CreateUserAsync(student, "Student");
            students.Add(student);
        }

        var actionTypes = SeedData.ActionTypes();
        context.AddRange(actionTypes);
        context.Activities.AddRange(SeedData.Activities(courses, actionTypes));
        context.Documents.AddRange(SeedData.Documents(courses, teachers[0].Id));
        context.Notifications.AddRange(SeedData.Notifications(students));
        context.Feedbacks.AddRange(SeedData.SubmissionsWithFeedback(students, teachers[0].Id));
        await context.SaveChangesAsync();
    }

    private async Task CreateUserAsync(
        ApplicationUser user,
        string role)
    {
        var result =
            await userManager.CreateAsync(user, _password);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(
                    "\n",
                    result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }

        var roleResult =
            await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            throw new Exception(
                string.Join(
                    "\n",
                    roleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
