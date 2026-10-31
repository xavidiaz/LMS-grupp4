using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMS.API.Tests;

public abstract class ApiTestBase(ApiFactory factory)
{
    protected const string Teacher = "Teacher@Lms.com";
    protected const string TeacherB = "Anna.Larsson@Lms.com";
    protected const string Student = "Student@Lms.com";
    protected const string StudentB = "Student2@Lms.com";
    protected const string StudentC = "Maria.Garcia@Lms.com";

    protected HttpClient Anonymous() => factory.CreateClient();

    protected Task<HttpClient> LoginAsync(string email) =>
        LoginAsync(email, ApiFactory.SeedPassword);

    protected async Task<HttpClient> LoginAsync(string email, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var tokens = await ReadJsonAsync(response);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.GetProperty("accessToken").GetString());
        return client;
    }

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

    protected static async Task<int> ReadIdAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await ReadJsonAsync(response)).GetProperty("id").GetInt32();
    }

    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..(prefix.Length + 9)];

    protected static object CourseBody(string? name = null, string? start = null, string? end = null) => new
    {
        name = name ?? "Test course",
        code = Unique("T"),
        description = "Created by a test.",
        startDate = start ?? "2030-01-01T00:00:00",
        endDate = end ?? "2030-12-31T00:00:00"
    };

    protected static object ModuleBody(int courseId, string name = "Test module",
        string start = "2030-02-01T00:00:00", string end = "2030-02-28T00:00:00") => new
    {
        courseId,
        name,
        description = "Created by a test.",
        startDate = start,
        endDate = end
    };

    protected static object ActivityBody(int moduleId, int actionTypeId,
        string start = "2030-02-05T09:00:00", string end = "2030-02-05T12:00:00") => new
    {
        moduleId,
        actionTypeId,
        title = "Test activity",
        description = "Created by a test.",
        startDate = start,
        endDate = end
    };

    protected static object UserBody(int courseId, string? email = null, string firstName = "Test", string role = "Student") => new
    {
        firstName,
        lastName = "User",
        email = email ?? $"{Unique("user")}@lms.com",
        phoneNumber = "0701234567",
        role,
        courseId
    };

    protected static async Task<int> CreateCourseAsync(HttpClient teacher) =>
        await ReadIdAsync(await teacher.PostAsJsonAsync("/api/courses", CourseBody()));

    protected static async Task<int> CreateModuleAsync(HttpClient teacher, int courseId) =>
        await ReadIdAsync(await teacher.PostAsJsonAsync("/api/Module", ModuleBody(courseId)));

    protected static async Task<int> GetActionTypeIdAsync(HttpClient client)
    {
        var types = await ReadJsonAsync(await client.GetAsync("/api/ActionType"));
        return types.GetProperty("items")[0].GetProperty("id").GetInt32();
    }

    protected static async Task<int> CreateActivityAsync(HttpClient teacher, int moduleId) =>
        await ReadIdAsync(await teacher.PostAsJsonAsync("/api/Activity",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher))));

    protected static async Task<int> GetMyCourseIdAsync(HttpClient student) =>
        await ReadIdAsync(await student.GetAsync("/api/courses/my-course"));

    protected static async Task<int> GetFirstModuleIdAsync(HttpClient client, int courseId)
    {
        var modules = await ReadJsonAsync(await client.GetAsync($"/api/Module/course/{courseId}"));
        return modules.GetProperty("items")[0].GetProperty("id").GetInt32();
    }

    // The assignment in the first module of the student's own course
    protected async Task<int> GetAssignmentIdAsync(string studentEmail)
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await GetMyCourseIdAsync(await LoginAsync(studentEmail));
        var moduleId = await GetFirstModuleIdAsync(teacher, courseId);

        var activities = await ReadJsonAsync(await teacher.GetAsync($"/api/Activity/module/{moduleId}"));
        return activities.GetProperty("items").EnumerateArray()
            .First(a => a.GetProperty("actionTypeName").GetString() == "Assignment")
            .GetProperty("id").GetInt32();
    }

    protected static MultipartFormDataContent FileForm(string fileName, string field, int value)
    {
        var form = new MultipartFormDataContent();
        var file = new StringContent("Test file content");
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        form.Add(file, "file", fileName);
        form.Add(new StringContent(value.ToString()), field);
        return form;
    }

    protected static Task<HttpResponseMessage> SubmitAsync(HttpClient student, int activityId, string fileName = "answer.txt") =>
        student.PostAsync("/api/submissions", FileForm(fileName, "activityId", activityId));

    protected static Task<HttpResponseMessage> UploadDocumentAsync(HttpClient client, string field, int value, string fileName = "document.txt") =>
        client.PostAsync("/api/documents", FileForm(fileName, field, value));
}
