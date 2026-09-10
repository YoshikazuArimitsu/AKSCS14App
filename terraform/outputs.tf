output "alb_dns_name" {
  description = "デモアクセス用ALBのDNS名（http://<この値>/swagger でSwagger UIを表示可能）"
  value       = aws_lb.main.dns_name
}

output "ecr_repository_url" {
  description = "APIコンテナイメージをpushするECRリポジトリURL"
  value       = aws_ecr_repository.api.repository_url
}

output "rds_endpoint" {
  description = "RDS(PostgreSQL)の接続エンドポイント"
  value       = aws_db_instance.postgres.endpoint
}

output "rds_master_user_secret_arn" {
  description = "RDSマスターユーザーの認証情報が格納されたSecrets ManagerシークレットのARN"
  value       = aws_db_instance.postgres.master_user_secret[0].secret_arn
}

output "sqs_queue_url" {
  description = "MessagesController が送信先とするSQSキューURL"
  value       = aws_sqs_queue.messages.url
}

output "ecs_cluster_name" {
  description = "ECSクラスタ名"
  value       = aws_ecs_cluster.main.name
}
