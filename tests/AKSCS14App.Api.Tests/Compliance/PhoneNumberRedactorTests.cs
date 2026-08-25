using CS14App.Api.Compliance;

namespace CS14App.Api.Tests.Compliance;

public class PhoneNumberRedactorTests
{
    private readonly PhoneNumberRedactor _sut = new();

    [Theory]
    [InlineData("09012345678", "*******5678")]
    [InlineData("0312345678", "******5678")]
    [InlineData("12345", "*2345")]
    public void Redact_MasksAllButLastFourDigits(string input, string expected)
    {
        var result = _sut.Redact(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("12")]
    [InlineData("")]
    public void Redact_WithFourOrFewerChars_MasksEntirely(string input)
    {
        var result = _sut.Redact(input);

        Assert.Equal(new string('*', input.Length), result);
    }
}