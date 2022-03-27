#!/usr/bin/env bash
# shellcheck disable=SC1090 # Can't follow non-constant source. Use a directive to specify location.

####
# Continuous integration environment action library.
#   These actions are intended to support other actions and be components of workflows.
#
# Exported library functions:
#   * ci-env-display - Display initialized CI environment.
#   * ci-env-init    - Initialize CI environment. *ALL OTHER ACTIONS ASSUME THIS ENVIRONMENT.*
#   * ci-env-require - Validates that the CI environment is initialized.
#   * ci-env-reset   - Unsets predenfined environment variables set by ci-env-init. Does not reset project or local variables.
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

##
# CI Environment Initialization.
#   All CI actions implicitly require this function to be executed. Its purpose is to provide a consistent execution
#   environment for project-level activities. For example, by using a consistent BUILD_PACKAGED_DIST variable,
#   CI actions creating an output zip file, NPM package, or APK can all place their artifacts in consistent locations.
#   Design note: This function's name follows the CI workflow pattern because it is expected to be executed in a
#                workflow entrypoint script.
#
# Expected environment available to all CI actions, after ci-env-init is executed:
#  - Contextual -
#   PROJECT_NAME - Project name. By convention this should be in lower kebab case. I.e., multi-word-project-name. This will be used to pattern conventional output paths, e.g., as part of a zip archive file name.
#   PROJECT_ROOT - Project root directory.
#   PROJECT_TITLE - Project human-readable name. Defaults to PROJECT_NAME.
#   PROJECT_VERSION - Project distributable Major.Minor.Patch version. I.e., 2.3.1.
#   PROJECT_VERSION_DIST - Project distributable version. Expected to be in the following format: Release versions: Major.Minor.Patch, e.g., 4.1.7. Pre-release versions: Major.Minor.Patch-YearMonthDay-HourMinuteSecond-sha-GitSha, e.g., 4.1.7-20220313-131121-sha-a7328f. These formats are very important. They help ensure compatibility across .NET projects, .NET NuGet packages, and Docker tags.
#   CURRENT_GIT_BRANCH - Current Git branch.
#   CURRENT_GIT_HASH - Current Git hash.
#  - Configuration -
#     The CIENV_VARIABLES* variables below are all derived from loading .ciEnvironment.variables JSON path from a project metadata file (if one exists) using jq.
#   CIENV_VARIABLES - Array of project-specific CI environment variable names.
#   CIENV_VARIABLES_PUBLIC - Array of project-specific CI environment variable names which are NOT marked secret (by their .secret JSON path property).
#   CIENV_VARIABLES_REQUIRED - Array of project-specific CI environment variable names which are marked required (by their .required JSON path property).
#   CIENV_VARIABLES_SECRET - Array of project-specific CI environment variable names which ARE marked secret (by their .secret JSON path property).
#  - Conventional Output -
#   BUILD_DOCS="${BUILD_ROOT}/docs" - Project distributable documentation which would accompany packaged build output.
#   BUILD_PACKAGED_DIST="${BUILD_ROOT}/dist" - Project packaged build output. E.g., .NET NuGet packages, zip archives, AWS CDK cloud assemblies.
#   BUILD_ROOT - Project build output root directory.
#   BUILD_UNPACKAGED_DIST="${BUILD_ROOT}/app" - Project unpackaged build output. E.g., processed output which might be further packaged or compressed before distribution. E.g., .NET DLLs, Java class files.
#
# Workflow:
#   1 - Initialize convention-based CI enviroment variables.
#   2 - Attempt to load project metadata by convention.
#       Populates PROJECT_NAME, PROJECT_TITLE, and PROJECT_VERSION from JSON file. From "name", "title", and "version" properties, respectively.
#       Default location: PROJECT_ROOT/project-metadata.json
#       Additional paths: PROJECT_ROOT/.project-metadata.json, PROJECT_ROOT/ci/project-metadata.json, PROJECT_ROOT/ci/.project-metadata.json, PROJECT_ROOT/package.json
#   3 - Apply fallback values for expected metadata.
#       PROJECT_VERSION_DIST defaults to PROJECT_VERSION if RELEASE_ENVIRONMENT=true in the environment, or PROJECT_VERSION-sha-CURRENT_GIT_HASH if not.
#   4 - Source project environment file, if available; enabling setting project defaults and overrides to convention.
#       This file is assumed to be stored in version control and all environments (e.g., both local and build server) are assumed to have the same file.
#       Default location: PROJECT_ROOT/ci/env.project.sh
#       Additional paths: PROJECT_ROOT/ci/env.ci.sh, PROJECT_ROOT/ci/ci.env, PROJECT_ROOT/ci/project.sh, PROJECT_ROOT/env.project.sh, PROJECT_ROOT/ci.env, PROJECT_ROOT/project.sh
#   5 - Source local environment file, if available; enabling setting local defaults and overrides to convention.
#       This file is assumed to NOT BE stored in version control. No consistency between environments is assumed, though patterns and templates are recommended.
#       Default location: PROJECT_ROOT/ci/env.local.sh
#       Additional paths: PROJECT_ROOT/ci/env.sh, PROJECT_ROOT/ci/.env, PROJECT_ROOT/env.local.sh, PROJECT_ROOT/env.sh, PROJECT_ROOT/.env
##
function ci-env-init() {
  local -r initialPwd="$(pwd)"

  # Helper function which attempts to source a file.
  function __trySource() {
    if [[ -f "${1}" ]]; then
      source "${1}" &&
        printf "\nSourced '%s'.\n\n" "${1}" &&
        return 0
    else
      return 1
    fi
  }

  #----
  # Tries to source a project environment file.
  #----
  function __loadProjectEnvironment() {
    # Infer the project environment has been sourced based upon PROJECT_ENV_FILE being non-empty.
    if [[ -n "${PROJECT_ENV_FILE:-}" ]]; then
      # Environment was already sourced.
      printf "CI environment currently sourced: %s\n" "${PROJECT_ENV_FILE}" &&
        return 0
    fi
    # Helper function which attempts to source a file and, if successful, export that file path as PROJECT_ENV_FILE.
    __trySourceProjectEnv() {
      __trySource "${1}" &&
        PROJECT_ENV_FILE="${1}" &&
        export PROJECT_ENV_FILE
    }

    # Try to source several common project environment file locations, preferring those in the ci directory.
    # Final return 0 ensures we return success, even if no environment files were loaded.
    __trySourceProjectEnv "${PROJECT_ROOT}/ci/env.project.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci/env.ci.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci/ci.env" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci/project.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/env.project.sh" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/ci.env" ||
      __trySourceProjectEnv "${PROJECT_ROOT}/project.sh" ||
      printf "No project environment initialization script found.\n  Recommended location: %s\n" "${PROJECT_ROOT}/ci/env.project.sh"
  }

  #----
  # Tries to source a local environment file.
  #----
  function __loadLocalEnvironment() {
    # Infer the project environment has been sourced based upon LOCAL_ENV_FILE being non-empty.
    if [[ -n "${LOCAL_ENV_FILE:-}" ]]; then
      # Environment was already sourced.
      printf "CI environment currently sourced: %s\n" "${LOCAL_ENV_FILE}" &&
        return 0
    fi
    # Helper function which attempts to source a file and, if successful, export that file path as LOCAL_ENV_FILE.
    __trySourceLocalEnv() {
      __trySource "$1" &&
        LOCAL_ENV_FILE="$1" &&
        export LOCAL_ENV_FILE
    }

    # Try to source several common local environment file locations, preferring those in the ci directory.
    # Final return 0 ensures we return success, even if no environment files were loaded.
    __trySourceLocalEnv "${PROJECT_ROOT}/ci/env.local.sh" ||
      __trySourceLocalEnv "${PROJECT_ROOT}/ci/env.sh" ||
      __trySourceLocalEnv "${PROJECT_ROOT}/ci/.env" ||
      __trySourceLocalEnv "${PROJECT_ROOT}/env.local.sh" ||
      __trySourceLocalEnv "${PROJECT_ROOT}/env.sh" ||
      __trySourceLocalEnv "${PROJECT_ROOT}/.env" ||
      printf "No local environment initialization script found.\n  Recommended location: %s\n" "${PROJECT_ROOT}/ci/env.local.sh"
  }

  #----
  # Initializes environment variables which are derived from other values.
  #----
  function __initializeDerivedEnvironment() {
    if [[ -z "${PROJECT_ROOT:-}" ]]; then
      printf "PROJECT_ROOT is not set. Defaulting to %s.\n" "${initialPwd}"
    fi
    PROJECT_ROOT="${PROJECT_ROOT:-${initialPwd}}"

    # Build output paths -- supports consistent output expectations.
    BUILD_ROOT="${PROJECT_ROOT}/build"
    BUILD_UNPACKAGED_DIST="${BUILD_ROOT}/app"
    BUILD_PACKAGED_DIST="${BUILD_ROOT}/dist"
    BUILD_DOCS="${BUILD_ROOT}/docs"

    if [[ -n "$(which git)" && -d "${PROJECT_ROOT}/.git" ]]; then
      # We have git installed
      CURRENT_GIT_BRANCH="$(git branch | sed -n '/\* /s///p')"
      CURRENT_GIT_HASH="$(git log --pretty=format:'%h' -n 1)"
    else
      CURRENT_GIT_BRANCH="git-is-not-installed"
      CURRENT_GIT_HASH="0000000"
    fi

    export BUILD_DOCS
    export BUILD_PACKAGED_DIST
    export BUILD_ROOT
    export BUILD_UNPACKAGED_DIST
    export CURRENT_GIT_BRANCH
    export CURRENT_GIT_HASH
    export PROJECT_ROOT
  }

  #----
  # Try to load project metadata file.
  #----
  function __tryLoadProjectMetadata() {
    if [[ -z "$(which jq)" ]]; then
      printf "jq not installed. Cannot load JSON project metadata.\n" &&
        return 0
    fi

    function __loadMetadataFromFile() {
      declare PROJECT_METADATA="$1"
      if [[ -f "$PROJECT_METADATA" ]]; then
        PROJECT_NAME="$(jq --raw-output 'if .name == null then "" else .name end' "${PROJECT_METADATA}")"
        PROJECT_TITLE="$(jq --raw-output 'if .title == null then "" else .title end' "${PROJECT_METADATA}")"
        PROJECT_VERSION="$(jq --raw-output 'if .version == null then "" else .version end' "${PROJECT_METADATA}")"
        # Declare arrays of defined CI environment variables.
        #   NOTE: The pattern below declares temporary, string variables for the `jq` result.
        #   The `jq` queries create JSON arrays from the .name properties of .ciEnvironment.variables, which are then piped to jq's `@sh` formatter.
        #   The `@sh` formatter flattens and space-separates the array elements. However, it wraps each environment variable name in single-quotes (').
        #   If those values are directly interpolated into a Bash array (done by surrounding the space-separated values within parentheses), then
        #   the values cannot be indirectly expanded. Doing so will result in "bad substitution" errors in Windows using Git's Bash, or "unbound variable"
        #   errors in some Linux environments. That impacts ci-env-display.
        #   To work around this, we capture the entire result as a single string and then, when we declare the Bash array, we use Bash substring
        #   replacement (see https://tldp.org/LDP/abs/html/string-manipulation.html) to remove the single-quotes.
        local __elements_CIENV_VARIABLES="$(jq -r '[ .ciEnvironment.variables | .[]? | .name ] | @sh' "${PROJECT_METADATA}")"
        local __elements_CIENV_VARIABLES_REQUIRED="$(jq -r '[ .ciEnvironment.variables | .[]? | select( .required ) | .name ] | @sh' "${PROJECT_METADATA}")"
        local __elements_CIENV_VARIABLES_SECRET="$(jq -r '[ .ciEnvironment.variables | .[]? | select( .secret ) | .name ] | @sh' "${PROJECT_METADATA}")"
        local __elements_CIENV_VARIABLES_PUBLIC="$(jq -r '[ .ciEnvironment.variables | .[]? | select( .secret==false ) | .name ] | @sh' "${PROJECT_METADATA}")"
        CIENV_VARIABLES=(${__elements_CIENV_VARIABLES//\'/})
        CIENV_VARIABLES_REQUIRED=(${__elements_CIENV_VARIABLES_REQUIRED//\'/})
        CIENV_VARIABLES_SECRET=(${__elements_CIENV_VARIABLES_SECRET//\'/})
        CIENV_VARIABLES_PUBLIC=(${__elements_CIENV_VARIABLES_PUBLIC//\'/})

        printf "Loaded project metadata from %s\n" "${PROJECT_METADATA}"

        export CIENV_VARIABLES
        export CIENV_VARIABLES_PUBLIC
        export CIENV_VARIABLES_REQUIRED
        export CIENV_VARIABLES_SECRET
        export PROJECT_NAME
        export PROJECT_TITLE
        export PROJECT_VERSION

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
      __loadMetadataFromFile "${PROJECT_ROOT}/src/package.json" ||
      return 0
  }

  #----
  # Apply defaults for project environment variables which were not configured.
  #----
  function __applyFallbackProjectEnvironmentValues() {
    PROJECT_NAME="${PROJECT_NAME:-unknown-project}"
    PROJECT_TITLE="${PROJECT_TITLE:-${PROJECT_NAME:-Unknown Project}}"
    PROJECT_VERSION="${PROJECT_VERSION:-0.0.0}"
    if [[ "${RELEASE_ENVIRONMENT:-false}" = true ]]; then
      PROJECT_VERSION_DIST="${PROJECT_VERSION_DIST:-${PROJECT_VERSION}}"
    else
      local BUILD_DATE_TIME="$(TZ="utc" date "+%Y%m%d-%H%M%S")"
      if [[ "${PROJECT_VERSION}" =~ ^[0-9]*\.[0-9]*\.[0-9]*$ ]]; then
        # The version is in Major.Minor.Patch format.
        # Calculate next release version. Assume next version is a minor (so we'll use 0 instead of current patch version).
        IFS='.' read -ra PROJECT_VERSION_SEGMENTS <<<"${PROJECT_VERSION}"
        local MAJOR="${PROJECT_VERSION_SEGMENTS[0]}"
        local MINOR="${PROJECT_VERSION_SEGMENTS[1]}"
        local PATCH="0"
        # The $(()) converts ${MINOR} to a number.
        local BUMPED_MINOR=$((${MINOR} + 1))
        PROJECT_VERSION_DIST="${PROJECT_VERSION_DIST:-${MAJOR}.${BUMPED_MINOR}.${PATCH}-${BUILD_DATE_TIME}-sha-${CURRENT_GIT_HASH}}"
      else
        # The version is not in Major.Minor.Patch format.
        PROJECT_VERSION_DIST="${PROJECT_VERSION_DIST:-${PROJECT_VERSION}-${BUILD_DATE_TIME}-sha-${CURRENT_GIT_HASH}}"
      fi
    fi

    export PROJECT_NAME
    export PROJECT_TITLE
    export PROJECT_VERSION
    export PROJECT_VERSION_DIST
  }

  # ... now that the workflow functions are established, execute the initialization workflow...
  # 1 - Initialize convention-based CI enviroment variables.
  # 2 - Attempt to load project metadata by convention.
  # 3 - Apply fallback values for expected metadata.
  # 4 - Source project environment file, if available; enabling setting project defaults and overrides to convention.
  # 5 - Source local environment file, if available; enabling setting local defaults and overrides to convention.
  printf "Initializing CI environment...\n" &&
    __initializeDerivedEnvironment &&
    __tryLoadProjectMetadata &&
    __applyFallbackProjectEnvironmentValues &&
    __loadProjectEnvironment &&
    __loadLocalEnvironment &&
    printf "CI environment initialized.\n"
}

##
# CI Environment Display.
#   Displays the current CI environment metadata and build output structure.
##
function ci-env-display() {
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

  # Check to see if we have CI environment variables defined.
  if [[ -n "${CIENV_VARIABLES:-}" ]]; then
    printf "  Environment\n"
    # Loop through the CI environment variables.
    for variableName in "${CIENV_VARIABLES[@]}"; do
      # If the variable is defined as "secret"...
      if [[ "${CIENV_VARIABLES_SECRET[*]}" =~ ${variableName} ]]; then
        # ... and if the variable has a value...
        if [[ -n "${!variableName:-}" ]]; then
          # ...record the value masked.
          local value="********"
        else
          # Otherwise, record it empty.
          local value=""
        fi
      else
        # Since the variable is NOT "secret", we display the value.
        # The ! below performs 'indirect expansion'. See: https://www.gnu.org/software/bash/manual/html_node/Shell-Parameter-Expansion.html
        local value="${!variableName:-}"
      fi
      printf "    %s: %s\n" "${variableName}" "${value:-}"
    done
  fi
}

##
# CI Environment Require.
#   Validates that the CI environment is initialized and all required variables set.
##
function ci-env-require() {
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

  # If the project contains metadata defining CI environment variables, ensure that all required variables are set.
  if [[ -n "${CIENV_VARIABLES_REQUIRED:-}" ]]; then
    require-var "${CIENV_VARIABLES_REQUIRED[@]}"
  fi
}

##
# CI Environment Reset.
#   Removes standard environment initialization variables.
##
function ci-env-reset() {
  unset BUILD_DOCS
  unset BUILD_PACKAGED_DIST
  unset BUILD_ROOT
  unset BUILD_UNPACKAGED_DIST
  unset CIENV_VARIABLES
  unset CIENV_VARIABLES_PUBLIC
  unset CIENV_VARIABLES_REQUIRED
  unset CIENV_VARIABLES_SECRET
  unset CURRENT_GIT_BRANCH
  unset CURRENT_GIT_HASH
  unset LOCAL_ENV_FILE
  unset PROJECT_ENV_FILE
  unset PROJECT_NAME
  unset PROJECT_ROOT
  unset PROJECT_TITLE
  unset PROJECT_VERSION
  unset PROJECT_VERSION_DIST
}

export -f ci-env-display
export -f ci-env-init
export -f ci-env-require
export -f ci-env-reset
