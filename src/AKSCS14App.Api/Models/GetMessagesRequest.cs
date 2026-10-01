using System.ComponentModel.DataAnnotations;

namespace CS14App.Api.Models;

/// <summary>
/// メッセージ一覧取得(GET /api/messages)のクエリパラメータ。
/// </summary>
public class GetMessagesRequest
{
    /// <summary>取得件数の上限。</summary>
    [Range(1, 1000)]
    public int Limit { get; set; } = 100;

    /// <summary>読み飛ばす件数（ページング用のオフセット）。</summary>
    [Range(0, int.MaxValue)]
    public int Offset { get; set; }
}
