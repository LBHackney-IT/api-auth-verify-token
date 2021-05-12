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
    bucket  = "terraform-state-development-apis"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/api-authenticator/state"
  }
}


resource "aws_dynamodb_table" "api_authenticator_dynamodb_table" {
    name                  = "APIAuthenticatorData"
    billing_mode          = "PROVISIONED"
    read_capacity         = 10
    write_capacity        = 10
    hash_key              = "apiName"
    range_key             = "environment"
	
    attribute {
        name              = "apiName"
        type              = "S"
    }
    attribute {
        name              = "environment"
        type              = "S"
    }

    tags = {
        Name              = "api-authenticator-development"
        Environment       = "development"
        terraform-managed = true
        project_name      = "api-authenticator"
    }
}

