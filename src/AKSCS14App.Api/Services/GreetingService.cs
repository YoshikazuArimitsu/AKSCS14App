namespace CS14App.Api.Services;

public class GreetingService : IGreetingService
{
    public string Greet(string name) => $"Hello, {name}!";
}