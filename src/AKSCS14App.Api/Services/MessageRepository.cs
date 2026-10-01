using CS14App.Api.Models;

using Npgsql;

namespace CS14App.Api.Services;

/// <summary>
/// PostgreSQL の audits テーブル（SQSProcessor が書き込む先）からメッセージを読み出す実装。
/// </summary>
public sealed class MessageRepository : IMessageRepository
{
    // audits テーブルが未作成のときに PostgreSQL が返す SQLSTATE(undefined_table)。
    private const string UndefinedTableSqlState = "42P01";

    private const string SelectMessagesSql = """
        SELECT id, body, sqs_message_id, received_at
        FROM audits
        ORDER BY id DESC
        LIMIT $1 OFFSET $2
        """;

    private readonly NpgsqlDataSource? _dataSource;

    /// <param name="dataSource">
    /// 接続文字列 ConnectionStrings:messagesdb が未設定の環境では null が渡る
    /// （Program.cs 側で NpgsqlDataSource を登録しないため）。
    /// </param>
    public MessageRepository(NpgsqlDataSource? dataSource)
    {
        _dataSource = dataSource;
    }

    public bool IsConfigured => _dataSource is not null;

    public async Task<IReadOnlyList<MessageModel>> GetMessagesAsync(int limit, int offset, CancellationToken cancellationToken)
    {
        if (_dataSource is null)
        {
            throw new InvalidOperationException("接続文字列 ConnectionStrings:messagesdb が設定されていません。");
        }

        await using var command = _dataSource.CreateCommand(SelectMessagesSql);
        command.Parameters.Add(new NpgsqlParameter { Value = limit });
        command.Parameters.Add(new NpgsqlParameter { Value = offset });

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var messages = new List<MessageModel>();
            while (await reader.ReadAsync(cancellationToken))
            {
                messages.Add(new MessageModel(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetFieldValue<DateTimeOffset>(3)));
            }

            return messages;
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            // audits テーブルは SQSProcessor(Lambda) が初回起動時に CREATE TABLE IF NOT EXISTS で作る。
            // まだ1件も処理されていない状態は「メッセージ0件」と等価なため、空リストを返す。
            return [];
        }
    }
}
