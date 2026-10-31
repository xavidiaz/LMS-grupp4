using System.Net;
using System.Net.Http.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US02CoursesTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Teacher_creates_course_returns_200()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync("/api/courses", CourseBody());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Empty_name_returns_400()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync("/api/courses", CourseBody(name: ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task End_date_before_start_date_returns_400()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PostAsJsonAsync("/api/courses",
            CourseBody(start: "2031-01-01T00:00:00", end: "2030-01-01T00:00:00"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_creates_course_returns_403()
    {
        var student = await LoginAsync(Student);

        var response = await student.PostAsJsonAsync("/api/courses", CourseBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_course_without_token_returns_401()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/courses", CourseBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_lists_courses_returns_200()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/courses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Student_lists_courses_returns_403()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync("/api/courses");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Course_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/courses/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_reads_own_course_returns_200()
    {
        var student = await LoginAsync(Student);
        var courseId = await GetMyCourseIdAsync(student);

        var response = await student.GetAsync($"/api/courses/{courseId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Student_reads_another_course_returns_403()
    {
        var student = await LoginAsync(Student);
        var otherCourseId = await GetMyCourseIdAsync(await LoginAsync(StudentB));

        var response = await student.GetAsync($"/api/courses/{otherCourseId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_updates_description_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PatchAsJsonAsync("/api/courses", new { id = courseId, description = "Updated." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_course_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.PatchAsJsonAsync("/api/courses", new { id = 999999, description = "Updated." });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_with_end_date_before_start_date_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PatchAsJsonAsync("/api/courses", new
        {
            id = courseId,
            startDate = "2031-01-01T00:00:00",
            endDate = "2030-01-01T00:00:00"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_updates_course_returns_403()
    {
        var student = await LoginAsync(Student);
        var courseId = await GetMyCourseIdAsync(student);

        var response = await student.PatchAsJsonAsync("/api/courses", new { id = courseId, description = "Updated." });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_course_with_students_returns_409()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        (await teacher.PostAsJsonAsync("/api/users", UserBody(courseId))).EnsureSuccessStatusCode();

        var response = await teacher.DeleteAsync($"/api/courses/{courseId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Student_deletes_course_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var student = await LoginAsync(Student);

        var response = await student.DeleteAsync($"/api/courses/{courseId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_deletes_empty_course_then_it_is_gone()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var first = await teacher.DeleteAsync($"/api/courses/{courseId}");
        var second = await teacher.DeleteAsync($"/api/courses/{courseId}");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
