# AWSデモ環境（Terraform）

AKSCS14App をAWS上のデモ環境として動かすためのTerraformコード。
PostgreSQLはRDS、APIはFargate、インターネット向けALBで構成する。
SQSProcessorはコンテナイメージ形式のLambdaとしてデプロイし、SQSキューへのイベントソースマッピングで
メッセージ到着時に自動起動する。

## 構成概要

- **VPC**: パブリックサブネット×2（ALB・Fargateタスク）、プライベートサブネット×2（RDS・Lambda）
- **ALB**: インターネット向け、HTTP:80 で待ち受け、Fargateタスク（ポート8080）へフォワード
- **ECS(Fargate)**: `AKSCS14App.Api` コンテナを実行。コスト削減のためNATゲートウェイは作らず、
  タスクをパブリックサブネットに直接配置（インバウンドはセキュリティグループでALBのみに制限）。
  実AWS環境では `LocalStack:UseLocalStack` を設定しないため、アプリは標準のAWS SDK経路
  （タスクロールの権限）で実SQSへ接続する
- **RDS(PostgreSQL)**: プライベートサブネットに配置し、ECSタスク・Lambdaのセキュリティグループからのみ接続可能。
  マスターパスワードは `random_password` で生成し、Secrets Managerにも保管
- **SQS**: `MessagesController` の送信先キューを作成し、ECSタスクロールに送信権限を付与
- **SQSProcessor(Lambda)**: コンテナイメージとしてデプロイし、プライベートサブネットに配置（RDSへ接続するためVPCアタッチ）。
  `aws_lambda_event_source_mapping` によりSQSキューをLambdaサービス自身が継続的にロングポーリングし、
  メッセージが届くたびに自動起動する（固定間隔のcron実行ではなく、ローカル(Aspire)の `.WithSQSEventSource` と同じ仕組み）。
  ログ配信はVPCのENI経由ではなくLambdaサービス側の経路で行われるため、NATゲートウェイは不要
- **ECR**: APIコンテナ・SQSProcessorコンテナ、それぞれ用のリポジトリ

## 事前準備

- Terraform >= 1.5
- AWS CLIの認証情報（対象アカウント・リージョンへのデプロイ権限）
- Docker（コンテナイメージのビルド用）

## デプロイ手順

`aws_ecs_service` と `aws_lambda_function` はどちらも作成時に有効なコンテナイメージを要求するため、
ECRリポジトリだけ先に作成 → イメージをpush → 残りをapply、という2段階で進める。

```bash
cd terraform
terraform init

# 1. ECRリポジトリのみ先に作成
terraform apply \
  -target=aws_ecr_repository.api \
  -target=aws_ecr_repository.sqs_processor

# 2. 両方のイメージをビルド & push（後述）

# 3. 残り全リソースをapply
terraform apply
```

### コンテナイメージのビルド & push

```bash
# リポジトリルート（AKSCS14App/）で実行
API_ECR_URL=$(terraform -chdir=terraform output -raw ecr_repository_url)
SQS_PROCESSOR_ECR_URL=$(terraform -chdir=terraform output -raw sqs_processor_ecr_repository_url)

# リージョンは variables.tf の aws_region（既定: ap-northeast-1）に合わせる
aws ecr get-login-password --region ap-northeast-1 | docker login --username AWS --password-stdin "${API_ECR_URL%/*}"

docker build -f src/AKSCS14App.Api/Dockerfile -t "${API_ECR_URL}:latest" .
docker push "${API_ECR_URL}:latest"

docker build -f src/AKSCS14App.SQSProcessor/Dockerfile -t "${SQS_PROCESSOR_ECR_URL}:latest" .
docker push "${SQS_PROCESSOR_ECR_URL}:latest"
```

### 新しいイメージをデプロイに反映

```bash
# API(ECS)
aws ecs update-service \
  --cluster "$(terraform -chdir=terraform output -raw ecs_cluster_name)" \
  --service akscs14app-api \
  --force-new-deployment

# SQSProcessor(Lambda) — イメージのタグ(latest)自体は変わらないため、
# 新しいdigestを反映させるには一度 update-function-code で明示的に取り直す
aws lambda update-function-code \
  --function-name "$(terraform -chdir=terraform output -raw sqs_processor_function_name)" \
  --image-uri "$(terraform -chdir=terraform output -raw sqs_processor_ecr_repository_url):latest"
```

## デモ確認

```bash
terraform output alb_dns_name
```

出力されたDNS名にブラウザでアクセスし、`http://<ALBのDNS名>/swagger` でSwagger UIが表示されれば成功。
`POST /api/messages` を呼び出すとSQSへメッセージが送信され、SQSProcessor(Lambda)が起動して
RDS(PostgreSQL)の `audits` テーブルへ書き込む。CloudWatch Logs（`/aws/lambda/akscs14app-sqs-processor`）で
処理結果を確認できる。

## 破棄

```bash
terraform destroy
```

## 本番転用時の注意（デモ構成からの変更点）

- ALBをHTTPS化（ACM証明書 + 443番リスナー）
- Fargateタスクをプライベートサブネット + NATゲートウェイ構成に変更
- RDSの `deletion_protection = true` / `skip_final_snapshot = false` への変更、Multi-AZ化
- ECSサービスのオートスケーリング設定
- RDSマスターパスワードのローテーション設定
