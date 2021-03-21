#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Build and publish the project's artifact composition.
#
# How to use:
#   Customize the "ci-compose" and "ci-publish" workflows (functions) defined in ci-workflows.sh.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

__initialize() {
  declare SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"
  # Load the CICEE continuous integration action library and project CI workflow library.
  # Then execute the ci-env-init, ci-env-display, and ci-env-require functions, provided by the CICEE action library.
  source "$(cicee lib)" &&
    source "${SCRIPT_LOCATION}/ci-workflows.sh" &&
    ci-env-init &&
    ci-env-display &&
    ci-env-require
}

# Execute the initialization function, defined above, and ci-compose and ci-publish functions, defined in ci-workflows.sh.
__initialize &&
  printf "Composing build artifacts...\n\n" &&
  ci-compose &&
  printf "Composition complete.\n" &&
  printf "Publishing composed artifacts...\n\n" &&
  ci-publish &&
  printf "Publishing complete.\n\n"
