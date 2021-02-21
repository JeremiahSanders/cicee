#!/usr/bin/env bash

###
# Executes docker tag, adding a 'latest' tag to the current image.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

DOCKER_LATEST_TAG="${DOCKER_IMAGE_REPOSITORY}:latest"

docker tag \
  "${DOCKER_IMAGE}" \
  "${DOCKER_LATEST_TAG}" \
&& printf "\n\nTagged docker image %s as %s\n\n" "${DOCKER_IMAGE}" "${DOCKER_LATEST_TAG}"
