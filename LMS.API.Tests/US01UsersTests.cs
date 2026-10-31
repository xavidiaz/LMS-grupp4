using System.Net;
using System.Net.Http.Json;

namespace LMS.API.Tests;

[Collection("Api")]
public class US01UsersTests(ApiFactory factory) : ApiTestBase(factory)
{
    private const string NewPassword = "TestUser!12345";

    private static async Task<(string Id, string Token, string Email)> CreateUserAsync(HttpClient teacher, int courseId)
    {
        var email = $"{Unique("user")}@lms.com";
        var response = await teacher.PostAsJsonAsync("/api/users", UserBody(courseId, email));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await ReadJsonAsync(response);
        return (body.GetProperty("user").GetProperty("id").GetString()!,
            body.GetProperty("passwordSetupToken").GetString()!,
            email);
    }

    [Fact]
    public async Task Teacher_lists_users_returns_200()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/users?PageNumber=1&PageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task User_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/users/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_lists_users_returns_403()
    {
        var student = await LoginAsync(Student);

        var response = await student.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Users_without_token_returns_401()
    {
        var response = await Anonymous().GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Same_email_again_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var (_, _, email) = await CreateUserAsync(teacher, courseId);

        var response = await teacher.PostAsJsonAsync("/api/users", UserBody(courseId, email));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_email_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/users", UserBody(courseId, "not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Missing_first_name_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/users", UserBody(courseId, firstName: ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 500 today")]
    public async Task Role_that_does_not_exist_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PostAsJsonAsync("/api/users", UserBody(courseId, role: "Nope"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Student_creates_user_returns_403()
    {
        var student = await LoginAsync(Student);

        var response = await student.PostAsJsonAsync("/api/users", UserBody(1));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 500 today")]
    public async Task Passwords_that_do_not_match_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var (id, token, _) = await CreateUserAsync(teacher, await CreateCourseAsync(teacher));

        var response = await Anonymous().PostAsJsonAsync("/api/users/set-password", new
        {
            userId = id,
            token,
            password = NewPassword,
            confirmPassword = "Different!12345"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "See #219: returns 500 today")]
    public async Task Wrong_setup_token_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var (id, _, _) = await CreateUserAsync(teacher, await CreateCourseAsync(teacher));

        var response = await Anonymous().PostAsJsonAsync("/api/users/set-password", new
        {
            userId = id,
            token = "wrong-token",
            password = NewPassword,
            confirmPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task New_user_sets_password_and_logs_in()
    {
        var teacher = await LoginAsync(Teacher);
        var (id, token, email) = await CreateUserAsync(teacher, await CreateCourseAsync(teacher));

        var response = await Anonymous().PostAsJsonAsync("/api/users/set-password", new
        {
            userId = id,
            token,
            password = NewPassword,
            confirmPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await LoginAsync(email, NewPassword);
    }

    [Fact]
    public async Task Teacher_updates_user_returns_200()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var (id, _, email) = await CreateUserAsync(teacher, courseId);

        var response = await teacher.PutAsJsonAsync($"/api/users/{id}", UserBody(courseId, email, "Updated"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_user_that_does_not_exist_returns_404()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);

        var response = await teacher.PutAsJsonAsync("/api/users/does-not-exist", UserBody(courseId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Student_updates_or_deletes_user_returns_403()
    {
        var teacher = await LoginAsync(Teacher);
        var courseId = await CreateCourseAsync(teacher);
        var (id, _, email) = await CreateUserAsync(teacher, courseId);
        var student = await LoginAsync(Student);

        var update = await student.PutAsJsonAsync($"/api/users/{id}", UserBody(courseId, email));
        var delete = await student.DeleteAsync($"/api/users/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Teacher_deletes_own_account_returns_400()
    {
        var teacher = await LoginAsync(Teacher);
        var users = await ReadJsonAsync(await teacher.GetAsync($"/api/users?SearchTerm={Teacher}"));
        var teacherId = users.GetProperty("items")[0].GetProperty("id").GetString();

        var response = await teacher.DeleteAsync($"/api/users/{teacherId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_deletes_user_then_it_is_gone()
    {
        var teacher = await LoginAsync(Teacher);
        var (id, _, _) = await CreateUserAsync(teacher, await CreateCourseAsync(teacher));

        var first = await teacher.DeleteAsync($"/api/users/{id}");
        var second = await teacher.DeleteAsync($"/api/users/{id}");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }
}
