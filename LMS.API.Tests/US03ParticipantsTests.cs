using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US03ParticipantsTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Student_sees_own_course_participants_only()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync("/api/courses/my-course/participants");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Erik", body);
        Assert.DoesNotContain("Lina", body);
    }

    [Fact]
    public async Task Teacher_sees_participants_of_a_course()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await GetMyCourseIdAsync(await LoginAsync(Student));

        var response = await teacher.GetAsync($"/api/courses/{courseId}/participants");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Erik", body);
        Assert.DoesNotContain("Lina", body);
    }

    [Fact]
    public async Task Student_calls_course_participants_returns_403()
    {
        var student = await LoginAsync(Student);
        var courseId = await GetMyCourseIdAsync(student);

        var response = await student.GetAsync($"/api/courses/{courseId}/participants");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participants_of_unknown_course_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/courses/999999/participants");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_participant_list_has_no_user_ids()
    {
        var student = await LoginAsync(Student);

        var body = await ReadJsonAsync(await student.GetAsync("/api/courses/my-course/participants"));

        Assert.All(body.EnumerateArray(), p =>
            Assert.Equal(System.Text.Json.JsonValueKind.Null, p.GetProperty("id").ValueKind));
    }

    [Fact]
    public async Task Teacher_reads_student_overview()
    {
        var teacher = await LoginAsync(Teacher);
        var studentId = await GetParticipantIdAsync(teacher, Student);

        var response = await teacher.GetAsync($"/api/students/{studentId}/overview");
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Student, body.GetProperty("student").GetProperty("email").GetString());
        Assert.NotEqual(0, body.GetProperty("modules").GetArrayLength());
        Assert.NotEqual(0, body.GetProperty("assignments").GetArrayLength());
    }

    [Fact]
    public async Task Student_reads_student_overview_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var studentId = await GetParticipantIdAsync(teacher, Student);

        var response = await (await LoginAsync(Student)).GetAsync($"/api/students/{studentId}/overview");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("does-not-exist")]
    [InlineData(Teacher)]
    public async Task Overview_of_unknown_user_or_teacher_returns_404(string who)
    {
        var teacher = await LoginAsync(Teacher);
        var id = who == Teacher ? await GetParticipantIdAsync(teacher, Teacher) : who;

        var response = await teacher.GetAsync($"/api/students/{id}/overview");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // The user id from the teacher's participant list of Student's course
    private async Task<string> GetParticipantIdAsync(HttpClient teacher, string email)
    {
        var courseId = await GetMyCourseIdAsync(await LoginAsync(Student));
        var body = await ReadJsonAsync(await teacher.GetAsync($"/api/courses/{courseId}/participants"));

        return body.EnumerateArray()
            .First(p => p.GetProperty("email").GetString() == email)
            .GetProperty("id").GetString()!;
    }

    [Fact]
    public async Task Teacher_calls_my_course_returns_403()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/courses/my-course");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_calls_participants_returns_403()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/courses/my-course/participants");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task My_course_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/courses/my-course");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Participants_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/courses/my-course/participants");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
