using System.Net;
using System.Net.Http.Json;

using CS14App.Api.Models;

using Microsoft.AspNetCore.Mvc.Testing;

namespace CS14App.Api.Tests.Controllers;

[Collection(ApiTestCollection.Name)]
public class GreetingsControllerTests
{
    private readonly HttpClient _client;

    public GreetingsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_WithValidUser_ReturnsOkWithGreeting()
    {
        var response = await _client.PostAsJsonAsync("/api/greetings", new UserModel { Name = "Taro", PhoneNumber = "09012345678" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Taro", body);
    }

    [Fact]
    public async Task Post_WithWhitespaceName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/greetings", new UserModel { Name = " ", PhoneNumber = "09012345678" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingPhoneNumber_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/greetings", new UserModel { Name = "Taro", PhoneNumber = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}