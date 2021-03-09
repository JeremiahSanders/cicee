#!/usr/bin/env bash
# shellcheck disable=SC2155

###
# Project CI Action Library.
#   Contains "action" functions which perform steps in a continuous integration workflow.
#   By convention, each CI action function begins with "_ci_".
#     Design note: The prefix "_ci_" was chosen to reduce collisions with other tools and convey that these are related
#                  to, but more "private" than, the CI workflow functions, which are prefixed "ci-".
#
# How to use:
#   Create functions to perform each continuous integration "action" in your workflows. Several templates are provided.
#     Examples: compile source, compress build artifacts, build Docker image, compile a .NET NuGet package
#
# Workflow Utilities
#   These actions are intended to support other actions and be components of workflows.
#     * ci-EnvDisplay - Display initialized CI environment.
#     * ci-EnvInit - Initialize CI environment. *ALL OTHER ACTIONS ASSUME THIS ENVIRONMENT.*
#     * ci-EnvRequire - Validates that the CI environment is initialized.
# Action library functions:
#   Amazon Web Services CLI (aws) and CDK CLI (cdk) actions:
#     * _ci_CdkDeploy - Execute 'cdk deploy'.
#     * _ci_CdkSynth - Execute 'cdk synth'.
#     * _ci_DockerLoginAwsEcr - Authenticate with AWS ECR and use authorization to execute docker login.
#   Docker actions:
#     * _ci_DockerBuild - Execute 'docker build'.
#     * _ci_DockerPush - Execute 'docker push'.
#   GitHub actions:
#     * _ci_DotnetNuGetPushGitHub - Use curl to push NuGet packages to GitHub packages. Does not use 'dotnet nuget push' to avoid authorization errors.
#     * _ci_GenerateNuGetConfigGitHub - Generate NuGet.Config at a provided location (default: PROJECT_ROOT) with authenticated GitHub packages source.
#   .NET CLI (dotnet) actions:
#     * _ci_DotnetBuild - Execute 'dotnet build'.
#     * _ci_DotnetNuGetPush - Execute 'dotnet nuget push'.
#     * _ci_DotnetPack - Execute 'dotnet pack'.
#     * _ci_DotnetPublish - Execute 'dotnet publish'.
#     * _ci_DotnetRestore - Execute 'dotnet restore'.
#     * _ci_DotnetTest - Execute 'dotnet test'.
###

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

# Infer this script has been sourced based upon ACTIONS_SCRIPT_LOCATION being non-empty.
if [[ -n "${ACTIONS_SCRIPT_LOCATION:-}" ]]; then
  # Actions are already sourced. Exit.
  # Check to see if this script was sourced.
  #   See: https://stackoverflow.com/a/28776166/402726
  (return 0 2>/dev/null) && sourced=1 || sourced=0
  if [[ $sourced -eq 1 ]]; then
    # NOTE: return is used, rather than exit, to prevent shell exit when sourcing from an interactive shell.
    return 0
  else
    exit 0
  fi
fi

# Context
ACTIONS_SCRIPT_LOCATION="$(dirname "${BASH_SOURCE[0]}")"
PROJECT_ROOT="${PROJECT_ROOT:-$(cd "${ACTIONS_SCRIPT_LOCATION}" && cd ../.. && pwd)}"

###############################################################################
# BEGIN utility functions.
###

# Require that one or more environment variables are set. Required variables are provided as string arguments. E.g. "PROJECT_ROOT", "NPM_API_KEY".
require-var() {
  for variableName in "$@"; do
    # The ! below performs 'indirect expansion'. See: https://www.gnu.org/software/bash/manual/html_node/Shell-Parameter-Expansion.html
    if [[ -z "${!variableName:-}" ]]; then
      printf "Required variable not set: %s\n\n" "${variableName}" &&
        return 1
    fi
  done
}

###
# END utility functions.
###############################################################################

###############################################################################
# BEGIN entrypoint support CI workflow actions.
###

