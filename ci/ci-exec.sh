#!/usr/bin/env bash

###
# Executes a command within the docker-compose-based CI environment.
#   (CICEE v1.1.0)
###

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status,
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

# Context
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && cd .. && pwd)"

source "${PROJECT_ROOT}/ci/lib/ci-env-load.sh"

if [[ $# -eq 0 ]]; then
  printf "No execution command provided.\nProvide one string argument to be used as docker execution command.\n"
  exit 1
fi

# CI_EXEC_CMD is the command which will be executed (in docker-compose.yml).
declare -x CI_EXEC_CMD="${1}"
printf "Executing docker-compose CI command:\n%s\n" "${CI_EXEC_CMD}"

__arrange() {
  # Build and tag the base, library CI environment image. Then build the project-specific environment image. Finally, pull the compose stack.
  docker build \
    --rm \
    --pull \
    --tag "cicee:1.1.0" \
    --file "${PROJECT_ROOT}/ci/lib/Dockerfile" \
    "${PROJECT_ROOT}/ci/lib" &&
    docker tag \
      "cicee:1.1.0" \
      "cicee:latest" &&
    docker build \
      --rm \
      --tag "${PROJECT_NAME}-ci:${PROJECT_VERSION_DOTNET}" \
      --file "${PROJECT_ROOT}/ci/Dockerfile" \
      "${PROJECT_ROOT}/ci" &&
    docker-compose \
      --file "${PROJECT_ROOT}/docker-compose.ci.dependencies.yml" \
      --file "${PROJECT_ROOT}/docker-compose.ci.cicee.yml" \
      --file "${PROJECT_ROOT}/docker-compose.ci.project.yml" \
      pull \
      --ignore-pull-failures \
      --include-deps \
      ci-exec
}

__act() {
  docker-compose \
    --file "${PROJECT_ROOT}/docker-compose.ci.dependencies.yml" \
    --file "${PROJECT_ROOT}/docker-compose.ci.cicee.yml" \
    --file "${PROJECT_ROOT}/docker-compose.ci.project.yml" \
    up \
    --abort-on-container-exit \
    --build \
    --renew-anon-volumes \
    --remove-orphans \
    ci-exec
}

__cleanup() {
  docker-compose \
    --file "${PROJECT_ROOT}/docker-compose.ci.dependencies.yml" \
    --file "${PROJECT_ROOT}/docker-compose.ci.cicee.yml" \
    --file "${PROJECT_ROOT}/docker-compose.ci.project.yml" \
    down \
    --volumes \
    --remove-orphans
}

{
  __arrange &&
    __act &&
    __cleanup
} || {
  # Failed execution. Cleanup and exit with failure.
  __cleanup
  exit 1
}
