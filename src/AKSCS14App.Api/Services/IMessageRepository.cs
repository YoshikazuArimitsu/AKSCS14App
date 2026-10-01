using CS14App.Api.Models;

namespace CS14App.Api.Services;

/// <summary>
/// DB に蓄積されたメッセージの参照を担うリポジトリ。
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// DB 接続（接続文字列 ConnectionStrings:messagesdb）が構成済みかどうか。
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// 蓄積済みメッセージを受信日時の新しい順に取得する。
    /// </summary>
    /// <param name="limit">取得件数の上限。</param>
    /// <param name="offset">読み飛ばす件数。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    Task<IReadOnlyList<MessageModel>> GetMessagesAsync(int limit, int offset, CancellationToken cancellationToken);
}
