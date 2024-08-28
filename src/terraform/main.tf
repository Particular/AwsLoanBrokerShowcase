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
  region = "us-west-2"
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

resource "aws_lambda_permission" "allow_public_lambda_invoke" {
  statement_id  = "FunctionURLAllowPublicAccess"
  action        = "lambda:InvokeFunctionUrl"
  function_name = aws_lambda_function.loan_broker_credit_score_lambda.function_name
  principal     = "*"
  function_url_auth_type = "NONE"
}

resource "aws_lambda_function_url" "loan_broker_credit_score_lambda_url" {
  function_name = aws_lambda_function.loan_broker_credit_score_lambda.function_name
  authorization_type = "NONE"

  cors {
    allow_credentials = true
    allow_origins = ["*"]
    allow_methods = ["*"]
    allow_headers = ["date", "keep-alive"]
    expose_headers = ["keep-alive", "date"]
    max_age           = 86400
  }
}


# Create docker containers for applications
# invoke transport cli to configure the transport
# create an empty dynamoDB