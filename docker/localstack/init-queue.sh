#!/usr/bin/env bash
# LocalStack起動完了後に自動実行される初期化フック。
# AppHost側の aws-resources.template (SQS::Queue) を docker-compose 環境向けに
# 固定名"MessageQueue"としてローカル再現する。
set -euo pipefail

awslocal sqs create-queue \
  --queue-name MessageQueue \
  --attributes VisibilityTimeout=60

echo "[init-queue] MessageQueue キューを作成しました。"