#------------------------------------------------------------------------------
# CI Environment Initialization.
#   All CI actions implicitly require this function to be executed. Its purpose is to provide a consistent execution
#   environment for project-level activities. For example, by using a consistent BUILD_PACKAGED_DIST variable,
#   CI actions creating an output zip file, NPM package, or APK can all place their artifacts in consistent locations.
#   Design note: This function's name follows the CI workflow pattern because it is expected to be executed in a
#                workflow entrypoint script.
#
# Expected environment available to all CI actions, after ci-EnvInit is executed:
#   BUILD_DOCS="${BUILD_ROOT}/docs" - Project distributable documentation which would accompany packaged build output.
#   BUILD_PACKAGED_DIST="${BUILD_ROOT}/dist" - Project packaged build output. E.g., .NET NuGet packages, zip archives, AWS CDK cloud assemblies.
#   BUILD_ROOT - Project build output root directory.
#   BUILD_UNPACKAGED_DIST="${BUILD_ROOT}/app" - Project unpackaged build output. E.g., processed output which might be further packaged or compressed before distribution. E.g., .NET DLLs, Java class files.
#
#   PROJECT_NAME - Project name. By convention this should be in lower kebab case. I.e., multi-word-project-name. This will be used to pattern conventional output paths, e.g., as part of a zip archive file name.
#   PROJECT_ROOT - Project root directory.
#   PROJECT_TITLE - Project human-readable name. Defaults to PROJECT_NAME.
#   PROJECT_VERSION - Project distributable Major.Minor.Patch version. I.e., 2.3.1.
#   PROJECT_VERSION_DIST - Project distributable version. Expected to be in the following format: Release versions: Major.Minor.Patch, e.g., 4.1.7. Pre-release versions: Major.Minor.Patch-sha-GitSha, e.g., 4.1.7-sha-a7328f. These formats are very important. They help ensure compatibility across .NET projects, .NET NuGet packages, and Docker tags.
#
# Workflow:
#   1 - Initialize convention-based CI enviroment variables.
#   2 - Attempt to load project metadata by convention.
#       Populates PROJECT_NAME, PROJECT_TITLE, and PROJECT_VERSION from JSON file. From "name", "title", and "version" properties, respectively.
#       Default location: PROJECT_ROOT/project-metadata.json
#       Additional paths: PROJECT_ROOT/.project-metadata.json, PROJECT_ROOT/ci/project-metadata.json, PROJECT_ROOT/ci/.project-metadata.json, PROJECT_ROOT/package.json
#   3 - Apply fallback values for expected metadata.
#       PROJECT_VERSION_DIST defaults to PROJECT_VERSION if RELEASE_ENVIRONMENT=true in the environment, or PROJECT_VERSION-sha-CURRENT_GIT_HASH if not.
#   4 - Source local environment file, if available; enabling setting local defaults and overrides to convention.
#--
ci-EnvInit() {
  # Tries to source a project environment file.
  __loadProjectEnvironment() {
    # Infer the project environment has been sourced based upon PROJECT_ENV_FILE being non-empty.
    if [[ -n "${PROJECT_ENV_FILE:-}" ]]; then
      # Environment was already sourced.
      printf "CI environment currently sourced: %s\n" "${PROJECT_ENV_FILE}" &&
        return 0
    fi
    # Helper function which attempts to source a file.
    __trySource() {
      if [[ -f "$1" ]]; then
        source "$1" &&
          printf "\nSourced '%s'.\n\n" "$1" &&
          return 0
      else
        return 1
      fi
    }
    # Helper function which attempts to source a file and, if successful, export that file path as PROJECT_ENV_FILE.
    __trySourceProjectEnv() {
      __trySource "$1" && PROJECT_ENV_FILE="$1"
    }

    # Try to source several common environment file locations, preferring those in the ci directory.
    # Final return 0 ensures we return success, even if no environment files were loaded.
    __trySourceProjectEnv "${PROJECT_ROOT}/ci/ci.env" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci/.env" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci/env.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci/project.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci.env" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/.env" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/env.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/project.sh" ||
      return 0
  }
  # Initializes environment variables which are derived from other values.
  __initializeDerivedEnvironment() {
    # Build output paths -- supports consistent output expectations.
    BUILD_ROOT="${PROJECT_ROOT}/build"
    BUILD_UNPACKAGED_DIST="${BUILD_ROOT}/app"
    BUILD_PACKAGED_DIST="${BUILD_ROOT}/dist"
    BUILD_DOCS="${BUILD_ROOT}/docs"

    if [[ -n "$(which git)" ]]; then
      # We have git installed
      CURRENT_GIT_BRANCH="$(git branch | sed -n '/\* /s///p')"
      CURRENT_GIT_HASH="$(git log --pretty=format:'%h' -n 1)"
    else
      CURRENT_GIT_BRANCH="git-is-not-installed"
      CURRENT_GIT_HASH="0000000"
    fi
  }

  __tryLoadProjectMetadata() {
    if [[ -z "$(which jq)" ]]; then
      printf "jq not installed. Cannot load JSON project metadata.\nApplying fallback defaults.\n" &&
        return 0
    fi

    __loadMetadataFromFile() {
      declare PROJECT_METADATA="$1"
      if [[ -f "$PROJECT_METADATA" ]]; then
        PROJECT_NAME="$(jq --raw-output 'if .name== null then "" else .name end' "${PROJECT_METADATA}")"
        PROJECT_TITLE="$(jq --raw-output 'if .title== null then "" else .title end' "${PROJECT_METADATA}")"
        PROJECT_VERSION="$(jq --raw-output 'if .version== null then "" else .version end' "${PROJECT_METADATA}")"
        printf "Loaded project metadata from %s\n" "${PROJECT_METADATA}"
        return 0
      else
        return 1
      fi
    }

    __loadMetadataFromFile "${PROJECT_ROOT}/project-metadata.json" ||
      __loadMetadataFromFile "${PROJECT_ROOT}/.project-metadata.json" ||
      __loadMetadataFromFile "${PROJECT_ROOT}/ci/project-metadata.json" ||
      __loadMetadataFromFile "${PROJECT_ROOT}/ci/.project-metadata.json" ||
      __loadMetadataFromFile "${PROJECT_ROOT}/package.json" ||
      __loadMetadataFromFile "${PROJECT_ROOT}/src/package.json"
  }

  __applyFallbackProjectEnvironmentValues() {
    PROJECT_NAME="${PROJECT_NAME:-unknown-project}"
    PROJECT_TITLE="${PROJECT_TITLE:-${PROJECT_NAME:-Unknown Project}}"
    PROJECT_VERSION="${PROJECT_VERSION:-0.0.0}"
    if [[ "${RELEASE_ENVIRONMENT:-false}" = true ]]; then
      PROJECT_VERSION_DIST="${PROJECT_VERSION_DIST:-${PROJECT_VERSION}}"
    else
      PROJECT_VERSION_DIST="${PROJECT_VERSION_DIST:-${PROJECT_VERSION}-sha-${CURRENT_GIT_HASH}}"
    fi
  }

  # ... now that the workflow functions are established, execute the initialization workflow...
  # 1 - Initialize convention-based CI enviroment variables.
  # 2 - Attempt to load project metadata by convention.
  # 3 - Apply fallback values for expected metadata.
  # 4 - Source local environment file, if available; enabling setting local defaults and overrides to convention.
  printf "Initializing CI environment...\n" &&
    __initializeDerivedEnvironment &&
    __tryLoadProjectMetadata &&
    __applyFallbackProjectEnvironmentValues &&
    __loadProjectEnvironment &&
    printf "CI environment initialized.\n"
}

