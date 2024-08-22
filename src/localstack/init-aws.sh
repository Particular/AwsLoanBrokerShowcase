#!/bin/bash
cd /var/lib/lambdas
zip function.zip creditbureau.js
awslocal lambda create-function \
    --function-name score \
    --runtime nodejs18.x \
    --zip-file fileb://function.zip \
    --handler creditbureau.score \
    --role arn:aws:iam::000000000000:role/lambda-role \
    --tags '{"_custom_id_":"score"}'
awslocal lambda create-function-url-config \
    --function-name score \
    --auth-type NONE