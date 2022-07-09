#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Validate the project's source, e.g. run tests, linting.
#
#   This script expects a CICEE CI library environment (which is provided when using 'cicee lib exec').
#   For CI library environment details, see: https://github.com/JeremiahSanders/cicee/blob/main/docs/use/ci-library.md
#
# How to use:
#   Modify the "ci-validate" function, below, to execute the steps required to produce the project's artifacts.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

function ci-validate() {
  printf "Beginning validation...\n\n"

  # How to use:
  #   Uncomment the example validation workflow line(s) below which apply to the project, or execute validation commands.

  printf "...\nTODO: Implement ci-validate in %s ...\n\n" "${BASH_SOURCE[0]}"

  # .NET _______
  #  ci-dotnet-restore &&
  #    ci-dotnet-build &&
  #    ci-dotnet-test

  # Node.js ____
  #  npm ci &&
  #    npm run build &&
  #    npm run test

  printf "Validation complete!\n\n"
}

export -f ci-validate
