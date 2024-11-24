#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Build and validate the project's source.
#
# How to use:
#   Customize the "ci-validate" workflow (function) defined in ci-workflows.sh.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

declare SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"
declare PROJECT_ROOT="${PROJECT_ROOT:-$(cd "${SCRIPT_LOCATION}/../.." && pwd)}"

__initialize() {
  local targetFramework="net8.0"
  # Publish the application so we can work with CICEE similarly to other projects
  #   NOTE: Previous implementations used `dotnet run -- lib`. This stopped working.
  #     When using `dotnet run` the raw output to STDOUT is prefixed with invisible control characters. Those characters trigger file not found responses from `source <path>`.
  #     However, if the DLL is executed with `dotnet <dll>` then the output of STDOUT lacks the control characters and it can be loaded with `source`.
  dotnet publish "${PROJECT_ROOT}/src" --framework "${targetFramework}"
  # Load the CICEE CI action library and project CI workflow library.
  # Then execute the ci-env-init, ci-env-display, and ci-env-require functions, provided by the CI action library.
  local ciLibPath="$(dotnet "${PROJECT_ROOT}/src/bin/Release/${targetFramework}/publish/cicee.dll" lib)"
  
  printf "Loading CI library entry point: %s\n" "${ciLibPath}" &&
    source "${ciLibPath}" &&
    printf "Loading CI workflows: %s\n\n" "${PROJECT_ROOT}/ci/libexec/ci-workflows.sh" &&
    source "${PROJECT_ROOT}/ci/libexec/ci-workflows.sh" &&
    ci-env-init &&
    ci-env-display &&
    ci-env-require
}

# Execute the initialization function, defined above, and ci-validate function defined in ci-workflows.sh.
__initialize &&
  printf "Beginning validation...\n\n" &&
  ci-validate &&
  printf "Validation complete!\n\n"
