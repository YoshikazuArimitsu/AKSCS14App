using Amazon;

using Aspire.Hosting.LocalStack.Container;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL コンテナ作成
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();
var messagesDb = postgres.AddDatabase("messagesdb");


// AWS SDK 初期設定
var awsConfig = builder.AddAWSSDKConfig()
    .WithRegion(RegionEndpoint.APNortheast1);

// LocalStack初期化
// dotnet user-secrets set "LocalStack:AuthToken" "<token>" --project src/AKSCS14App.AppHost

var localStackAuthToken = builder.Configuration["LocalStack:AuthToken"];
var localstack = builder.AddLocalStack(awsConfig: awsConfig, configureContainer: container =>
{
    container.Lifetime = ContainerLifetime.Persistent;
    if (!string.IsNullOrEmpty(localStackAuthToken))
    {
        container.AdditionalEnvironmentVariables["LOCALSTACK_AUTH_TOKEN"] = localStackAuthToken;
    }
});

// SQSキュー作成・URL取得
var awsResources = builder.AddAWSCloudFormationTemplate("aws-resources", "aws-resources.template")
    .WithReference(awsConfig);
var messageQueueUrl = awsResources.GetOutput("MessageQueueUrl");

// Lambda登録
builder.AddAWSLambdaFunction<Projects.AKSCS14App_SQSProcessor>(
        "sqs-processor",
        lambdaHandler: "AKSCS14App.SQSProcessor::AKSCS14App.SQSProcessor.Function::FunctionHandler")
    .WithReference(awsConfig)
    .WithReference(awsResources)
    .WithReference(messagesDb)
    .WithSQSEventSource(messageQueueUrl)
    .WaitFor(messagesDb);

// API: SQS キューへメッセージを送信する
builder.AddProject<Projects.AKSCS14App_Api>("api")
    .WithReference(awsConfig)
    .WithReference(awsResources)
    .WaitFor(awsResources);
//.WithReplicas(3);    // スケールアウトする場合はコメントアウトを外す

builder.UseLocalStack(localstack);
builder.Build().Run();
