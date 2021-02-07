#!/usr/bin/env bash

###
# Executes docker push.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

docker push "${DOCKER_IMAGE}"
