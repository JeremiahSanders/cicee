#!/usr/bin/env bash

###
# Generate a NuGet.Config targeting GitHub repositories. File generated at the directory specified by argument, or at the project root.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

# Context

printf "Generating NuGet configuration for %s version %s...\n\n\n" "${PROJECT_NAME}" "${PROJECT_VERSION_DIST}"

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

printf "%s" "$NUGET_CONFIG_CONTENT" >"$NUGET_PROJECT_CONFIG_FILE"

printf "\nNuGet.Config written to: %s\n" "$NUGET_PROJECT_CONFIG_FILE"
