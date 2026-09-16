# --- 共通 ---

variable "aws_region" {
  description = "リソースを作成するAWSリージョン"
  type        = string
  default     = "ap-northeast-1"
}

variable "project_name" {
  description = "リソース名のプレフィックスとして使用するプロジェクト名"
  type        = string
  default     = "akscs14app"
}

# --- ネットワーク ---

variable "vpc_cidr" {
  description = "VPCのCIDRブロック"
  type        = string
  default     = "10.0.0.0/16"
}

variable "public_subnet_cidrs" {
  description = "ALB・Fargateタスクを配置するパブリックサブネットのCIDRブロック（AZ数分指定）"
  type        = list(string)
  default     = ["10.0.0.0/24", "10.0.1.0/24"]
}

variable "private_subnet_cidrs" {
  description = "RDSを配置するプライベートサブネットのCIDRブロック（AZ数分指定）"
  type        = list(string)
  default     = ["10.0.10.0/24", "10.0.11.0/24"]
}

# --- ECS / Fargate ---

variable "container_port" {
  description = "APIコンテナがリッスンするポート番号（Dockerfileの ASPNETCORE_URLS に合わせる）"
  type        = number
  default     = 8080
}

variable "container_image_tag" {
  description = "ECRにpushするAPIコンテナイメージのタグ"
  type        = string
  default     = "latest"
}

variable "fargate_cpu" {
  description = "FargateタスクのCPUユニット（例: 256 = 0.25 vCPU）"
  type        = string
  default     = "256"
}

variable "fargate_memory" {
  description = "Fargateタスクのメモリ（MiB）"
  type        = string
  default     = "512"
}

variable "desired_count" {
  description = "ECSサービスで起動するタスク数（デモ用途のため既定は1）"
  type        = number
  default     = 1
}

# --- RDS (PostgreSQL) ---

variable "db_name" {
  description = "作成するデータベース名（docker-compose の messagesdb に合わせる）"
  type        = string
  default     = "messagesdb"
}

variable "db_username" {
  description = "RDSマスターユーザー名"
  type        = string
  default     = "postgres"
}

variable "db_engine_version" {
  description = "PostgreSQLのエンジンバージョン"
  type        = string
  default     = "16"
}

variable "db_instance_class" {
  description = "RDSインスタンスクラス（デモ用途のため低コストなクラスを既定値とする）"
  type        = string
  default     = "db.t4g.micro"
}

variable "db_allocated_storage" {
  description = "RDSの割り当てストレージ容量（GB）"
  type        = number
  default     = 20
}
