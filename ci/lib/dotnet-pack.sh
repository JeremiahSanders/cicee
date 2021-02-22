#!/usr/bin/env bash

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status,
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

###
# Executes dotnet pack.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh).
###

# Use of alternate base intermediate (obj) path is to reduce collision with local development environment.

# GenerateAssemblyInfo and GenerateTargetFrameworkAttribute may need to be disabled to prevent CS0579 errors experienced
# in some environments, e.g., WSL2 Pengwin. See: https://stackoverflow.com/a/63853501/402726
# However, this causes project assembly information to be omitted, e.g., project version.
#  -p:GenerateAssemblyInfo=false \
#  -p:GenerateTargetFrameworkAttribute=false \

dotnet pack "${PROJECT_ROOT}/src" \
  --configuration Release \
  --output "${PROJECT_ROOT}/build/dist/" \
  -p:BaseIntermediateOutputPath="ci-obj/" \
  -p:DocumentationFile="${PROJECT_ROOT}/build/docs/${PROJECT_NAME}-${PROJECT_VERSION_DOTNET}.xml" \
  -p:PackageVersion="${PROJECT_VERSION_DOTNET}" \
  -p:Version="${PROJECT_VERSION_DOTNET}" \
  "$@"
