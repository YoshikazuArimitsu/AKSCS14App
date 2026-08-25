using CS14App.Api.Compliance;
using CS14App.Api.Data;
using CS14App.Api.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.OpenApi;

using Serilog;
using Serilog.Extensions.Hosting;

const string ConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}";
const string FileOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up CS14App.Api");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("sqlitedb")));

    builder.AddRedisClient("redis");

    // .NETコンプライアンス有効化（電話番号用 Redactor の登録）
    builder.Services.AddRedaction(options =>
        options.SetRedactor<PhoneNumberRedactor>(new DataClassificationSet(AppTaxonomy.PhoneNumber)));

    // ロガー設定読み込み＆初期化
    // コンソール出力にはコンプライアンスマスクを適用し、ファイル出力には原文のまま出力する。
    // writeToProviders: true により、AddServiceDefaults() (ConfigureOpenTelemetry) で登録された
    // OpenTelemetry の ILoggerProvider にもログイベントを転送し、構造化ログとして OTLP エクスポートさせる。
    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Logger(lc => lc
            .Enrich.With(new ClassifiedLogPropertyMaskingEnricher(services.GetRequiredService<IRedactorProvider>()))
            .WriteTo.Console(outputTemplate: ConsoleOutputTemplate))
        .WriteTo.Logger(lc => lc
            .WriteTo.File(
                "logs/cs14app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: FileOutputTemplate)),
        writeToProviders: true);

    // Init swagger
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AKSCS14App API",
            Version = "v1",
            Description = "Arimitsu ga Kangaeta Saikyo no C#14App API",
        });
    });

    builder.Services.AddSingleton<IGreetingService, GreetingService>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AKSCS14App API v1");
    });

    // DevContainer 等でルートにアクセスした際に Swagger UI を表示する。
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.MapDefaultEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "AKSCS14App.Api がスタートアップ中に予期せず終了しました");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;