#------------------------------------------------------------------------------
# CI Environment Display.
#   Displays the current CI environment metadata and build output structure.
#--
ci-EnvDisplay() {
  printf "\n-- CI Environment -\n"
  printf "Project            :  %s\n" "${PROJECT_NAME}"
  printf "  Title            :  %s\n" "${PROJECT_TITLE}"
  printf "  Version          :  %s\n" "${PROJECT_VERSION}"
  printf "    Distribution   :  %s\n" "${PROJECT_VERSION_DIST}"
  printf "  Root             :  %s\n\n" "${PROJECT_ROOT}"

  printf "  Build output     :  %s\n" "${BUILD_ROOT}"
  printf "    Documentation  :  %s\n" "${BUILD_DOCS}"
  printf "    Packaged       :  %s\n" "${BUILD_PACKAGED_DIST}"
  printf "    Unpackaged     :  %s\n\n" "${BUILD_UNPACKAGED_DIST}"
}

#------------------------------------------------------------------------------
# CI Environment Require.
#   Validates that the CI environment is initialized.
#--
ci-EnvRequire() {
  require-var \
    "BUILD_DOCS" \
    "BUILD_PACKAGED_DIST" \
    "BUILD_ROOT" \
    "BUILD_UNPACKAGED_DIST" \
    "PROJECT_NAME" \
    "PROJECT_ROOT" \
    "PROJECT_TITLE" \
    "PROJECT_VERSION_DIST" \
    "PROJECT_VERSION"
}

export -f ci-EnvDisplay
export -f ci-EnvInit
export -f ci-EnvRequire

###
# END entrypoint support CI workflow actions.
###############################################################################

###############################################################################
# BEGIN AWS CI workflow actions.
#
# Environment:
#   $AWS_ACCOUNT   - AWS account ID/number.
#   $AWS_REGION    - AWS region.
#   $PROJECT_ROOT  - Project root directory.
###

#-
# AWS CLI (aws)
#-

# Docker login to AWS ECR - requires environment $AWS_ACCOUNT and $AWS_REGION.
_ci_DockerLoginAwsEcr() {
  require-var "AWS_REGION" "AWS_ACCOUNT"
  aws ecr get-login-password --region "${AWS_REGION}" |
    docker login --username AWS --password-stdin "${AWS_ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com"
}

