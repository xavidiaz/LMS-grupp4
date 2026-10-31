using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US06DocumentsTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Teacher_uploads_course_and_module_documents_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await GetMyCourseIdAsync(await LoginAsync(Student));
        var moduleId = await GetFirstModuleIdAsync(teacher, courseId);

        Assert.Equal(HttpStatusCode.OK, (await UploadDocumentAsync(teacher, "courseId", courseId)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await UploadDocumentAsync(teacher, "moduleId", moduleId)).StatusCode);
    }

    [Fact]
    public async Task Forbidden_file_type_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await GetMyCourseIdAsync(await LoginAsync(Student));

        var response = await UploadDocumentAsync(teacher, "courseId", courseId, "virus.exe");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_without_token_returns_401()
    {
        var response = await UploadDocumentAsync(Anonymous(), "courseId", 1);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Course_documents_are_limited_to_the_course()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);
        var otherCourseId = await GetMyCourseIdAsync(await LoginAsync(StudentB));
        var courseId = await GetMyCourseIdAsync(student);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/documents/course/{courseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync($"/api/documents/course/{courseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync($"/api/documents/course/{otherCourseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous().GetAsync($"/api/documents/course/{courseId}")).StatusCode);
    }

    [Fact]
    public async Task Module_documents_are_limited_to_the_course()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);
        var moduleId = await GetFirstModuleIdAsync(teacher, await GetMyCourseIdAsync(student));
        var otherStudent = await LoginAsync(StudentB);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"/api/documents/module/{moduleId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync($"/api/documents/module/{moduleId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherStudent.GetAsync($"/api/documents/module/{moduleId}")).StatusCode);
    }

    [Fact]
    public async Task Download_is_limited_to_the_course()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);
        var otherStudent = await LoginAsync(StudentB);
        var id = await ReadIdAsync(await UploadDocumentAsync(teacher, "courseId", await GetMyCourseIdAsync(student)));
        var url = $"/api/documents/{id}/download";

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherStudent.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Anonymous().GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Document_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/documents/999999/download");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Only_teacher_deletes_a_document_then_it_is_gone()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);
        var id = await ReadIdAsync(await UploadDocumentAsync(teacher, "courseId", await GetMyCourseIdAsync(student)));

        var byStudent = await student.DeleteAsync($"/api/documents/{id}");
        var first = await teacher.DeleteAsync($"/api/documents/{id}");
        var second = await teacher.DeleteAsync($"/api/documents/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, byStudent.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
