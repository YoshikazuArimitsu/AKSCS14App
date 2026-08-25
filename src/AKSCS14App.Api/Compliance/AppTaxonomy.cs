using Microsoft.Extensions.Compliance.Classification;

namespace CS14App.Api.Compliance;

public static class AppTaxonomy
{
    private const string Name = "AKSCS14App";

    public static DataClassification PhoneNumber { get; } = new(Name, nameof(PhoneNumber));
}