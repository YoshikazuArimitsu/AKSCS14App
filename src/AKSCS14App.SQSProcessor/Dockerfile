# syntax=docker/dockerfile:1

# Lambda用コンテナイメージ。ビルドコンテキストはリポジトリルートを想定。
# 例: docker build -f src/AKSCS14App.SQSProcessor/Dockerfile .
#
# 最終ステージは AWS Lambda 公式の .NET ベースイメージを使用する。
# このイメージには Lambda Runtime Interface Client が組み込まれているため、
# 独自の bootstrap 実装は不要で、CMD にハンドラ文字列を指定するだけでよい。

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AKSCS14App.SQSProcessor/AKSCS14App.SQSProcessor.csproj", "src/AKSCS14App.SQSProcessor/"]
RUN dotnet restore "src/AKSCS14App.SQSProcessor/AKSCS14App.SQSProcessor.csproj"

COPY ["src/AKSCS14App.SQSProcessor/", "src/AKSCS14App.SQSProcessor/"]

WORKDIR /src/src/AKSCS14App.SQSProcessor
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM public.ecr.aws/lambda/dotnet:10 AS final
WORKDIR /var/task
COPY --from=build /app/publish .

CMD ["AKSCS14App.SQSProcessor::AKSCS14App.SQSProcessor.Function::FunctionHandler"]
