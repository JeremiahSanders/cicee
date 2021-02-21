#!/usr/bin/env bash

###
# Executes docker build.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

docker build \
  --rm \
  --tag "${DOCKER_IMAGE}" \
  -f "${PROJECT_ROOT}/Dockerfile" \
  "${PROJECT_ROOT}" \
&& printf "\n\nBuilt docker image %s\n\n" "${DOCKER_IMAGE}"
