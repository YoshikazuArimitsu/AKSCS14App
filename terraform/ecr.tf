# APIコンテナイメージ用リポジトリ。
# `docker build -f src/AKSCS14App.Api/Dockerfile .` で作成したイメージをpushして利用する。
resource "aws_ecr_repository" "api" {
  name                 = "${var.project_name}-api"
  image_tag_mutability = "MUTABLE"
  force_delete         = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "${var.project_name}-api"
  }
}
