#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Require that one or more environment variables are set. Required variables are provided as string arguments. E.g. "PROJECT_ROOT", "NPM_API_KEY".
require-var() {
  for variableName in "$@"; do
    # The ! below performs 'indirect expansion'. See: https://www.gnu.org/software/bash/manual/html_node/Shell-Parameter-Expansion.html
    if [[ -z "${!variableName:-}" ]]; then
      printf "Required variable not set: %s\n\n" "${variableName}" &&
        return 1
    fi
  done
}

export -f require-var
