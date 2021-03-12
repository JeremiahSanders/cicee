#!/usr/bin/env bash
# shellcheck disable=SC2155

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
  # Load the project's CI action and CI workflow libraries.
  # Then execute the ci-EnvInit function, defined in ci-actions.sh.
  . "${SCRIPT_LOCATION}/ci-actions.sh" &&
    . "${SCRIPT_LOCATION}/ci-workflows.sh" &&
    ci-EnvInit &&
    ci-EnvDisplay
}

# Execute the initialization function, defined above, and ci-compose function, defined in ci-workflows.sh.
__initialize &&
  printf "Beginning artifact composition...\n\n" &&
  ci-compose &&
  printf "Composition complete.\n\n"
