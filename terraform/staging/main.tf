terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

terraform {
  backend "s3" {
    bucket  = "terraform-state-staging-apis"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/api-authenticator/state"
  }
}


resource "aws_dynamodb_table" "api_authenticator_dynamodb_table" {
  name         = "APIAuthenticatorData"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "apiName"
  range_key    = "environment"

  attribute {
    name = "apiName"
    type = "S"
  }
  attribute {
    name = "environment"
    type = "S"
  }

  tags = {
    Name              = "api-authenticator-staging"
    Environment       = "stg"
    terraform-managed = true
    project_name      = "api-authenticator"
    Application       = "API Authenticator"
    TeamEmail         = "developementteam@hackney.gov.uk"
  }

  global_secondary_index {
    name            = "apiGatewayIdIndex"
    hash_key        = "apiGatewayId"
    projection_type = "ALL"
  }

  attribute {
    name = "apiGatewayId"
    type = "S"
  }
}

