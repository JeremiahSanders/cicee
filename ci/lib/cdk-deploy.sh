#!/usr/bin/env bash

###
# Executes 'cdk deploy' with default context.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

cdk deploy --app "${PROJECT_ROOT}/build/cloud-assembly" "$@"
