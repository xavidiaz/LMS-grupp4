using System.Net;
using System.Net.Http.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US05ActivitiesTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static async Task<int> CreateTestModuleAsync(HttpClient teacher) =>
        await CreateModuleAsync(teacher, await CreateCourseAsync(teacher));

    [Fact]
    public async Task Teacher_and_student_list_activity_types_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync("/api/ActionType")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync("/api/ActionType")).StatusCode);
    }

    [Fact]
    public async Task Activity_types_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/ActionType");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_creates_activity_returns_201()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Activity",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Overlapping_dates_returns_409()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);
        await CreateActivityAsync(teacher, moduleId);

        var response = await teacher.PostAsJsonAsync("/api/Activity",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher), "2030-02-05T10:00:00", "2030-02-05T11:00:00"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task End_date_before_start_date_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Activity",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher), "2030-02-06T12:00:00", "2030-02-06T09:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Dates_outside_the_module_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Activity",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher), "2030-06-01T09:00:00", "2030-06-01T12:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Module_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync("/api/Activity",
            ActivityBody(999999, await GetActionTypeIdAsync(teacher)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_creates_activity_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);
        var student = await LoginAsync(Student);

        var response = await student.PostAsJsonAsync("/api/Activity",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher)));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_activity_without_token_returns_401()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/Activity", ActivityBody(1, 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_reads_activity_in_every_way_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);
        var activityId = await CreateActivityAsync(teacher, moduleId);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync("/api/Activity?PageNumber=1&PageSize=5")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/Activity/module/{moduleId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/Activity/{activityId}")).StatusCode);
    }

    [Fact]
    public async Task Activity_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/Activity/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 204 today")]
    public async Task Type_of_an_activity_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var activityId = await CreateActivityAsync(teacher, await CreateTestModuleAsync(teacher));

        var response = await teacher.GetAsync($"/api/ActionType/activity/{activityId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 204 today")]
    public async Task Type_of_an_activity_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/ActionType/activity/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_updates_activity_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);
        var activityId = await CreateActivityAsync(teacher, moduleId);

        var response = await teacher.PutAsJsonAsync($"/api/Activity/{activityId}",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_activity_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);

        var response = await teacher.PutAsJsonAsync("/api/Activity/999999",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_with_dates_outside_the_module_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);
        var activityId = await CreateActivityAsync(teacher, moduleId);

        var response = await teacher.PutAsJsonAsync($"/api/Activity/{activityId}",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher), "2030-06-01T09:00:00", "2030-06-01T12:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_updates_or_deletes_activity_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateTestModuleAsync(teacher);
        var activityId = await CreateActivityAsync(teacher, moduleId);
        var student = await LoginAsync(Student);

        var update = await student.PutAsJsonAsync($"/api/Activity/{activityId}",
            ActivityBody(moduleId, await GetActionTypeIdAsync(teacher)));
        var delete = await student.DeleteAsync($"/api/Activity/{activityId}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Teacher_deletes_activity_then_it_is_gone()
    {
        var teacher = await LoginAsync(Teacher);
        var activityId = await CreateActivityAsync(teacher, await CreateTestModuleAsync(teacher));

        var first = await teacher.DeleteAsync($"/api/Activity/{activityId}");
        var second = await teacher.DeleteAsync($"/api/Activity/{activityId}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
