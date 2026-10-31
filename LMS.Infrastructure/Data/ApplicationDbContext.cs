using Domain.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();

    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActionType> ActionTypes => Set<ActionType>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    public DbSet<FeedBack> Feedbacks => Set<FeedBack>();

    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
             typeof(ApplicationDbContext).Assembly);
    }
}
