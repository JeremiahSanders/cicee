#!/usr/bin/env bash

###
# Clean the build output.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# --recursive --force --verbose
rm -rfv "${PROJECT_ROOT}/build/"
