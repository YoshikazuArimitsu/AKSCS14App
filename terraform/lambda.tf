# SQSProcessor(Lambda)。SQSメッセージを受け取りPostgreSQL(RDS)へ書き込む。
# aws_lambda_event_source_mapping により、キューにメッセージが積まれるたびに
# Lambdaサービス自身が継続的にポーリング(long polling)して自動起動する
# （固定cron等での定期実行ではなく、ローカル(Aspire)の .WithSQSEventSource と同じ仕組み）。

resource "aws_ecr_repository" "sqs_processor" {
  name                 = "${var.project_name}-sqs-processor"
  image_tag_mutability = "MUTABLE"
  force_delete         = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "${var.project_name}-sqs-processor"
  }
}

data "aws_iam_policy_document" "lambda_assume_role" {
  statement {
    effect  = "Allow"
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "sqs_processor" {
  name               = "${var.project_name}-sqs-processor-role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role.json
}

# CloudWatch Logsへの書き込み権限に加え、VPCアタッチ(ENI作成)に必要な権限も含む
resource "aws_iam_role_policy_attachment" "sqs_processor_vpc_access" {
  role       = aws_iam_role.sqs_processor.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}

# イベントソースマッピングによるポーリング(ReceiveMessage/DeleteMessage)に必要な権限
data "aws_iam_policy_document" "sqs_processor_sqs" {
  statement {
    effect = "Allow"
    actions = [
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
    ]
    resources = [aws_sqs_queue.messages.arn]
  }
}

resource "aws_iam_role_policy" "sqs_processor_sqs" {
  name   = "${var.project_name}-sqs-processor-sqs-policy"
  role   = aws_iam_role.sqs_processor.id
  policy = data.aws_iam_policy_document.sqs_processor_sqs.json
}

resource "aws_cloudwatch_log_group" "sqs_processor" {
  name              = "/aws/lambda/${var.project_name}-sqs-processor"
  retention_in_days = 7
}

resource "aws_lambda_function" "sqs_processor" {
  function_name = "${var.project_name}-sqs-processor"
  role          = aws_iam_role.sqs_processor.arn

  package_type = "Image"
  image_uri    = "${aws_ecr_repository.sqs_processor.repository_url}:${var.container_image_tag}"

  timeout     = 30
  memory_size = 512

  vpc_config {
    subnet_ids         = aws_subnet.private[*].id
    security_group_ids = [aws_security_group.lambda_sqs_processor.id]
  }

  environment {
    variables = {
      ConnectionStrings__messagesdb = "Host=${aws_db_instance.postgres.address};Port=${aws_db_instance.postgres.port};Database=${var.db_name};Username=${var.db_username};Password=${random_password.db_master.result}"
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.sqs_processor_vpc_access,
    aws_cloudwatch_log_group.sqs_processor,
  ]
}

resource "aws_lambda_event_source_mapping" "sqs_processor" {
  event_source_arn = aws_sqs_queue.messages.arn
  function_name    = aws_lambda_function.sqs_processor.arn
  batch_size       = 10
}
