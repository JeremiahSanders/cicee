#!/usr/bin/env bash

###
# Build and validate the project's source.
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

source "${PROJECT_ROOT}/ci/lib/ci-env-load.sh" &&
    "${PROJECT_ROOT}/ci/lib/ci-env-display.sh" &&
    printf "Validating %s version %s...\n\n\n" "${PROJECT_NAME}" "${PROJECT_VERSION_DIST}"

#--
# Begin project-specific build steps.
#--

"${PROJECT_ROOT}/ci/lib/dotnet-restore.sh" &&
    "${PROJECT_ROOT}/ci/lib/dotnet-build.sh" &&
    "${PROJECT_ROOT}/ci/lib/dotnet-test.sh" \
        -p:GenerateTargetFrameworkAttribute=false \
        -p:GenerateAssemblyInfo=false
