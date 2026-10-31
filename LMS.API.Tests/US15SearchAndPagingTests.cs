using System.Net;

namespace LMS.API.Tests;

[Collection("Api")]
public class US15SearchAndPagingTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Search_finds_a_user_by_name()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync("/api/users?SearchTerm=Maria");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Maria", body);
        Assert.DoesNotContain("Omar", body);
    }

    [Fact]
    public async Task Search_with_no_match_returns_an_empty_list()
    {
        var teacher = await LoginAsync(Teacher);

        var body = await ReadJsonAsync(await teacher.GetAsync("/api/users?SearchTerm=NobodyHasThisName"));

        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Search_term_above_100_characters_returns_400()
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync($"/api/users?SearchTerm={new string('x', 101)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/Module")]
    [InlineData("/api/Activity")]
    [InlineData("/api/ActionType")]
    public async Task Page_size_limits_the_rows(string url)
    {
        var teacher = await LoginAsync(Teacher);

        var body = await ReadJsonAsync(await teacher.GetAsync($"{url}?PageNumber=1&PageSize=2"));

        Assert.Equal(2, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Pages_contain_different_users()
    {
        var teacher = await LoginAsync(Teacher);

        var first = await ReadJsonAsync(await teacher.GetAsync("/api/users?PageNumber=1&PageSize=2"));
        var second = await ReadJsonAsync(await teacher.GetAsync("/api/users?PageNumber=2&PageSize=2"));

        Assert.NotEqual(
            first.GetProperty("items")[0].GetProperty("id").GetString(),
            second.GetProperty("items")[0].GetProperty("id").GetString());
    }

    [Theory]
    [InlineData("PageNumber=0")]
    [InlineData("PageSize=0")]
    [InlineData("PageSize=101")]
    public async Task Invalid_paging_returns_400(string query)
    {
        var teacher = await LoginAsync(Teacher);

        var response = await teacher.GetAsync($"/api/users?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
