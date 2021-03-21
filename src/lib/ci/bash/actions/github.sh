#!/usr/bin/env bash
# shellcheck disable=SC2155

####
# GitHub CI Action Library.
#   Contains functions supporting the use of GitHub.
#
# Exported library functions:
#   * ci-github-nuget-push            - Use curl to push NuGet packages to GitHub packages. Does not use 'dotnet nuget push' to avoid authorization errors.
#   * ci-github-nuget-config-generate - Generate NuGet.Config at a provided location (default: PROJECT_ROOT) with authenticated GitHub packages source.
#
# Required Environment:
#   $GITHUB_AUTH_TOKEN - GitHub API authorization token.
#   $GITHUB_OWNER      - GitHub repository owner, i.e., username.
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

##
# Generate a NuGet.Config which uses GitHub packages as a source.
#   GITHUB_AUTH_TOKEN will be stored in plain text due to lack of encrypted credential support in 'dotnet nuget' on macOS and Linux.
##
ci-github-nuget-config-generate() {
  require-var "GITHUB_OWNER" "GITHUB_AUTH_TOKEN"
  printf "Generating NuGet configuration...\n\n\n"

  declare -r GITHUB_REPO_URL="https://nuget.pkg.github.com/${GITHUB_OWNER}/index.json"
  declare -r NUGET_CONFIG_CONTENT="<?xml version=\"1.0\" encoding=\"utf-8\"?>
<configuration>
      <packageSources>
           <clear />
           <add key=\"github\" value=\"${GITHUB_REPO_URL}\" />
           <add key=\"nuget\" value=\"https://api.nuget.org/v3/index.json\" protocolVersion=\"3\" />
       </packageSources>
       <packageSourceCredentials>
           <github>
               <add key=\"Username\" value=\"${GITHUB_OWNER}\" />
               <add key=\"ClearTextPassword\" value=\"${GITHUB_AUTH_TOKEN}\" />
           </github>
       </packageSourceCredentials>
</configuration>"

  if [[ -d "$1" ]]; then
    declare -r NUGET_PROJECT_CONFIG_FILE="$1/NuGet.Config"
  else
    declare -r NUGET_PROJECT_CONFIG_FILE="${PROJECT_ROOT}/NuGet.Config"
  fi

  printf "%s" "$NUGET_CONFIG_CONTENT" >"$NUGET_PROJECT_CONFIG_FILE" &&
    printf "\nNuGet.Config written to: %s\n" "$NUGET_PROJECT_CONFIG_FILE"
}

##
# Use curl to push NuGet packages to GitHub packages. Does not use 'dotnet nuget push' to avoid authorization errors.
##
ci-github-nuget-push() {
  require-var "GITHUB_OWNER" "GITHUB_AUTH_TOKEN"
  printf "Publishing %s version %s to GitHub packages '%s'\n\n\n" "${PROJECT_NAME}" "${PROJECT_VERSION_DIST}" "https://nuget.pkg.github.com/${GITHUB_OWNER}"

  for packageFile in "${BUILD_PACKAGED_DIST}"/nuget/*.nupkg; do
    curl -vX PUT \
      -u "${GITHUB_OWNER}:${GITHUB_AUTH_TOKEN}" \
      -F package=@"${packageFile}" \
      "https://nuget.pkg.github.com/${GITHUB_OWNER}" &&
      printf "\n  Pushed '%s'" "${packageFile}"
  done

  printf "\nNuGet packages published to GitHub packages.\n"
}

export -f ci-github-nuget-push
export -f ci-github-nuget-config-generate
