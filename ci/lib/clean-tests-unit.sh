#!/usr/bin/env bash

###
# Clean the bin and obj of 'tests/unit'.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# --recursive --force --verbose
rm -rfv "${PROJECT_ROOT}/tests/unit/bin/" "${PROJECT_ROOT}/tests/unit/obj/" "${PROJECT_ROOT}/tests/unit/ci-bin/" "${PROJECT_ROOT}/tests/unit/ci-obj/"
