#!/usr/bin/env bash

###
# Pushes all NuGet packages in /build/dist to GitHub packages.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh).
###

# Fail or exit immediately if there is an error.
set -o errexit
# Fail if an unset variable is used.
set -o nounset
# Sets the exit code of a pipeline to that of the rightmost command to exit with a non-zero status,
# or zero if all commands of the pipeline exit successfully.
set -o pipefail

if [ -z "$GITHUB_OWNER" ]; then
  echo "Required 'GITHUB_OWNER' environment variable not set"
  exit 1
fi
if [ -z "$GITHUB_AUTH_TOKEN" ]; then
  echo "Required 'GITHUB_AUTH_TOKEN' environment variable not set"
  exit 1
fi

printf "Publishing %s version %s to GitHub packages '%s'\n\n\n" "${PROJECT_NAME}" "${PROJECT_VERSION_DIST}" "https://nuget.pkg.github.com/${GITHUB_OWNER}"

for packageFile in "${PROJECT_ROOT}"/build/dist/*.nupkg; do
  curl -vX PUT \
    -u "${GITHUB_OWNER}:${GITHUB_AUTH_TOKEN}" \
    -F package=@"${packageFile}" \
    "https://nuget.pkg.github.com/${GITHUB_OWNER}" \
  && printf "\n  Pushed '%s'" "${packageFile}"
done

printf "\nNuGet packages published to GitHub packages.\n"
