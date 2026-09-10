# MessagesController が SendMessage 先として参照するSQSキュー。
# ローカル(docker-compose+LocalStack)の MessageQueue に相当する。
resource "aws_sqs_queue" "messages" {
  name = "${var.project_name}-message-queue"

  tags = {
    Name = "${var.project_name}-message-queue"
  }
}
