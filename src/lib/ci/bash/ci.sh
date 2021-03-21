#!/usr/bin/env bash
# shellcheck disable=SC1090 # Can't follow non-constant source. Use a directive to specify location.

####
# Continuous integration action library for Bash.
#   Contains "action" functions which perform steps in a continuous integration workflow.
#   By convention, each CI action function begins with "ci-".
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Infer this script has been sourced based upon CI_SCRIPT_PATH being non-empty.
if [[ -n "${CI_SCRIPT_PATH:-}" ]]; then
  # Actions are already sourced. Exit.
  (return 0 2>/dev/null) || exit 0
fi

# Context
CI_SCRIPT_PATH="${BASH_SOURCE[0]}"
export CI_SCRIPT_PATH
CI_LIB_ROOT="$(dirname "${CI_SCRIPT_PATH}")"
export CI_LIB_ROOT

function __source_actions() {
  for script in "${CI_LIB_ROOT}"/actions/*.sh; do
    source "${script}"
  done
}

source "${CI_LIB_ROOT}/utils.sh" && __source_actions
