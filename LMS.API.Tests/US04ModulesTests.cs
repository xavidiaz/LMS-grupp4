using System.Net;
using System.Net.Http.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US04ModulesTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Teacher_creates_module_returns_201()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Module", ModuleBody(courseId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Overlapping_dates_returns_409()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        await CreateModuleAsync(teacher, courseId);

        var response = await teacher.PostAsJsonAsync("/api/Module",
            ModuleBody(courseId, start: "2030-02-10T00:00:00", end: "2030-02-20T00:00:00"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task End_date_before_start_date_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Module",
            ModuleBody(courseId, start: "2030-03-20T00:00:00", end: "2030-03-10T00:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Dates_outside_the_course_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Module",
            ModuleBody(courseId, start: "2031-02-01T00:00:00", end: "2031-02-28T00:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Empty_name_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/Module", ModuleBody(courseId, name: ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Course_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync("/api/Module", ModuleBody(999999));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_creates_module_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var student = await LoginAsync(Student);

        var response = await student.PostAsJsonAsync("/api/Module", ModuleBody(courseId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_module_without_token_returns_401()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/Module", ModuleBody(1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_reads_module_in_every_way_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var moduleId = await CreateModuleAsync(teacher, courseId);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync("/api/Module?PageNumber=1&PageSize=5")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/Module/{moduleId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/Module/{moduleId}/course")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/Module/course/{courseId}")).StatusCode);
    }

    [Fact]
    public async Task Module_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        Assert.Equal(HttpStatusCode.NotFound, (await teacher.GetAsync("/api/Module/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await teacher.GetAsync("/api/Module/999999/course")).StatusCode);
    }

    [Fact]
    public async Task Overlap_check_reports_correctly()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var moduleId = await CreateModuleAsync(teacher, courseId);
        var url = $"/api/Module/{courseId}/overlapping-dates";

        var overlap = await ReadJsonAsync(await teacher.GetAsync($"{url}?startDate=2030-02-10&endDate=2030-02-20"));
        var free = await ReadJsonAsync(await teacher.GetAsync($"{url}?startDate=2030-08-01&endDate=2030-08-20"));
        var excluded = await ReadJsonAsync(await teacher.GetAsync($"{url}?startDate=2030-02-10&endDate=2030-02-20&excludeModuleId={moduleId}"));

        Assert.True(overlap.GetProperty("isOverlapping").GetBoolean());
        Assert.False(free.GetProperty("isOverlapping").GetBoolean());
        Assert.False(excluded.GetProperty("isOverlapping").GetBoolean());
    }

    [Fact]
    public async Task Student_checks_overlap_returns_403()
    {
        var student = await LoginAsync(Student);
        var courseId = await GetMyCourseIdAsync(student);

        var response = await student.GetAsync($"/api/Module/{courseId}/overlapping-dates?startDate=2030-02-10&endDate=2030-02-20");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_updates_module_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var moduleId = await CreateModuleAsync(teacher, courseId);

        var response = await teacher.PutAsJsonAsync($"/api/Module/{moduleId}", ModuleBody(courseId, name: "Updated"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_module_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PutAsJsonAsync("/api/Module/999999", ModuleBody(courseId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_with_dates_outside_the_course_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var moduleId = await CreateModuleAsync(teacher, courseId);

        var response = await teacher.PutAsJsonAsync($"/api/Module/{moduleId}",
            ModuleBody(courseId, start: "2031-02-01T00:00:00", end: "2031-02-28T00:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_updates_or_deletes_module_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var moduleId = await CreateModuleAsync(teacher, courseId);
        var student = await LoginAsync(Student);

        var update = await student.PutAsJsonAsync($"/api/Module/{moduleId}", ModuleBody(courseId));
        var delete = await student.DeleteAsync($"/api/Module/{moduleId}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Teacher_deletes_module_then_it_is_gone()
    {
        var teacher = await LoginAsync(Teacher);
        var moduleId = await CreateModuleAsync(teacher, await CreateCourseAsync(teacher));

        var first = await teacher.DeleteAsync($"/api/Module/{moduleId}");
        var second = await teacher.DeleteAsync($"/api/Module/{moduleId}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
