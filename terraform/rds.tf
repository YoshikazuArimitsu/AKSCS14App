# PostgreSQL (RDS)。docker-compose の postgres サービス（messagesdb）に相当する。
# マスターパスワードは manage_master_user_password により
# Secrets Manager 上に自動生成・管理させ、tfstate や変数に平文で残さない。

resource "aws_db_subnet_group" "main" {
  name       = "${var.project_name}-db-subnet-group"
  subnet_ids = aws_subnet.private[*].id

  tags = {
    Name = "${var.project_name}-db-subnet-group"
  }
}

resource "aws_db_instance" "postgres" {
  identifier     = "${var.project_name}-postgres"
  engine         = "postgres"
  engine_version = var.db_engine_version

  instance_class    = var.db_instance_class
  allocated_storage = var.db_allocated_storage
  storage_type      = "gp3"
  storage_encrypted = true

  db_name  = var.db_name
  username = var.db_username

  manage_master_user_password = true

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  publicly_accessible    = false
  multi_az               = false

  backup_retention_period = 1
  apply_immediately       = true

  # デモ環境のため、削除しやすいように保護・最終スナップショットを無効化している。
  # 本番運用に転用する場合は deletion_protection = true / skip_final_snapshot = false に変更すること。
  deletion_protection = false
  skip_final_snapshot = true

  tags = {
    Name = "${var.project_name}-postgres"
  }
}
