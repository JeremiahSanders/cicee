#!/usr/bin/env bash
# shellcheck disable=SC2155

####
# Docker continuous integration actions.
#   Contains "action" functions which interact with Docker and Docker Compose.
#
# Exported library functions:
#   * ci-docker-build - Execute 'docker build'.
#   * ci-docker-push - Execute 'docker push'.
#
# Required environment:
#   $DOCKER_IMAGE  - Docker image tag.
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

__tryApplyFallbackDockerEnvironment() {
  require-var "PROJECT_VERSION_DIST" "PROJECT_NAME"
  DOCKER_IMAGE_TAG="${DOCKER_IMAGE_TAG:-${PROJECT_VERSION_DIST}}"
  DOCKER_IMAGE_REPOSITORY="${DOCKER_IMAGE_REPOSITORY:-${PROJECT_NAME}}"
  DOCKER_IMAGE="${DOCKER_IMAGE_REPOSITORY}:${DOCKER_IMAGE_TAG}"
}

# Docker build - requires environment $DOCKER_IMAGE, which should be formatted as a Docker tag.
ci-docker-build() {
  __tryApplyFallbackDockerEnvironment &&
    require-var "DOCKER_IMAGE" &&
    docker build \
      --rm \
      --tag "${DOCKER_IMAGE}" \
      -f "${PROJECT_ROOT}/Dockerfile" \
      "${PROJECT_ROOT}"
}

# Docker push
ci-docker-push() {
  __tryApplyFallbackDockerEnvironment &&
    require-var "DOCKER_IMAGE" &&
    docker push "${DOCKER_IMAGE}"
}

export -f ci-docker-build
export -f ci-docker-push
