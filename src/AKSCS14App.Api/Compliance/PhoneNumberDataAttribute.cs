using Microsoft.Extensions.Compliance.Classification;

namespace CS14App.Api.Compliance;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class PhoneNumberDataAttribute : DataClassificationAttribute
{
    public PhoneNumberDataAttribute()
        : base(AppTaxonomy.PhoneNumber)
    {
    }
}