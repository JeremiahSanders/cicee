# Customizing Project CI Environment to Add Private NuGet Sources

This recipe allows private NuGet sources to be used during `dotnet restore` by conditionally generating `NuGet.config` when specific build environment variables are provided.

## Goal

The goal of this recipe is to ensure the `dotnet` CLI can restore packages using a private NuGet source when executed in the containerized CI environment.

## Possible Reasons / Use Cases

Common use cases are:

* Enable private NuGet source when executing `dotnet restore` in the containerized CI environment.

## Example

> ### Prerequisites
>
> Create a project CI environment script, saved to `${PROJECT_ROOT}/ci/env.project.sh`. This script should be committed to source control.
>
> Create a _local_ CI environment script, saved to `${PROJECT_ROOT}/ci/env.local.sh`. This script _should not_ be committed to source control. Adding it to `.gitignore` is recommended.
>
> Configure any applicable continuous integration environments (e.g., GitHub workflow definition, cloud-hosted continuous integration agent, TeamCity build configuration) with the following environment variables:
>
> * `NUGET_PASSWORD` - Authentication password.
> * `NUGET_SOURCE_NAME` - Name which should be assigned to the source if it is not configured.
> * `NUGET_SOURCE_PATH` - URL for the NuGet source, e.g., `https://api.nuget.org/v3/index.json`.
> * `NUGET_USER` - Authentication username.

### Project Environment `env.project.sh` Script

```bash
#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

#--
# Add an unconfigured NuGet source if environment contains:
#   NUGET_SOURCE_NAME
#   NUGET_SOURCE_PATH
#   NUGET_USER
#   NUGET_PASSWORD
# The function silently completes if any of the variables are undefined or empty.
#--
ci-add-nuget-source-if-provided() {
  if [[ -n "${NUGET_SOURCE_NAME:-}" && -n "${NUGET_SOURCE_PATH:-}" && -n "${NUGET_USER:-}" && -n "${NUGET_PASSWORD:-}" ]]; then
    # A NuGet source is fully specified and credentials were provided.
    local MATCHING_SOURCES=$(dotnet nuget list source --format "Short" | grep "${NUGET_SOURCE_PATH}")
    if [[ -z "${MATCHING_SOURCES}" ]]; then
      # The source not recognized.
      echo "NuGet source '${NUGET_SOURCE_PATH}' is not configured in 'dotnet nuget list source'."
      {
        echo "Attempting to create source '${NUGET_SOURCE_NAME}' with encrypted credentials."
        # Try to write the NuGet config with encrypted credentials.
        dotnet nuget add source "${NUGET_SOURCE_PATH}" \
          --name "${NUGET_SOURCE_NAME}" \
          --username "${NUGET_USER}" \
          --password "${NUGET_PASSWORD}" \
          "$@"
      } || {
        # Fallback to storing password in clear text. This is common on Linux and macOS.
        echo "Failed to create source with encrypted credentials. Attempting to create source '${NUGET_SOURCE_NAME}' with plaintext credentials."
        dotnet nuget add source "${NUGET_SOURCE_PATH}" \
          --name "${NUGET_SOURCE_NAME}" \
          --username "${NUGET_USER}" \
          --password "${NUGET_PASSWORD}" \
          --store-password-in-clear-text \
          "$@"
      }
    else
      echo "NuGet source '${NUGET_SOURCE_PATH}' is already configured."
    fi
  fi
}

export -f ci-add-nuget-source-if-provided

# Execute our project-specific initialization workflow.
ci-add-nuget-source-if-provided
```

### Project `docker-compose.project.yml`

```yml
version: "3.7"

##
# Project-specific CI environment extensions
##

services:
  # cicee execution service.
  ci-exec:
    environment:
      # Environment variables with only a key are resolved to their values on the machine running (Docker) Compose.
      #--
      # Project
      #--
      NUGET_PASSWORD:
      NUGET_SOURCE_NAME:
      NUGET_SOURCE_PATH:
      NUGET_USER:
    # NOTE: Root user specified below helps address permissions errors when using the default CICEE Dockerfile.
    user: root
```

### Local Environment `env.local.sh` Script

```bash
#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Uncomment the lines below and replace placeholder values to add a private source to NuGet.config during local builds.

# export NUGET_PASSWORD="<replace-with-password>"
# export NUGET_SOURCE_NAME="<replace-with-source-name>"
# export NUGET_SOURCE_PATH="https://replace-with-real-url"
# export NUGET_USER="<replace-with-source-user>"
#
# ci-add-nuget-source-if-provided
```
