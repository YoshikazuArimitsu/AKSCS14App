using System.Runtime.CompilerServices;

namespace CS14App.Api.Tests;

/// <summary>
/// テスト用の AWS 環境変数設定。
/// </summary>
/// <remarks>
/// Program.cs は LocalStack 無効時に AddAWSService&lt;IAmazonSQS&gt;() で実クライアントを登録するため、
/// リージョンと認証情報が解決できないと MessagesController の生成自体が
/// AmazonClientException で失敗する（EC2 インスタンスメタデータへの問い合わせで待たされる）。
/// docker-compose の api サービスと同様にダミー値を与え、メタデータ取得も無効化する。
/// 実際に AWS へ接続するテストは無いため、署名が検証されることはない。
/// </remarks>
internal static class AwsTestEnvironment
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        SetIfEmpty("AWS_REGION", "ap-northeast-1");
        SetIfEmpty("AWS_DEFAULT_REGION", "ap-northeast-1");
        SetIfEmpty("AWS_ACCESS_KEY_ID", "test");
        SetIfEmpty("AWS_SECRET_ACCESS_KEY", "test");
        SetIfEmpty("AWS_EC2_METADATA_DISABLED", "true");
    }

    private static void SetIfEmpty(string name, string value)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
