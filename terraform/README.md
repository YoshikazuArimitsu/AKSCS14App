# AWSデモ環境（Terraform）

AKSCS14App をAWS上のデモ環境として動かすためのTerraformコード。
PostgreSQLはRDS、APIはFargate、インターネット向けALBで構成する。

## 構成概要

- **VPC**: パブリックサブネット×2（ALB・Fargateタスク）、プライベートサブネット×2（RDS）
- **ALB**: インターネット向け、HTTP:80 で待ち受け、Fargateタスク（ポート8080）へフォワード
- **ECS(Fargate)**: `AKSCS14App.Api` コンテナを実行。コスト削減のためNATゲートウェイは作らず、
  タスクをパブリックサブネットに直接配置（インバウンドはセキュリティグループでALBのみに制限）
- **RDS(PostgreSQL)**: プライベートサブネットに配置し、ECSタスクのセキュリティグループからのみ接続可能。
  マスターパスワードは `manage_master_user_password` によりSecrets Managerで自動管理
- **SQS**: `MessagesController` の送信先キューを作成し、ECSタスクロールに送信権限を付与
- **ECR**: APIコンテナイメージ用リポジトリ

現状のアプリコードはAPIから直接PostgreSQLへ接続していない（RDSはSQSProcessor(Lambda)用途を見据えて用意）ため、
接続文字列はコンテナ環境変数に含めていない。RDSのエンドポイント・認証情報は `terraform output` で確認できる。

## 事前準備

- Terraform >= 1.5
- AWS CLIの認証情報（対象アカウント・リージョンへのデプロイ権限）
- Docker（APIコンテナイメージのビルド用）

## デプロイ手順

```bash
cd terraform
terraform init
terraform apply
```

`aws_ecs_service` はタスク起動時に有効なコンテナイメージを要求するため、
初回applyでは先に `terraform apply -target=aws_ecr_repository.api` などでECRリポジトリのみ作成し、
イメージをpushしてから残りのリソースをapplyする方法でも良い。
もしくは、一度applyが失敗（タスク起動失敗）してもECSサービス自体は作成されるため、
イメージpush後にサービスを再デプロイ（後述）すれば問題ない。

### コンテナイメージのビルド & push

```bash
# リポジトリルート（AKSCS14App/）で実行
ECR_URL=$(terraform -chdir=terraform output -raw ecr_repository_url)

# リージョンは variables.tf の aws_region（既定: ap-northeast-1）に合わせる
aws ecr get-login-password --region ap-northeast-1 | docker login --username AWS --password-stdin "${ECR_URL%/*}"

docker build -f src/AKSCS14App.Api/Dockerfile -t "${ECR_URL}:latest" .
docker push "${ECR_URL}:latest"
```

### 新しいイメージをデプロイに反映

```bash
aws ecs update-service \
  --cluster "$(terraform -chdir=terraform output -raw ecs_cluster_name)" \
  --service akscs14app-api \
  --force-new-deployment
```

## デモ確認

```bash
terraform output alb_dns_name
```

出力されたDNS名にブラウザでアクセスし、`http://<ALBのDNS名>/swagger` でSwagger UIが表示されれば成功。

## 破棄

```bash
terraform destroy
```

## 本番転用時の注意（デモ構成からの変更点）

- ALBをHTTPS化（ACM証明書 + 443番リスナー）
- Fargateタスクをプライベートサブネット + NATゲートウェイ構成に変更
- RDSの `deletion_protection = true` / `skip_final_snapshot = false` への変更、Multi-AZ化
- ECSサービスのオートスケーリング設定
