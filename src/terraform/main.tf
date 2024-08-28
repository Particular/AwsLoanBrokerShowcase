terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.16"
    }
  }

  required_version = ">= 1.2.0"
}

provider "aws" {
  region  = "us-west-2"
}

resource "aws_ecr_repository" "loan_broker_ecr_repo" {
  name = "loan_broker-repo"
}

resource "aws_iam_role" "iam_for_loan_broker_lambda" {
  name = "iam_for_loan_broker_lambda"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

resource "aws_lambda_function" "loan_broker_credit_score_lambda" {
  filename      = "../lambdas/function.zip"
  function_name = "score"
  role          = aws_iam_role.iam_for_loan_broker_lambda.arn
  handler       = "creditbureau.score"
  runtime       = "nodejs18.x"
  memory_size   = 1024
  timeout       = 300
}


# Create docker containers for applications
# Create labmda
# invoke transport cli to configure the transport
# create an empty dynamoDB