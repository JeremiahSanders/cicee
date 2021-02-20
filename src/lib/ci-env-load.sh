#!/usr/bin/env bash

# shellcheck disable=SC2155

###
# Load the project's continuous integration environment.
###

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status,
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

# Context
# declare
# -r	Make names readonly. These names cannot then be assigned values by subsequent assignment statements or unset.
# -x	Mark each name for export to subsequent commands via the environment.
declare -x PROJECT_ROOT="${PROJECT_ROOT-:$(cd "$(dirname "${BASH_SOURCE[0]}")" && cd ../.. && pwd)}"

# Source local environment, if available. Enables setting local defaults.
CI_ENV_INIT="${CI_ENV_INIT:-${PROJECT_ROOT}/ci/ci.env}"
if [[ -f "${CI_ENV_INIT}" ]]; then
  source "${CI_ENV_INIT}" &&
    printf "\nSourced '%s'.\n\n" "${CI_ENV_INIT}"
fi

# Application Metadata
PROJECT_METADATA="${PROJECT_ROOT}/.project-metadata.json"
if [ -f "$PROJECT_METADATA" ]; then
  declare -x PROJECT_NAME="$(jq --raw-output 'if .name== null then "unknown-project" else .name end' "${PROJECT_METADATA}")"
  declare -x PROJECT_TITLE="$(jq --raw-output 'if .title== null then "Unknown Project" else .title end' "${PROJECT_METADATA}")"
  declare -x PROJECT_VERSION="$(jq --raw-output 'if .version== null then "0.0.0" else .version end' "${PROJECT_METADATA}")"
else
  declare -x PROJECT_NAME="unknown-project"
  declare -x PROJECT_TITLE="Unknown Project"
  declare -x PROJECT_VERSION="0.0.0"
fi
declare -x CURRENT_GIT_BRANCH="$(git branch | sed -n '/\* /s///p')"
declare -x CURRENT_GIT_HASH="$(git log --pretty=format:'%h' -n 1)"

# PROJECT_VERSION_DIST     - SemVer-like version string using hyphens as separators. Provides compatibility with .NET versions and Docker image tags by excluding plus sign (+) and some .NET/NuGet tool versions dislike the period (.) separator.
# PROJECT_VERSION_SEMVER   - SemVer version string
if [ "${RELEASE_ENVIRONMENT:-false}" = true ]; then
  declare -x PROJECT_VERSION_DIST="${PROJECT_VERSION}"
  declare -x PROJECT_VERSION_SEMVER="${PROJECT_VERSION}"
else
  declare -x PROJECT_VERSION_DIST="${PROJECT_VERSION}-sha-${CURRENT_GIT_HASH}"
  declare -x PROJECT_VERSION_SEMVER="${PROJECT_VERSION}+sha.${CURRENT_GIT_HASH}"
fi
declare -x DOCKER_IMAGE_TAG="${PROJECT_VERSION_DIST}"
declare -x DOCKER_IMAGE_REPOSITORY="${DOCKER_IMAGE_REPOSITORY:-${PROJECT_NAME}}"
declare -x DOCKER_IMAGE="${DOCKER_IMAGE_REPOSITORY}:${DOCKER_IMAGE_TAG}"

printf "\nLoaded %s version %s environment.\n\n\n" "${PROJECT_NAME}" "${PROJECT_VERSION_DIST}"
