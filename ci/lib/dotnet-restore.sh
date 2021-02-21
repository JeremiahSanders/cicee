#!/usr/bin/env bash

###
# Executes dotnet restore.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# Use of alternate base output (bin) and intermediate (obj) paths is to reduce collision with local development environment.
# GenerateAssemblyInfo and GenerateTargetFrameworkAttribute are disabled to prevent CS0579 errors experienced in some environments, e.g., WSL2 Pengwin. See: https://stackoverflow.com/a/63853501/402726

dotnet restore "${PROJECT_ROOT}" \
  -p:BaseIntermediateOutputPath="ci-obj/" \
  -p:BaseOutputPath="ci-bin/" \
  -p:GenerateTargetFrameworkAttribute=false \
  -p:GenerateAssemblyInfo=false \
  --verbosity normal \
  "$@"
