
locals {
  environment = "pre-production"
}

resource "aws_ssm_parameter" "postgres_hostname" {
  name  = "api-auth-token-generator/${local.environment}/postgres-hostname"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "postgres_port" {
  name  = "api-auth-token-generator/${local.environment}/postgres-port"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "postgres_username" {
  name  = "api-auth-token-generator/${local.environment}/postgres-username"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "postgres_password" {
  name  = "api-auth-token-generator/${local.environment}/postgres-password"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "token_secret" {
  name  = "api-auth-token-generator/${local.environment}/token-secret"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "hackney_jwt_secret" {
  name  = "common/hackney-jwt-secret"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "sts_role_name" {
  name  = "api-auth-token-generator/${local.environment}/sts-role-name"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}
