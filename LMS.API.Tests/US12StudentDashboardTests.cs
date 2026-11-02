using System.Net;
using System.Net.Http.Json;

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

    [Fact]
    public async Task Recent_feedback_names_the_assignment()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);
        var assignmentId = await GetAssignmentIdAsync(Student);
        var submissionId = await ReadIdAsync(await SubmitAsync(student, assignmentId));
        await teacher.PostAsJsonAsync($"/api/submissions/{submissionId}/feedback", new { comment = "Well done.", grade = 9 });

        var body = await ReadJsonAsync(await student.GetAsync(Url));
        var feedback = body.GetProperty("recentFeedback").EnumerateArray()
            .First(f => f.GetProperty("submissionId").GetInt32() == submissionId);

        Assert.Equal(assignmentId, feedback.GetProperty("activityId").GetInt32());
        Assert.False(string.IsNullOrEmpty(feedback.GetProperty("assignmentTitle").GetString()));
    }
}
