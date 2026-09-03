using Amazon.SQS;

using CS14App.Api.Models;

using Microsoft.AspNetCore.Mvc;

namespace CS14App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class MessagesController : ControllerBase
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<MessagesController> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _logger = logger;
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
}
