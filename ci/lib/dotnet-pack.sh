#!/usr/bin/env bash

###
# Executes dotnet pack.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh).
###

# Use of alternate base intermediate (obj) path is to reduce collision with local development environment.
# GenerateAssemblyInfo and GenerateTargetFrameworkAttribute are disabled to prevent CS0579 errors experienced in some environments, e.g., WSL2 Pengwin. See: https://stackoverflow.com/a/63853501/402726

dotnet pack "${PROJECT_ROOT}/src" \
  --configuration Release \
  --output "${PROJECT_ROOT}/build/dist/" \
  -p:BaseIntermediateOutputPath="ci-obj/" \
  -p:DocumentationFile="${PROJECT_ROOT}/build/docs/${PROJECT_NAME}-${PROJECT_VERSION_DOTNET}.xml" \
  -p:GenerateTargetFrameworkAttribute=false \
  -p:GenerateAssemblyInfo=false \
  -p:Version="${PROJECT_VERSION_DOTNET}" \
  "$@"
