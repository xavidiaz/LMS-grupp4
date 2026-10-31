using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US09SubmissionsTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Teacher_tries_to_submit_returns_403()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        var teacher = await LoginAsync(Teacher);

        var response = await SubmitAsync(teacher, assignmentId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Student_submits_on_time_returns_Submitted()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        var student = await LoginAsync(Student);

        var response = await SubmitAsync(student, assignmentId);
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Submitted", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Student_submits_after_deadline_returns_Late()
    {
        var assignmentId = await GetAssignmentIdAsync(StudentC);
        var student = await LoginAsync(StudentC);

        var response = await SubmitAsync(student, assignmentId);
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Late", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Resubmitting_lists_the_newest_first()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        var student = await LoginAsync(Student);
        await SubmitAsync(student, assignmentId, "first.txt");
        await SubmitAsync(student, assignmentId, "second.txt");

        var response = await student.GetAsync($"/api/submissions/activity/{assignmentId}/mine");
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("second.txt", body[0].GetProperty("fileName").GetString());
    }

    [Fact]
    public async Task No_submission_returns_an_empty_list()
    {
        var assignmentId = await GetAssignmentIdAsync(StudentB);
        var student = await LoginAsync(Student);

        var response = await student.GetAsync($"/api/submissions/activity/{assignmentId}/mine");
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, body.GetArrayLength());
    }

    [Fact]
    public async Task Forbidden_file_type_returns_400()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        var student = await LoginAsync(Student);

        var response = await SubmitAsync(student, assignmentId, "virus.exe");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Another_courses_assignment_returns_404()
    {
        var assignmentId = await GetAssignmentIdAsync(StudentB);
        var student = await LoginAsync(Student);

        var response = await SubmitAsync(student, assignmentId);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