#-
# AWS Cloud Development Kit CLI (cdk)
# -

# AWS CDK deploy
_ci_CdkDeploy() {
  cdk deploy --app "${BUILD_PACKAGED_DIST}/cloud-assembly" "$@"
}

# AWS CDK synth
_ci_CdkSynth() {
  cd "${PROJECT_ROOT}" &&
    cdk synth --output "${BUILD_PACKAGED_DIST}/cloud-assembly" "$@" &&
    cd -
}

export -f _ci_CdkDeploy
export -f _ci_CdkSynth
export -f _ci_DockerLoginAwsEcr

###
# END AWS CI workflow actions.
###############################################################################

###############################################################################
# BEGIN Docker CI workflow actions.
#
# Environment:
#   $DOCKER_IMAGE  - Docker image tag.
#   $PROJECT_ROOT  - Project root directory.
###

# Docker build - requires environment $DOCKER_IMAGE, which should be formatted as a Docker tag.
_ci_DockerBuild() {
  require-var "DOCKER_IMAGE" &&
    docker build \
      --rm \
      --tag "${DOCKER_IMAGE}" \
      -f "${PROJECT_ROOT}/Dockerfile" \
      "${PROJECT_ROOT}"
}

# Docker push
_ci_DockerPush() {
  require-var "DOCKER_IMAGE" &&
    docker push "${DOCKER_IMAGE}"
}

export -f _ci_DockerBuild
export -f _ci_DockerPush

###
# END Docker CI workflow actions.
###############################################################################

###############################################################################
# BEGIN GitHub CI workflow actions.
#
# Environment:
#   $GITHUB_AUTH_TOKEN - GitHub API authorization token.
#   $GITHUB_OWNER      - GitHub repository owner, i.e., username.
#   $PROJECT_ROOT      - Project root directory.
###

# Generate a NuGet.Config which uses GitHub packages as a source.
#   GITHUB_AUTH_TOKEN will be stored in plain text due to lack of encrypted credential support in 'dotnet nuget' on macOS and Linux.
_ci_GenerateNuGetConfigGitHub() {
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

_ci_DotnetNuGetPushGitHub() {
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

export -f _ci_DotnetNuGetPushGitHub
export -f _ci_GenerateNuGetConfigGitHub

###
# END GitHub CI workflow actions.
###############################################################################

###############################################################################
# BEGIN .NET CI workflow actions.
#
# Environment:
#   $NUGET_API_KEY - NuGet source API key.
#   $NUGET_SOURCE  - NuGet source, e.g., https://api.nuget.org/v3/index.json .
#   $PROJECT_ROOT  - Project root directory.
###

# .NET build
_ci_DotnetBuild() {
  dotnet build "${PROJECT_ROOT}" \
    -p:GenerateDocumentationFile=true \
    -p:Version="${PROJECT_VERSION_DIST}" \
    "$@"
}

# .NET Project publish
_ci_DotnetPublish() {
  dotnet publish "${PROJECT_ROOT}/src" \
    --configuration Release \
    --output "${BUILD_UNPACKAGED_DIST}" \
    -p:DocumentationFile="${PROJECT_ROOT}/build/docs/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.xml" \
    -p:Version="${PROJECT_VERSION_DIST}" \
    "$@"
}

_ci_DotnetRestore() {
  dotnet restore "${PROJECT_ROOT}" \
    "$@"
}

_ci_DotnetTest() {
  dotnet test "${PROJECT_ROOT}" \
    "$@"
}

#--
# NuGet
#--

# .NET NuGet pack
_ci_DotnetPack() {
  dotnet pack "${PROJECT_ROOT}/src" \
    --configuration Release \
    --output "${BUILD_PACKAGED_DIST}/nuget/" \
    -p:DocumentationFile="${BUILD_DOCS}/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.xml" \
    -p:PackageVersion="${PROJECT_VERSION_DIST}" \
    -p:Version="${PROJECT_VERSION_DIST}" \
    "$@"
}

# .NET NuGet push - requires environment $NUGET_SOURCE and NUGET_API_KEY
_ci_DotnetNuGetPush() {
  for packageFile in "${BUILD_PACKAGED_DIST}"/nuget/*.nupkg; do
    dotnet nuget push "${packageFile}" --api-key "${NUGET_API_KEY}" --source "${NUGET_SOURCE}" &&
      printf "\n  Pushed '%s'" "${packageFile}"
  done
}

export -f _ci_DotnetBuild
export -f _ci_DotnetNuGetPush
export -f _ci_DotnetPack
export -f _ci_DotnetPublish
export -f _ci_DotnetRestore
export -f _ci_DotnetTest

###
# END .NET CI workflow actions.
###############################################################################
