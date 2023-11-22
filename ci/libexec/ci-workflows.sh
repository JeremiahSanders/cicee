#!/usr/bin/env bash
# shellcheck disable=SC1090 # ShellCheck can't follow non-constant source. Use a directive to specify location.
# shellcheck disable=SC2155 # Declare and assign separately to avoid masking return values.

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
  (return 0 2>/dev/null) || exit 0
fi

# Context

WORKFLOWS_SCRIPT_LOCATION="${BASH_SOURCE[0]}"
declare WORKFLOWS_SCRIPT_DIRECTORY="$(dirname "${WORKFLOWS_SCRIPT_LOCATION}")"
PROJECT_ROOT="${PROJECT_ROOT:-$(cd "${WORKFLOWS_SCRIPT_DIRECTORY}" && cd ../.. && pwd)}"

# Load the CI action library.
source "$(dotnet run --project src --framework net6.0 -- lib)"

####
#-- BEGIN Workflow Compositions
#     These commands are executed by CI entrypoint scripts, e.g., publish.sh.
#     By convention, each CI workflow function begins with "ci-".
####

#--
# Validate the project's source, e.g. run tests, linting.
#--
ci-validate() {
  ci-dotnet-restore &&
    printf "\nBeginning 'dotnet build'...\n\n" &&
    ci-dotnet-build \
      -p:GenerateDocumentationFile=true &&
    printf "\nBeginning 'dotnet test'...\n\n" &&
    ci-dotnet-test
}

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  printf "\nBeginning 'dotnet restore'...\n\n" &&
    ci-dotnet-restore &&
    printf "\nBeginning 'dotnet publish' targeting .NET 6...\n\n" &&
    dotnet publish "${PROJECT_ROOT}/src" \
      --configuration Release \
      --output "${BUILD_UNPACKAGED_DIST}/net6.0" \
      -p:Version="${PROJECT_VERSION_DIST}" \
      --framework net6.0 &&
    printf "\nBeginning 'dotnet publish' targeting .NET 7...\n\n" &&
    dotnet publish "${PROJECT_ROOT}/src" \
      --configuration Release \
      --output "${BUILD_UNPACKAGED_DIST}/net7.0" \
      -p:Version="${PROJECT_VERSION_DIST}" \
      --framework net7.0 &&
    printf "\nBeginning 'dotnet publish' targeting .NET 8...\n\n" &&
    dotnet publish "${PROJECT_ROOT}/src" \
      --configuration Release \
      --output "${BUILD_UNPACKAGED_DIST}/net8.0" \
      -p:Version="${PROJECT_VERSION_DIST}" \
      --framework net8.0 \
      -p:GenerateDocumentationFile=true &&
    printf "\nCompleted 'dotnet publish' targeting .NET 8.\n\n" &&
    printf "\nBeginning 'dotnet pack'...\n\n" &&
    ci-dotnet-pack -p:GenerateDocumentationFile=true &&
    printf "\nCompleted 'dotnet pack'.\n\n"
}

#--
# Publish the project's artifact composition.
#--
ci-publish() {
  ci-dotnet-nuget-push
}

export -f ci-compose
export -f ci-publish
export -f ci-validate

####
#-- END Workflow Compositions
####
