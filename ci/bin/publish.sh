#!/usr/bin/env bash

###
# Publish the project's artifact composition.
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
    printf "Publishing %s version %s...\n\n\n" "${PROJECT_NAME}" "${PROJECT_VERSION_DIST}"

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
__compose() {
    # .NET Project publish
     "${PROJECT_ROOT}/ci/lib/dotnet-publish.sh" &&
         "${PROJECT_ROOT}/ci/lib/dotnet-pack.sh"

    # Docker build
    # "${PROJECT_ROOT}/ci/lib/docker-build.sh"

    # AWS CDK synth
    # "${PROJECT_ROOT}/ci/lib/cdk-synth.sh"

    printf "Composition complete.\n\n"
}

#--
# Publish the project's artifact composition.
#--
__publish() {
    # AWS ECR login
    # "${PROJECT_ROOT}/ci/lib/docker-login-aws-ecr-get-login-password.sh"

    # Docker push
    # "${PROJECT_ROOT}/ci/lib/docker-push.sh"

    # AWS CDK deploy
    # "${PROJECT_ROOT}/ci/lib/cdk-deploy.sh"
    
    printf "Publishing complete.\n\n"
}

if [ "${DRY_RUN:-false}" = true ]; then
    printf "Beginning dry run composition...\n\n"
    __compose
else
    printf "Composing build artifacts...\n\n" &&
        __compose &&
        printf "Publishing composed artifacts...\n\n" &&
        __publish
fi
