using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;

using Serilog.Core;
using Serilog.Events;

namespace CS14App.Api.Compliance;

/// <summary>
/// [LogProperties] で展開されたログプロパティのうち、モデルに DataClassificationAttribute
/// (例: PhoneNumberDataAttribute) が付与されたメンバーに対応するものをマスクする Serilog エンリッチャー。
/// コンソール出力用のサブロガーにのみ適用し、ファイル出力には適用しない想定。
/// </summary>
public sealed class ClassifiedLogPropertyMaskingEnricher : ILogEventEnricher
{
    private static readonly ConcurrentDictionary<string, DataClassification> ClassifiedMemberNames = DiscoverClassifiedMemberNames();

    private readonly IRedactorProvider _redactorProvider;

    public ClassifiedLogPropertyMaskingEnricher(IRedactorProvider redactorProvider)
    {
        _redactorProvider = redactorProvider;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var key in logEvent.Properties.Keys.ToArray())
        {
            var memberName = key[(key.LastIndexOf('.') + 1)..];
            if (!ClassifiedMemberNames.TryGetValue(memberName, out var classification))
            {
                continue;
            }

            if (logEvent.Properties[key] is not ScalarValue { Value: string plainText })
            {
                continue;
            }

            var redactor = _redactorProvider.GetRedactor(new DataClassificationSet(classification));
            var masked = redactor.Redact(plainText);
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(key, masked));
        }
    }

    private static ConcurrentDictionary<string, DataClassification> DiscoverClassifiedMemberNames()
    {
        var result = new ConcurrentDictionary<string, DataClassification>();

        var members = Assembly.GetExecutingAssembly().GetTypes()
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var member in members)
        {
            var attribute = member.GetCustomAttribute<DataClassificationAttribute>();
            if (attribute is not null)
            {
                result[member.Name] = attribute.Classification;
            }
        }

        return result;
    }
}