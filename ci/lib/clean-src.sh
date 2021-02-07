#!/usr/bin/env bash

###
# Clean the bin and obj of 'src'.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# --recursive --force --verbose
rm -rfv "${PROJECT_ROOT}/src/bin/" "${PROJECT_ROOT}/src/obj/" "${PROJECT_ROOT}/src/ci-bin/" "${PROJECT_ROOT}/src/ci-obj/"
