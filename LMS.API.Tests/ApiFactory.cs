using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Tests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string SeedPassword = "Test1!Password";

    private readonly string _connectionString;
    private readonly string _uploadFolder = Path.Combine(Path.GetTempPath(), $"LMS_Tests_{Guid.NewGuid():N}");

    public ApiFactory()
    {
        var server = Environment.GetEnvironmentVariable("TEST_SQL_CONNECTION")
            ?? throw new InvalidOperationException("Set TEST_SQL_CONNECTION to a SQL Server connection string.");

        // Always a new database, so the tests never touch the LMS development database
        _connectionString = new SqlConnectionStringBuilder(server)
        {
            InitialCatalog = $"LMS_Tests_{Guid.NewGuid():N}"
        }.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        // Program reads these before the test host can override its configuration
        Environment.SetEnvironmentVariable("ConnectionStrings__ApplicationDbContext", _connectionString);
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "test-only-secret-key-at-least-32-characters");
        Environment.SetEnvironmentVariable("password", SeedPassword);
        Environment.SetEnvironmentVariable("FileStorage__RootPath", _uploadFolder);

        // The seed step runs when the API starts, so the tables must exist first
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    // The seed step only runs in Development
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development");

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();

        if (Directory.Exists(_uploadFolder))
            Directory.Delete(_uploadFolder, recursive: true);
    }

    private ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options);
}
