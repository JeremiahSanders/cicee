#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Execute the normal cicee-exec.sh entrypoint (normally used by CICEE) from current source, providing the variables which CICEE would normally provide.

PROJECT_NAME="cicee" \
    PROJECT_ROOT="$(pwd)" \
    LIB_ROOT="$(pwd)/src/lib" \
    CI_COMMAND="ci/bin/publish.sh" \
    CI_EXEC_CONTEXT="$(pwd)/ci" \
    src/lib/cicee-exec.sh
