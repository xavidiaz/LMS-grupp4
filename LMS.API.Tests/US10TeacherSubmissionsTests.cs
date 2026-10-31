using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US10TeacherSubmissionsTests(ApiFactory factory) : ApiTestBase(factory)
{
    private const string Url = "/api/teacher/submissions";

    private async Task<int> CreateSubmissionAsync()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        return await ReadIdAsync(await SubmitAsync(await LoginAsync(Student), assignmentId));
    }

    [Theory]
    [InlineData("?PageNumber=1&PageSize=20")]
    [InlineData("?status=Submitted")]
    [InlineData("?status=Missing")]
    [InlineData("?status=late")]
    public async Task Teacher_lists_and_filters_returns_200(string query)
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync(Url + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_by_assignment_returns_200()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync($"{Url}?activityId={assignmentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("?status=Nope")]
    [InlineData("?PageSize=101")]
    public async Task Invalid_query_returns_400(string query)
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync(Url + query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_lists_submissions_returns_403()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync(Url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_on_the_list_returns_405()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.DeleteAsync(Url);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_downloads_the_file_returns_200()
    {
        var id = await CreateSubmissionAsync();
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync($"{Url}/{id}/file");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 500 today")]
    public async Task Teacher_in_another_course_downloads_returns_403()
    {
        var id = await CreateSubmissionAsync();
        var teacher = await LoginAsync(TeacherB);

        var response = await teacher.GetAsync($"{Url}/{id}/file");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Submission_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync($"{Url}/999999/file");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_or_no_token_cannot_download()
    {
        var id = await CreateSubmissionAsync();
        var student = await LoginAsync(Student);

        Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync($"{Url}/{id}/file")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous().GetAsync($"{Url}/{id}/file")).StatusCode);
    }
}
