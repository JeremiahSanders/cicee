#!/usr/bin/env bash

###
# Executes docker build.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

aws ecr get-login-password --region "${AWS_REGION}" \
| docker login --username AWS --password-stdin "${AWS_ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com"
