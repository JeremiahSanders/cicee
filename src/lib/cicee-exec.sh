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

# Context (provided by CICEE)
# --------
# PROJECT_NAME - Project name
# PROJECT_ROOT - Project root directory
# LIB_ROOT - CICEE library root directory
# CI_COMMAND - Container's Command
# CI_ENTRYPOINT - Container's Entrypoint
# CI_EXEC_CONTEXT - ci-exec service build context. Defaults to $PROJECT_ROOT/ci. If directory exists and contains a Dockerfile, that is used.
# CI_EXEC_IMAGE - Optional ci-exec service image. Overrides $CI_EXEC_CONTEXT/Dockerfile.

# Declare fallback context values, supporting execution outside CICEE (using library scripts).
#   Default project root to present working directory.
PROJECT_ROOT="${PROJECT_ROOT:-$(pwd)}"
#   Default project name to project root directory name.
PROJECT_NAME="${PROJECT_NAME:-$(basename ${PROJECT_ROOT})}"
#   Default CI library root to: $PROJECT_ROOT/ci/lib
LIB_ROOT="${LIB_ROOT:-${PROJECT_ROOT}/ci/lib}"
#   Default ci-exec service build context to $PROJECT_ROOT/ci.
CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-${PROJECT_ROOT}/ci}"

printf "\n|__\nBeginning Continuous Integration Containerized Execution...\n__\n"
printf "  | Entrypoint   : %s\n" "${CI_ENTRYPOINT:-}"
printf "  | Command      : %s\n" "${CI_COMMAND:-}"
printf "  | Project Root : %s\n" "${PROJECT_ROOT}"
printf "  | CICEE Library: %s\n" "${LIB_ROOT}"
printf "  | Image        : %s\n" "${CI_EXEC_IMAGE:-Not applicable. Using Dockerfile.}"
printf "\n\n"

# Load CI library and initialize the CI environment.
#   Initializing a CI environment enables project- and local-environment initialization scripts.
source "${LIB_ROOT}/ci/bash/ci.sh" && ci-env-init

declare -r DOCKERCOMPOSE_DEPENDENCIES_CI="${PROJECT_ROOT}/ci/docker-compose.dependencies.yml"
declare -r DOCKERCOMPOSE_DEPENDENCIES_ROOT="${PROJECT_ROOT}/docker-compose.ci.dependencies.yml"
declare -r DOCKERCOMPOSE_CICEE="${LIB_ROOT}/docker-compose.yml"
declare -r DOCKERCOMPOSE_PROJECT_CI="${PROJECT_ROOT}/ci/docker-compose.project.yml"
declare -r DOCKERCOMPOSE_PROJECT_ROOT="${PROJECT_ROOT}/docker-compose.ci.project.yml"

declare -a COMPOSE_FILE_ARGS=()
# Use project docker-compose as the primary file (by loading it first). Affects docker container name generation.
if [[ -f "${DOCKERCOMPOSE_PROJECT_CI}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT_CI}")
else
  if [[ -f "${DOCKERCOMPOSE_PROJECT_ROOT}" ]]; then
    COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT_ROOT}")
  fi
fi
# Add dependencies
if [[ -f "${DOCKERCOMPOSE_DEPENDENCIES_CI}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_DEPENDENCIES_CI}")
else
  if [[ -f "${DOCKERCOMPOSE_DEPENDENCIES_ROOT}" ]]; then
    COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_DEPENDENCIES_ROOT}")
  fi
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
if [[ -f "${DOCKERCOMPOSE_PROJECT_CI}" ]]; then
  COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT_CI}")
else
  if [[ -f "${DOCKERCOMPOSE_PROJECT_ROOT}" ]]; then
    COMPOSE_FILE_ARGS+=("--file" "${DOCKERCOMPOSE_PROJECT_ROOT}")
  fi
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
    # Explicit empty string default applied to prevent Docker Compose from reporting that it is defaulting to empty strings.
    LIB_ROOT="${LIB_ROOT:-}" \
      CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-}" \
      CI_ENTRYPOINT="${CI_ENTRYPOINT:-}" \
      CI_COMMAND="${CI_COMMAND:-}" \
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
  # Explicit empty string default applied to prevent Docker Compose from reporting that it is defaulting to empty strings.
  LIB_ROOT="${LIB_ROOT:-}" \
    CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-}" \
    CI_ENTRYPOINT="${CI_ENTRYPOINT:-}" \
    CI_COMMAND="${CI_COMMAND:-}" \
    COMPOSE_PROJECT_NAME="${PROJECT_NAME}" \
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
  # Explicit empty string default applied to prevent Docker Compose from reporting that it is defaulting to empty strings.
  LIB_ROOT="${LIB_ROOT:-}" \
    CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-}" \
    CI_ENTRYPOINT="${CI_ENTRYPOINT:-}" \
    CI_COMMAND="${CI_COMMAND:-}" \
    COMPOSE_PROJECT_NAME="${PROJECT_NAME}" \
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
