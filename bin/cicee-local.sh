#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Execute the normal cicee-exec.sh entrypoint (normally used by CICEE) from current source, providing the variables which CICEE would normally provide.

# Context
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && cd .. && pwd)"

function __generate_cicee_binary(){
  targetFramework="net8.0"
  # Publish the application so we can work with CICEE similarly to other projects
  #   NOTE: Previous implementations used `dotnet run -- lib`. This stopped working.
  #     When using `dotnet run` the raw output to STDOUT is prefixed with invisible control characters. Those characters trigger file not found responses from `source <path>`.
  #     However, if the DLL is executed with `dotnet <dll>` then the output of STDOUT lacks the control characters and it can be loaded with `source`.
  rm -rf "${PROJECT_ROOT}/build/cicee-entrypoint" &&
    mkdir -p "${PROJECT_ROOT}/build/cicee-entrypoint" &&
    dotnet publish "${PROJECT_ROOT}/src" --framework "${targetFramework}" --output "${PROJECT_ROOT}/build/cicee-entrypoint"
}

# Now run 'exec' using the Direct harness
__generate_cicee_binary && dotnet "${PROJECT_ROOT}/build/cicee-entrypoint/cicee.dll" $@
