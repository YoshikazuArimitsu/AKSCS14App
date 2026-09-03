using CS14App.Api.Services;

namespace CS14App.Api.Tests.Services;

public class GreetingServiceTests
{
    private readonly GreetingService _sut = new();

    [Fact]
    public void Greet_ReturnsGreetingContainingName()
    {
        var result = _sut.Greet("Taro");

        Assert.Equal("Hello, Taro!", result);
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("Bob")]
    public void Greet_ReturnsNameInMessage(string name)
    {
        var result = _sut.Greet(name);

        Assert.Contains(name, result);
    }
}
