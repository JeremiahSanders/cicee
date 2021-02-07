#!/usr/bin/env bash

###
# Executes dotnet test.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# Use of alternate base output (bin) and intermediate (obj) paths is to reduce collision with local development environment.

dotnet test "${PROJECT_ROOT}" \
    -p:BaseIntermediateOutputPath="ci-obj/" \
    -p:BaseOutputPath="ci-bin/" \
    "$@"
