#!/usr/bin/env bash

###
# Clean the project's artifacts.
###

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status, 
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

# Context
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && cd ../.. && pwd)"

rm -rfv \
  "${PROJECT_ROOT}/build" \
  "${PROJECT_ROOT}/src/bin" \
  "${PROJECT_ROOT}/src/obj" \
  "${PROJECT_ROOT}/tests/unit/bin" \
  "${PROJECT_ROOT}/tests/unit/obj" \
  "${PROJECT_ROOT}/tests/integration/bin" \
  "${PROJECT_ROOT}/tests/integration/obj" &&
  printf "\n\nBuild artifacts cleaned.\n\n\n"
