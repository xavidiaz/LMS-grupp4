using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US14NotificationsTests(ApiFactory factory) : ApiTestBase(factory)
{
    private const string Url = "/api/notifications";

    // Feedback on a submission creates a notification for the student
    private async Task<int> CreateNotificationAsync()
    {
        var assignmentId = await GetAssignmentIdAsync(Student);
        var student = await LoginAsync(Student);
        var teacher = await LoginAsync(Teacher);
        var submissionId = await ReadIdAsync(await SubmitAsync(student, assignmentId));
        (await teacher.PostAsJsonAsync($"/api/submissions/{submissionId}/feedback",
            new { comment = "Good work.", grade = 8 })).EnsureSuccessStatusCode();

        var notifications = await ReadJsonAsync(await student.GetAsync(Url));
        return notifications.EnumerateArray().Max(n => n.GetProperty("id").GetInt32());
    }

    private static JsonElement? Find(JsonElement notifications, int id)
    {
        foreach (var notification in notifications.EnumerateArray())
            if (notification.GetProperty("id").GetInt32() == id)
                return notification;
        return null;
    }

    [Fact]
    public async Task Student_sees_only_own_notifications()
    {
        var id = await CreateNotificationAsync();
        var student = await LoginAsync(Student);
        var otherStudent = await LoginAsync(StudentB);

        var own = await ReadJsonAsync(await student.GetAsync(Url));
        var other = await ReadJsonAsync(await otherStudent.GetAsync(Url));

        Assert.NotNull(Find(own, id));
        Assert.Null(Find(other, id));
    }

    [Fact]
    public async Task Notifications_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync(Url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Marking_someone_elses_notification_returns_404()
    {
        var id = await CreateNotificationAsync();
        var otherStudent = await LoginAsync(StudentB);

        var response = await otherStudent.PatchAsync($"{Url}/{id}/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Notification_that_does_not_exist_returns_404()
    {
        var student = await LoginAsync(Student);

        var response = await student.PatchAsync($"{Url}/999999/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Marking_without_token_returns_401()
    {
        var response = await Anonymous().PatchAsync($"{Url}/1/read", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Student_marks_notification_as_read()
    {
        var id = await CreateNotificationAsync();
        var student = await LoginAsync(Student);

        var response = await student.PatchAsync($"{Url}/{id}/read", null);
        var notifications = await ReadJsonAsync(await student.GetAsync(Url));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(Find(notifications, id)!.Value.GetProperty("isRead").GetBoolean());
    }
}
