#!/usr/bin/env bash
# shellcheck disable=SC1090
# shellcheck disable=SC2155

###
# Initializes a CI environment and executes the arguments passed to this script (using 'eval').
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

declare SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"

# Load the CICEE continuous integration action library.
source "${SCRIPT_LOCATION}/ci.sh"

RED='\033[0;31m'
CYAN='\033[0;36m'
PURPLE='\033[0;35m'
UNCOLORED='\033[0m'

# Initialize an empty string, for displaying script arguments.
args=""

# Loop over all arguments
for arg in "$@"
do
    # Append each argument to the string, separated by a space
    args+="$arg "
done

# Load project CI actions and workflows, if they exist
if [[ -d "${PROJECT_ROOT:-}/ci/libexec/actions" ]]; then
  for script in "${PROJECT_ROOT:-}"/ci/libexec/actions/*.sh; do
    source "${script}"
    printf "${CYAN}Sourced project CI action:${UNCOLORED} %s\n" "${script}"
  done
fi
if [[ -d "${PROJECT_ROOT:-}/ci/libexec/workflows" ]]; then
  for script in "${PROJECT_ROOT:-}"/ci/libexec/workflows/*.sh; do
    source "${script}"
    printf "${PURPLE}Sourced project CI workflow:${UNCOLORED} %s\n" "${script}"
  done
fi
if [[ -f "${PROJECT_ROOT:-}/ci/libexec/ci-workflows.sh" ]]; then
  source "${PROJECT_ROOT:-}/ci/libexec/ci-workflows.sh"
  printf "${PURPLE}Sourced project CI workflows:${UNCOLORED} %s\n\n" "${PROJECT_ROOT:-}/ci/libexec/ci-workflows.sh"
fi

# 1 - Execute ci-env-init, provided by the CICEE action library.
# 2 - Execute ci-env-display, provided by the CICEE action library.
# 3 - Execute ci-env-require, provided by the CICEE action library.
# 4 - Change directory into the project root.
# 5 - Execute the arguments passed to this script.

ci-env-init &&
  ci-env-display &&
  ci-env-require &&
  printf "\nCI execution command:\n  %s\n\n" "${args}" &&
  cd "${EXEC_DIR:-${PROJECT_ROOT}}"
  eval "$@"
