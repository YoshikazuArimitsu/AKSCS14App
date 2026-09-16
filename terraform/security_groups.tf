# インターネット向けALB用SG。デモのためHTTP(80)を全世界に開放する。
resource "aws_security_group" "alb" {
  name        = "${var.project_name}-alb-sg"
  description = "Internet-facing ALB"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "HTTP from internet"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-alb-sg"
  }
}

# ECS(Fargate)タスク用SG。ALBからのコンテナポート宛通信のみ許可する。
resource "aws_security_group" "ecs_service" {
  name        = "${var.project_name}-ecs-sg"
  description = "Fargate service for API"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "From ALB"
    from_port       = var.container_port
    to_port         = var.container_port
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-ecs-sg"
  }
}

# SQSProcessor(Lambda)用SG。VPC内(プライベートサブネット)からRDSへ接続するために使用する。
resource "aws_security_group" "lambda_sqs_processor" {
  name        = "${var.project_name}-sqs-processor-sg"
  description = "SQSProcessor Lambda (VPC-attached, for RDS access)"
  vpc_id      = aws_vpc.main.id

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-sqs-processor-sg"
  }
}

# RDS用SG。ECSタスクSG・LambdaSGからのPostgreSQL(5432)宛通信のみ許可する。
resource "aws_security_group" "rds" {
  name        = "${var.project_name}-rds-sg"
  description = "PostgreSQL RDS"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "PostgreSQL from ECS tasks"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_service.id]
  }

  ingress {
    description     = "PostgreSQL from SQSProcessor Lambda"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.lambda_sqs_processor.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-rds-sg"
  }
}
