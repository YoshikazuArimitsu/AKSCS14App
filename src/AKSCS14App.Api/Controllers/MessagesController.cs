using Amazon.SQS;

using CS14App.Api.Models;
using CS14App.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace CS14App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class MessagesController : ControllerBase
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IMessageRepository _messageRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IAmazonSQS sqsClient,
        IMessageRepository messageRepository,
        IConfiguration configuration,
        ILogger<MessagesController> logger)
    {
        _sqsClient = sqsClient;
        _messageRepository = messageRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// DB に蓄積済みのメッセージ一覧を、受信日時の新しい順に取得する。
    /// </summary>
    /// <remarks>
    /// POST /api/messages で送信したメッセージを SQSProcessor(Lambda) が PostgreSQL へ書き込むため、
    /// 送信直後は反映されていない場合がある。
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MessageModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<MessageModel>>> GetAsync(
        [FromQuery] GetMessagesRequest request,
        CancellationToken cancellationToken)
    {
        if (!_messageRepository.IsConfigured)
        {
            return Problem("データベースの接続文字列が設定されていません。");
        }

        var messages = await _messageRepository.GetMessagesAsync(request.Limit, request.Offset, cancellationToken);

        LogMessagesListed(_logger, messages.Count, request.Limit, request.Offset);

        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var queueUrl = _configuration["AWS:Resources:MessageQueueUrl"];
        if (string.IsNullOrEmpty(queueUrl))
        {
            return Problem("SQS キューの URL が設定されていません。");
        }

        var response = await _sqsClient.SendMessageAsync(
            new Amazon.SQS.Model.SendMessageRequest { QueueUrl = queueUrl, MessageBody = request.Text },
            cancellationToken);

        LogMessageSent(_logger, response.MessageId);

        return Accepted(new { messageId = response.MessageId });
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Send message to SQS 。MessageId={MessageId}")]
    private static partial void LogMessageSent(ILogger logger, string messageId);

    [LoggerMessage(Level = LogLevel.Information, Message = "List messages from PostgreSQL 。Count={Count} Limit={Limit} Offset={Offset}")]
    private static partial void LogMessagesListed(ILogger logger, int count, int limit, int offset);
}
