#!/usr/bin/env bash
# shellcheck disable=SC1090 # ShellCheck can't follow non-constant source. Use a directive to specify location.
# shellcheck disable=SC2155 # Declare and assign separately to avoid masking return values.

###
# Build the project's artifact composition.
#
# How to use:
#   Customize the "ci-compose" workflow (function) defined in ci-workflows.sh.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

__initialize() {
  declare SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"
  # Load the CICEE CI action library and project CI workflow library.
  # Then execute the ci-env-init, ci-env-display, and ci-env-require functions, provided by the CI action library.
  source "$(dotnet run --project src -- lib)" &&
    source "${SCRIPT_LOCATION}/ci-workflows.sh" &&
    ci-env-init &&
    ci-env-display &&
    ci-env-require
}

# Execute the initialization function, defined above, and ci-compose function, defined in ci-workflows.sh.
__initialize &&
  printf "Beginning artifact composition...\n\n" &&
  ci-compose &&
  printf "Composition complete.\n\n"
