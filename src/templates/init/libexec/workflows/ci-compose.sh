#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#   This workflow performs all steps required to create the project's output.
#
#   This script expects a CICEE CI library environment (which is provided when using 'cicee lib exec').
#   For CI library environment details, see: https://github.com/JeremiahSanders/cicee/blob/main/docs/use/ci-library.md
#
# How to use:
#   Modify the "ci-compose" function, below, to execute the steps required to produce the project's artifacts. 
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

function ci-compose() {
  printf "Composing build artifacts...\n\n"
  
  # How to use:
  #   Uncomment the example composition workflow line(s) below which apply to the project, or execute composition commands.

  printf "...\nTODO: Implement ci-compose in %s ...\n\n" "${BASH_SOURCE[0]}"

  # .NET Library ________________________________
  # ci-dotnet-pack

  # .NET Application distributed as Docker image
  # ci-dotnet-publish && ci-docker-build

  # AWS CDK _____________________________________
  # ci-aws-cdk-synth
  
  printf "Composition complete.\n"
}

export -f ci-compose
