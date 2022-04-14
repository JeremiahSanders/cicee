#!/usr/bin/env bash
# shellcheck disable=SC2155

####
# .NET continuous integration actions.
#   Contains "action" functions which interact with the .NET CLI (dotnet) and its related utilities, e.g., NuGet.
#   Unless specified otherwise, all functions target the PROJECT_ROOT and assume a .NET solution exists there.
#
# Exported library functions:
#   * ci-dotnet-build - Execute 'dotnet build'.
#   * ci-dotnet-clean - Execute 'dotnet clean'.
#   * ci-dotnet-nuget-push - Execute 'dotnet nuget push'.
#   * ci-dotnet-pack - Execute 'dotnet pack'.
#   * ci-dotnet-publish - Execute 'dotnet publish'.
#   * ci-dotnet-restore - Execute 'dotnet restore'.
#   * ci-dotnet-test - Execute 'dotnet test'.
#
# Conditionally-required Environment:
#   $NUGET_API_KEY - NuGet source API key. Required for pushing NuGet packages.
#   $NUGET_SOURCE  - NuGet source, e.g., https://api.nuget.org/v3/index.json . Required for pushing NuGet packages.
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# .NET build
ci-dotnet-build() {
  dotnet build "${PROJECT_ROOT}" \
    -p:Version="${PROJECT_VERSION_DIST}" \
    "$@"
}

ci-dotnet-clean() {
  dotnet clean "${PROJECT_ROOT}" \
    "$@"
}

# .NET Project publish
ci-dotnet-publish() {
  dotnet publish "${PROJECT_ROOT}/src" \
    --configuration Release \
    --output "${BUILD_UNPACKAGED_DIST}" \
    -p:Version="${PROJECT_VERSION_DIST}" \
    "$@"
}

ci-dotnet-restore() {
  dotnet restore "${PROJECT_ROOT}" \
    "$@"
}

ci-dotnet-test() {
  dotnet test "${PROJECT_ROOT}" \
    "$@"
}

#--
# NuGet
#--

# .NET NuGet pack
ci-dotnet-pack() {
  dotnet pack "${PROJECT_ROOT}/src" \
    --configuration Release \
    --output "${BUILD_PACKAGED_DIST}/nuget/" \
    -p:PackageVersion="${PROJECT_VERSION_DIST}" \
    -p:Version="${PROJECT_VERSION_DIST}" \
    "$@"
}

# .NET NuGet push - requires environment $NUGET_SOURCE and NUGET_API_KEY
ci-dotnet-nuget-push() {
  # dotnet nuget push supports wildcard package names. See: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push
  printf "\n  Pushing NuGet packages to '%s' using 'dotnet nuget push'.\n" "${NUGET_SOURCE}" &&
    dotnet nuget push "${BUILD_PACKAGED_DIST}/nuget/*.nupkg" --api-key "${NUGET_API_KEY}" --source "${NUGET_SOURCE}" &&
    printf "\n  Pushed NuGet packages to '%s'.\n" "${NUGET_SOURCE}"
}

export -f ci-dotnet-build
export -f ci-dotnet-clean
export -f ci-dotnet-nuget-push
export -f ci-dotnet-pack
export -f ci-dotnet-publish
export -f ci-dotnet-restore
export -f ci-dotnet-test
