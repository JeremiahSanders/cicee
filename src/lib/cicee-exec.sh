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
# CI_EXEC_IMAGE - Optional ci-exec service image. Overrides $PROJECT_ROOT/ci/Dockerfile.

# shellcheck source=./ci-env-load.sh
source "${LIB_ROOT}/ci-env-load.sh"

"${LIB_ROOT}/ci-env-display.sh" &&
  printf "\n|__\nBeginning Continuous Integration Containerized Execution...\n__\n  | Entrypoint   : %s\n  | Command      : %s\n  | Project Root : %s\n  | CICEE Library: %s\n\n" "${CI_ENTRYPOINT:-}" "${CI_COMMAND:-}" "${PROJECT_ROOT}" "${LIB_ROOT}"

declare -r DOCKERCOMPOSE_DEPENDENCIES="${PROJECT_ROOT}/docker-compose.ci.dependencies.yml"
declare -r DOCKERCOMPOSE_CICEE="${LIB_ROOT}/docker-compose.yml"
declare -r DOCKERCOMPOSE_PROJECT="${PROJECT_ROOT}/docker-compose.ci.project.yml"

# Define the ci-exec service build context
if [[ -f "${PROJECT_ROOT}/ci/Dockerfile" ]]; then
  declare -rx CI_EXEC_CONTEXT="${PROJECT_ROOT}/ci"
else
  declare -rx CI_EXEC_CONTEXT="${LIB_ROOT}"
fi

declare -a COMPOSE_FILE_ARGS=()
# Use project docker-compose as the primary file (by loading it first). Affects docker container name generation.
if [[ -f "${DOCKERCOMPOSE_PROJECT}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT}")
fi
# Add dependencies
if [[ -f "${DOCKERCOMPOSE_DEPENDENCIES}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_DEPENDENCIES}")
fi
# Add CICEE
COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_CICEE}")
# - Import the ci-exec service image source (Dockerfile or image)
if [[ -n "${CI_EXEC_IMAGE:-}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${LIB_ROOT}/docker-compose.image.yml")
else
  COMPOSE_FILE_ARGS+=("--file" "${LIB_ROOT}/docker-compose.dockerfile.yml")
fi
# Re-add project, to load project settings last (to override all other dependencies, e.g., CICEE defaults).
if [[ -f "${DOCKERCOMPOSE_PROJECT}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT}")
fi

__arrange() {
  __build_project_ci() {
    if [[ -f "${PROJECT_ROOT}/ci/Dockerfile" ]]; then
      docker build \
        --rm \
        --file "${PROJECT_ROOT}/ci/Dockerfile" \
        "${PROJECT_ROOT}/ci"
    fi
  }
  __pull_dependencies() {
    docker-compose \
      "${COMPOSE_FILE_ARGS[@]}" \
      pull \
      --ignore-pull-failures \
      --include-deps \
      ci-exec

  }
  # Build and tag the base, library CI environment image. Then build the project-specific environment image. Finally, pull the compose stack.
  __build_project_ci &&
    __pull_dependencies
}

__act() {
  docker-compose \
    "${COMPOSE_FILE_ARGS[@]}" \
    up \
    --abort-on-container-exit \
    --build \
    --renew-anon-volumes \
    --remove-orphans \
    ci-exec
}

__cleanup() {
  docker-compose \
    "${COMPOSE_FILE_ARGS[@]}" \
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
