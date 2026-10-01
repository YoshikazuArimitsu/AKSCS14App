using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace CS14App.Api.Tests.Controllers;

[Collection(ApiTestCollection.Name)]
public class MessagesControllerTests
{
    private readonly HttpClient _client;

    public MessagesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("limit=0")]
    [InlineData("limit=1001")]
    [InlineData("offset=-1")]
    public async Task Get_WithOutOfRangeQuery_ReturnsBadRequest(string query)
    {
        var response = await _client.GetAsync($"/api/messages?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // テスト環境には ConnectionStrings:messagesdb が無いため NpgsqlDataSource は登録されず、
    // SQS キューURL未設定時の POST と同様に ProblemDetails(500) を返すことを確認する。
    [Fact]
    public async Task Get_WithoutConnectionString_ReturnsProblem()
    {
        var response = await _client.GetAsync("/api/messages");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("接続文字列", body);
    }
}
