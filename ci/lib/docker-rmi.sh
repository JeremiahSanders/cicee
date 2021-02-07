#!/usr/bin/env bash

###
# Executes docker rmi.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

docker rmi --force "${DOCKER_IMAGE}"
