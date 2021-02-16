#!/usr/bin/env bash

###
# Executes a command within the docker-compose-based CI environment.
###

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status,
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

# Context
# PROJECT_ROOT - Project root directory
# LIB_ROOT - CICEE library root directory
# CI_COMMAND - Container's Command
# CI_ENTRYPOINT - Container's Entrypoint
# CI_ENV_INIT - Initialization script

# shellcheck source=./ci-env-load.sh
source "${LIB_ROOT}/ci-env-load.sh"

"${LIB_ROOT}/ci-env-display.sh" &&
    printf "\n|__\nBeginning Continuous Integration Containerized Execution...\n__\n  | Entrypoint   : %s\n  | Command      : %s\n  | Project Root : %s\n  | CICEE Library: %s\n\n" "${CI_ENTRYPOINT:-}" "${CI_COMMAND}" "${PROJECT_ROOT}" "${LIB_ROOT}"

declare -r DOCKERCOMPOSE_DEPENDENCIES="${PROJECT_ROOT}/docker-compose.ci.dependencies.yml"
declare -r DOCKERCOMPOSE_CICEE="${LIB_ROOT}/docker-compose.yml"
declare -r DOCKERCOMPOSE_PROJECT="${PROJECT_ROOT}/docker-compose.ci.project.yml"

declare -a PROJECT_FILE_ARGS=()
if [[ -f "${DOCKERCOMPOSE_DEPENDENCIES}" ]]; then
  PROJECT_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_DEPENDENCIES}")
fi
if [[ -f "${DOCKERCOMPOSE_PROJECT}" ]]; then
  PROJECT_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT}")
fi

__arrange() {
  __build_cicee(){
    docker build \
        --rm \
        --pull \
        --tag "cicee:latest" \
        --file "${LIB_ROOT}/Dockerfile" \
        "${LIB_ROOT}"
  }
  __build_project_ci(){
    docker build \
      --rm \
      --file "${PROJECT_ROOT}/ci/Dockerfile" \
      "${PROJECT_ROOT}/ci"
  }
  __pull_dependencies(){
    docker-compose \
      --file "${DOCKERCOMPOSE_CICEE}" \
      "${PROJECT_FILE_ARGS[@]}" \
      pull \
      --ignore-pull-failures \
      --include-deps \
      ci-exec
    
  }
  # Build and tag the base, library CI environment image. Then build the project-specific environment image. Finally, pull the compose stack.
  __build_cicee &&
    __build_project_ci &&
    __pull_dependencies
}

__act() {
  docker-compose \
    --file "${DOCKERCOMPOSE_CICEE}" \
    "${PROJECT_FILE_ARGS[@]}" \
    up \
    --abort-on-container-exit \
    --build \
    --renew-anon-volumes \
    --remove-orphans \
    ci-exec
}

__cleanup() {
  docker-compose \
    --file "${DOCKERCOMPOSE_CICEE}" \
    "${PROJECT_FILE_ARGS[@]}" \
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
