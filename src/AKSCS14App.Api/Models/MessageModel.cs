namespace CS14App.Api.Models;

/// <summary>
/// DB に蓄積されたメッセージ1件分の情報。
/// SQSProcessor(Lambda) が PostgreSQL の audits テーブルへ INSERT した行に対応する。
/// </summary>
/// <param name="Id">audits テーブルの連番(BIGSERIAL)。</param>
/// <param name="Body">SQS メッセージ本文（API の POST /api/messages で送信した Text）。</param>
/// <param name="SqsMessageId">送信元の SQS メッセージ ID（重複受信の判定キー）。</param>
/// <param name="ReceivedAt">SQSProcessor が DB へ書き込んだ日時(UTC)。</param>
public record MessageModel(long Id, string Body, string SqsMessageId, DateTimeOffset ReceivedAt);
