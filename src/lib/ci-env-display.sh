#!/usr/bin/env bash

# shellcheck disable=SC2155

###
# Display the project's continuous integration environment.
###

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status,
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

printf "Project           :  %s\n" "${PROJECT_NAME}"
printf "  Title           :  %s\n" "${PROJECT_TITLE}"
printf "  Version         :  %s\n" "${PROJECT_VERSION}"
printf "    Distribution  :  %s\n" "${PROJECT_VERSION_DIST}"
printf "    SemVer        :  %s\n" "${PROJECT_VERSION_SEMVER}"
printf "  Root            :  %s\n\n" "${PROJECT_ROOT}"

printf "Git branch        :  %s\n" "${CURRENT_GIT_BRANCH}"
printf "Git hash          :  %s\n\n" "${CURRENT_GIT_HASH}"

printf "Docker image      :  %s\n" "${DOCKER_IMAGE}"
printf "  Repository      :  %s\n" "${DOCKER_IMAGE_REPOSITORY}"
printf "  Tag             :  %s\n\n" "${DOCKER_IMAGE_TAG}"
