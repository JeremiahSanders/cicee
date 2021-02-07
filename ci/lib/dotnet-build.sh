#!/usr/bin/env bash

###
# Executes dotnet build.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# Use of alternate base output (bin) and intermediate (obj) paths is to reduce collision with local development environment.
# GenerateDocumentationFile is used without specified output file so default generation occurs (generated within project output).
# GenerateAssemblyInfo and GenerateTargetFrameworkAttribute are disabled to prevent CS0579 errors experienced in some environments, e.g., WSL2 Pengwin. See: https://stackoverflow.com/a/63853501/402726

dotnet build "${PROJECT_ROOT}" \
  --verbosity normal \
  -p:BaseIntermediateOutputPath="ci-obj/" \
  -p:BaseOutputPath="ci-bin/" \
  -p:GenerateAssemblyInfo=false \
  -p:GenerateDocumentationFile=true \
  -p:GenerateTargetFrameworkAttribute=false \
  -p:Version="${PROJECT_VERSION_DOTNET}" \
  "$@"
