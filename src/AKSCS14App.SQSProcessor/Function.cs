using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

using Npgsql;

// Lambda の JSON 入出力を .NET の型へ変換できるようにするアセンブリ属性
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AKSCS14App.SQSProcessor;

public class Function
{
    // テーブル作成
    private const string EnsureTableSql = """
        CREATE TABLE IF NOT EXISTS audits (
            id BIGSERIAL PRIMARY KEY,
            body TEXT NOT NULL,
            sqs_message_id TEXT NOT NULL UNIQUE,
            received_at TIMESTAMPTZ NOT NULL DEFAULT now()
        )
        """;

    // SQS は at-least-once 配信のため、重複受信時は sqs_message_id で無視する
    private const string InsertMessageSql = """
        INSERT INTO audits (body, sqs_message_id)
        VALUES ($1, $2)
        ON CONFLICT (sqs_message_id) DO NOTHING
        """;

    private readonly string _connectionString;

    public Function()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__messagesdb")
            ?? throw new InvalidOperationException("接続文字列 ConnectionStrings__messagesdb が設定されていません。");
    }

    /// <summary>
    /// SQSから受け取ったメッセージバッチINSERT
    /// </summary>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        if (evnt.Records.Count == 0)
        {
            return;
        }

        context.Logger.LogInformation($"Received {evnt.Records.Count} SQS Messages");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var batch = new NpgsqlBatch(connection);
        batch.BatchCommands.Add(new NpgsqlBatchCommand(EnsureTableSql));

        foreach (var record in evnt.Records)
        {
            var command = new NpgsqlBatchCommand(InsertMessageSql);
            command.Parameters.Add(new NpgsqlParameter { Value = record.Body });
            command.Parameters.Add(new NpgsqlParameter { Value = record.MessageId });
            batch.BatchCommands.Add(command);
        }

        var affected = await batch.ExecuteNonQueryAsync();
        context.Logger.LogInformation($"Write {affected} Records to PostgreSQL.");
    }
}
