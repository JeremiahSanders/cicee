#!/usr/bin/env bash

###
# Executes dotnet test on unit test project.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# Use of alternate base output (bin) and intermediate (obj) paths is to reduce collision with local development environment.

dotnet test "${PROJECT_ROOT}/tests/unit" \
    -p:BaseIntermediateOutputPath="ci-obj/" \
    -p:BaseOutputPath="ci-bin/" \
    "$@"
