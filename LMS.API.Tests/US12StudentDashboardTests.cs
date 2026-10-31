using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US12StudentDashboardTests(ApiFactory factory) : ApiTestBase(factory)
{
    private const string Url = "/api/dashboard/student";

    [Fact]
    public async Task Student_reads_dashboard_returns_200()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_reads_dashboard_returns_403()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync(Url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
