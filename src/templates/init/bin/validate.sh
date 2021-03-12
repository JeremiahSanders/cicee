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

__initialize() {
  declare SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"
  # Load the project's CI action and CI workflow libraries.
  # Then execute the ci-EnvInit and ci-EnvDisplay functions, defined in ci-actions.sh, which initialize and display the environment, respectively.
  . "${SCRIPT_LOCATION}/ci-actions.sh" &&
    . "${SCRIPT_LOCATION}/ci-workflows.sh" &&
    ci-EnvInit &&
    ci-EnvDisplay
}

# Execute the initialization function, defined above, and ci-validate function defined in ci-workflows.sh.
__initialize &&
  printf "Beginning validation...\n\n" &&
  ci-validate &&
  printf "Validation complete!\n\n"
