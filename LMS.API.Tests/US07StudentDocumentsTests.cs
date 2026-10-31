using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US07StudentDocumentsTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Student_shares_with_own_course_and_module_returns_200()
    {
        var student = await LoginAsync(Student);
        var courseId = await GetMyCourseIdAsync(student);
        var moduleId = await GetFirstModuleIdAsync(student, courseId);

        Assert.Equal(HttpStatusCode.OK, (await UploadDocumentAsync(student, "courseId", courseId)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await UploadDocumentAsync(student, "moduleId", moduleId)).StatusCode);
    }

    [Fact]
    public async Task Forbidden_file_type_returns_400()
    {
        var student = await LoginAsync(Student);
        var courseId = await GetMyCourseIdAsync(student);

        var response = await UploadDocumentAsync(student, "courseId", courseId, "virus.exe");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_shares_with_another_course_returns_403()
    {
        var student = await LoginAsync(Student);
        var otherCourseId = await GetMyCourseIdAsync(await LoginAsync(StudentB));

        var response = await UploadDocumentAsync(student, "courseId", otherCourseId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Student_shares_with_another_courses_module_returns_403()
    {
        var student = await LoginAsync(Student);
        var otherStudent = await LoginAsync(StudentB);
        var moduleId = await GetFirstModuleIdAsync(student, await GetMyCourseIdAsync(student));

        var response = await UploadDocumentAsync(otherStudent, "moduleId", moduleId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Shared_document_is_limited_to_the_course()
    {
        var teacher = await LoginAsync(Teacher);
        var student = await LoginAsync(Student);
        var otherStudent = await LoginAsync(StudentB);
        var id = await ReadIdAsync(await UploadDocumentAsync(student, "courseId", await GetMyCourseIdAsync(student)));
        var url = $"/api/documents/{id}/download";

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherStudent.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Student_deletes_own_document_returns_403()
    {
        var student = await LoginAsync(Student);
        var id = await ReadIdAsync(await UploadDocumentAsync(student, "courseId", await GetMyCourseIdAsync(student)));

        var response = await student.DeleteAsync($"/api/documents/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
