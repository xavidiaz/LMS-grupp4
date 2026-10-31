using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US16SecurityTests(ApiFactory factory) : ApiTestBase(factory)
{
    private async Task<JsonElement> LoginTokensAsync()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/login",
            new { email = Student, password = ApiFactory.SeedPassword });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadJsonAsync(response);
    }

    // A module and an activity in Course A, read by the teacher
    private async Task<(int CourseId, int ModuleId, int ActivityId)> GetCourseADataAsync()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await GetMyCourseIdAsync(await LoginAsync(Student));
        var moduleId = await GetFirstModuleIdAsync(teacher, courseId);
        var activities = await ReadJsonAsync(await teacher.GetAsync($"/api/Activity/module/{moduleId}"));
        return (courseId, moduleId, activities.GetProperty("items")[0].GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Courses_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/courses");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Student_logs_in_returns_200()
    {
        var tokens = await LoginTokensAsync();

        Assert.False(string.IsNullOrEmpty(tokens.GetProperty("accessToken").GetString()));
    }

    [Theory]
    [InlineData(Student)]
    [InlineData("nobody@lms.com")]
    public async Task Wrong_login_returns_401(string email)
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/login", new { email, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Missing_password_returns_400()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/auth/login", new { email = Student });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_reads_own_claims_returns_200()
    {
        var student = await LoginAsync(Student);

        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync("/api/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync("/api/demo-auth")).StatusCode);
    }

    [Fact]
    public async Task Me_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_token_returns_401()
    {
        var client = Anonymous();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-token");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_refresh_token_returns_401()
    {
        var tokens = await LoginTokensAsync();

        var response = await Anonymous().PostAsJsonAsync("/api/token/refresh", new
        {
            accessToken = tokens.GetProperty("accessToken").GetString(),
            refreshToken = "wrong-token",
            accessTokenExpiresAtUtc = tokens.GetProperty("accessTokenExpiresAtUtc").GetString(),
            refreshTokenExpiresAtUtc = tokens.GetProperty("refreshTokenExpiresAtUtc").GetString()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refreshed_token_works()
    {
        var tokens = await LoginTokensAsync();

        var refresh = await Anonymous().PostAsJsonAsync("/api/token/refresh", new
        {
            accessToken = tokens.GetProperty("accessToken").GetString(),
            refreshToken = tokens.GetProperty("refreshToken").GetString(),
            accessTokenExpiresAtUtc = tokens.GetProperty("accessTokenExpiresAtUtc").GetString(),
            refreshTokenExpiresAtUtc = tokens.GetProperty("refreshTokenExpiresAtUtc").GetString()
        });
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);

        var client = Anonymous();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            (await ReadJsonAsync(refresh)).GetProperty("accessToken").GetString());

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/courses")]
    [InlineData("/api/dashboard/teacher")]
    [InlineData("/api/teacher/submissions")]
    public async Task Student_on_teacher_endpoint_returns_403(string url)
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/dashboard/student")]
    [InlineData("/api/courses/my-course")]
    [InlineData("/api/submissions/activity/1/mine")]
    public async Task Teacher_on_student_endpoint_returns_403(string url)
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync(url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Student_reads_module_in_another_course_returns_403()
    {
        var (_, moduleId, _) = await GetCourseADataAsync();
        var otherStudent = await LoginAsync(StudentB);

        var response = await otherStudent.GetAsync($"/api/Module/{moduleId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 200 today")]
    public async Task Student_reads_module_with_course_in_another_course_returns_403()
    {
        var (_, moduleId, _) = await GetCourseADataAsync();
        var otherStudent = await LoginAsync(StudentB);

        var response = await otherStudent.GetAsync($"/api/Module/{moduleId}/course");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 200 today")]
    public async Task Student_lists_modules_in_another_course_returns_403()
    {
        var (courseId, _, _) = await GetCourseADataAsync();
        var otherStudent = await LoginAsync(StudentB);

        var response = await otherStudent.GetAsync($"/api/Module/course/{courseId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 200 today")]
    public async Task Student_reads_activity_in_another_course_returns_403()
    {
        var (_, _, activityId) = await GetCourseADataAsync();
        var otherStudent = await LoginAsync(StudentB);

        var response = await otherStudent.GetAsync($"/api/Activity/{activityId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 200 today")]
    public async Task Student_lists_activities_in_another_course_returns_403()
    {
        var (_, moduleId, _) = await GetCourseADataAsync();
        var otherStudent = await LoginAsync(StudentB);

        var response = await otherStudent.GetAsync($"/api/Activity/module/{moduleId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
