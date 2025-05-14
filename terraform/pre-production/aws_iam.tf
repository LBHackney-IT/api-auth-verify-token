resource "aws_iam_role" "allow_api_gateway_get_role" {
  assume_role_policy = jsonencode({
    "Version" : "2012-10-17",
    "Statement" : [
      {
        "Effect" : "Allow",
        "Principal" : {
          "AWS" : "arn:aws:iam::${data.aws_caller_identity.current.account_id}:root"
        },
        "Action" : "sts:AssumeRole"
      }
    ]
  })

  name = "LBH_Api_Gateway_Allow_GET"
}

resource "aws_iam_role_policy" "allow_api_gateway_get_policy" {
  role   = aws_iam_role.allow_api_gateway_get_role.id
  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "apigateway:GET",
      "Effect": "Allow",
      "Resource": "*",
      "Sid": ""
    }
  ]
}
EOF
}
