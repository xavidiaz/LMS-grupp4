using System.Net;
using System.Net.Http.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US11FeedbackTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static readonly object ValidFeedback = new { comment = "Good work.", grade = 8.5 };

    private async Task<int> CreateSubmissionAsync()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        return await ReadIdAsync(await SubmitAsync(await LoginAsync(Student), assignmentId));
    }

    [Fact]
    public async Task Teacher_gives_feedback_and_student_reads_it()
    {
        var id = await CreateSubmissionAsync();
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);

        var create = await teacher.PostAsJsonAsync($"/api/submissions/{id}/feedback", ValidFeedback);
        var forTeacher = await teacher.GetAsync($"/api/submissions/{id}/feedback");
        var forStudent = await student.GetAsync($"/api/submissions/{id}/feedback/mine");
        var body = await ReadJsonAsync(forStudent);

        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        Assert.Equal(HttpStatusCode.OK, forTeacher.StatusCode);
        Assert.Equal(HttpStatusCode.OK, forStudent.StatusCode);
        Assert.False(string.IsNullOrEmpty(body[0].GetProperty("teacherName").GetString()));
    }

    [Fact]
    public async Task Grade_out_of_range_returns_400()
    {
        var id = await CreateSubmissionAsync();
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync($"/api/submissions/{id}/feedback", new { comment = "Too high", grade = 11 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Empty_comment_returns_400()
    {
        var id = await CreateSubmissionAsync();
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync($"/api/submissions/{id}/feedback", new { comment = "", grade = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submission_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync("/api/submissions/999999/feedback", ValidFeedback);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_gives_feedback_returns_403()
    {
        var id = await CreateSubmissionAsync();
        var student = await LoginAsync(Student);

        var response = await student.PostAsJsonAsync($"/api/submissions/{id}/feedback", ValidFeedback);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Another_student_cannot_read_the_feedback()
    {
        var id = await CreateSubmissionAsync();
        var otherStudent = await LoginAsync(StudentB);

        var mine = await otherStudent.GetAsync($"/api/submissions/{id}/feedback/mine");
        var teacherEndpoint = await otherStudent.GetAsync($"/api/submissions/{id}/feedback");

        Assert.Equal(HttpStatusCode.NotFound, mine.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, teacherEndpoint.StatusCode);
    }
}
