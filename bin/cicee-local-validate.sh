#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Build a local CICEE executable and use its exec command to invoke 'ci/bin/validate.sh'.

# Context
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && cd .. && pwd)"
SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"

"${SCRIPT_LOCATION}/cicee-local.sh" exec --harness direct --project-root "${PROJECT_ROOT}" --command "ci/bin/validate.sh" --verbosity "Verbose"
