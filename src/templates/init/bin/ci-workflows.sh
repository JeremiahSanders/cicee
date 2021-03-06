#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Project CI Workflow Composition Library.
#   Contains functions which execute the project's high-level continuous integration tasks.
#
# How to use:
#   Update the "workflow compositions" in this file to perform each of the named continuous integration tasks.
#   Add additional workflow functions as needed. Note: Functions must be executed
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Infer this script has been sourced based upon WORKFLOWS_SCRIPT_LOCATION being non-empty.
if [[ -n "${WORKFLOWS_SCRIPT_LOCATION:-}" ]]; then
  # Workflows are already sourced. Exit.
  # Check to see if this script was sourced.
  #   See: https://stackoverflow.com/a/28776166/402726
  (return 0 2>/dev/null) && sourced=1 || sourced=0
  if [[ $sourced -eq 1 ]]; then
    # NOTE: return is used, rather than exit, to prevent shell exit when sourcing from an interactive shell.
    return 0
  else
    exit 0
  fi
fi

# Context
WORKFLOWS_SCRIPT_LOCATION="${BASH_SOURCE[0]}"
declare WORKFLOWS_SCRIPT_DIRECTORY="$(dirname "${WORKFLOWS_SCRIPT_LOCATION}")"
PROJECT_ROOT="${PROJECT_ROOT:-$(cd "${WORKFLOWS_SCRIPT_DIRECTORY}" && cd ../.. && pwd)}"

# Load the CICEE continuous integration action library (by 'cicee lib' or the specific location CICEE mounts it to).
if [[ -n "$(command -v cicee)" ]]; then
  source "$(cicee lib)"
else
  # CICEE mounts the Bash CI action library at /opt/ci-lib/bash/ci.sh.
  source "/opt/ci-lib/bash/ci.sh"
fi

####
# BEGIN Workflow Compositions
#     These commands are executed by CI entrypoint scripts, e.g., publish.sh.
#     By convention, each CI workflow function begins with "ci-".
####

#--
# Validate the project's source, e.g. run tests, linting.
#--
ci-validate() {
  # How to use:
  #   Uncomment the example validation workflow line(s) below which apply to the project, or execute validation commands.

  printf "...\nTODO: Implement ci-validate in %s ...\n\n" "${WORKFLOWS_SCRIPT_LOCATION}"
  # .NET _______
  #  ci-dotnet-restore &&
  #    ci-dotnet-build &&
  #    ci-dotnet-test

  # Node.js ____
  #  npm ci &&
  #    npm run build &&
  #    npm run test
}

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  # How to use:
  #   Uncomment the example composition workflow line(s) below which apply to the project, or execute composition commands.

  printf "...\nTODO: Implement ci-compose in %s ...\n\n" "${WORKFLOWS_SCRIPT_LOCATION}"
  # .NET Library ________________________________
  # ci-dotnet-pack

  # .NET Application distributed as Docker image
  # ci-dotnet-publish && ci-docker-build

  # AWS CDK _____________________________________
  # ci-aws-cdk-synth
}

#--
# Publish the project's artifact composition.
#--
ci-publish() {
  # How to use:
  #   Uncomment the example publishing workflow line(s) below which apply to the project, or execute publishing commands.

  printf "...\nTODO: Implement ci-publish  in %s ...\n\n" "${WORKFLOWS_SCRIPT_LOCATION}"
  # Push Docker image to AWS ECR ______
  # ci-aws-ecr-docker-login && ci-docker-push

  # Push .NET NuGet package ___________
  # ci-dotnet-nuget-push

  # Deploy AWS CDK Cloud Assembly _____
  # ci-aws-cdk-deploy
}

export -f ci-compose
export -f ci-publish
export -f ci-validate

####
# END Workflow Compositions
####
