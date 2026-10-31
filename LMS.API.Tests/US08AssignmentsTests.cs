using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US08AssignmentsTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Student_lists_assignments_returns_200()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync("/api/Activity/my-assignments?PageNumber=1&PageSize=100");
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(0, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Page_size_limits_the_rows()
    {
        var student = await LoginAsync(Student);

        var body = await ReadJsonAsync(await student.GetAsync("/api/Activity/my-assignments?PageNumber=1&PageSize=1"));

        Assert.Equal(1, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Page_size_above_the_limit_returns_400()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync("/api/Activity/my-assignments?PageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Assignments_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/Activity/my-assignments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
