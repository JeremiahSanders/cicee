#!/usr/bin/env bash

###
# Clean the bin and obj of 'tests/integration'.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# --recursive --force --verbose
rm -rfv "${PROJECT_ROOT}/tests/integration/bin/" "${PROJECT_ROOT}/tests/integration/obj/" "${PROJECT_ROOT}/tests/integration/ci-bin/" "${PROJECT_ROOT}/tests/integration/ci-obj/"
