#!/usr/bin/env bash

###
# Executes 'cdk synth' with default context.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

cdk synth --output "${PROJECT_ROOT}/build/cloud-assembly" "$@"
