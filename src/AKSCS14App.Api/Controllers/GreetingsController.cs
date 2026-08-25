using CS14App.Api.Models;
using CS14App.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace CS14App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class GreetingsController : ControllerBase
{
    private readonly IGreetingService _greetingService;
    private readonly ILogger<GreetingsController> _logger;

    public GreetingsController(IGreetingService greetingService, ILogger<GreetingsController> logger)
    {
        _greetingService = greetingService;
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<string> Post(UserModel user)
    {
        LogGreetingRequested(_logger, user.Name, user.PhoneNumber);

        return Ok(_greetingService.Greet(user.Name));
    }

    // NOTE: [LogProperties] でクラス分類プロパティを渡すと、EnableRedaction を有効化しない限り
    // 当該プロパティがログに一切出力されない（Redactor 未設定時は握りつぶされる）ため、
    // ここでは通常のテンプレート引数として渡し、出力先ごとのマスク適用は
    // ClassifiedLogPropertyMaskingEnricher（コンソール用サブロガーのみ）に委ねる。
    [LoggerMessage(Level = LogLevel.Information, Message = "Greeting requested for {Name} {PhoneNumber}")]
    private static partial void LogGreetingRequested(ILogger logger, string name, string phoneNumber);
}