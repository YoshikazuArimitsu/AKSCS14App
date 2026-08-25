using Microsoft.AspNetCore.Mvc.Testing;

namespace CS14App.Api.Tests;

[CollectionDefinition(Name)]
public class ApiTestCollection : ICollectionFixture<WebApplicationFactory<Program>>
{
    public const string Name = "Api test collection";
}