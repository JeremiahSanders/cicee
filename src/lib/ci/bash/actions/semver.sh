#!/usr/bin/env bash
# shellcheck disable=SC1090 # Can't follow non-constant source. Use a directive to specify location.

####
# Continuous integration SemVer action library.
#   These actions are intended to support other actions. Notably, ci-env-init.
#
# Exported library functions:
#   * ci-semver-prerelease-minor - Calculate a SemVer2 prerelease version.
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

##
# CI SemVer Pre-release Minor version calculation.
#   Parameters:
#     1 - Current "production" SemVer base version (in Major.Minor.Patch format). E.g., 4.2.3
#         Defaults to 0.0.0
#     2 - Current commit hash (assumed to be a Git SHA). E.g., a34ff60
#         Defaults to 0000000
#     3 - Current build date and time (in YYYYMMDD-hhmmss format). E.g., 20230116-141103
#         Defaults to current UTC date and time
#
#   Writes a calculated, minor, prerelease version to STDOUT (using echo).
##
function ci-semver-prerelease-minor() {
  local base_version="${1:-${PROJECT_VERSION:-0.0.0}}"
  local git_hash="${2:-${CURRENT_GIT_HASH:-0000000}}"

  local build_date_time="${3:-$(TZ="utc" date "+%Y%m%d-%H%M%S")}"
  local calculated_version=""

  if [[ "${base_version}" =~ ^[0-9]*\.[0-9]*\.[0-9]*$ ]]; then
    # The version is in Major.Minor.Patch format.
    # Calculate next release version. Assume next version is a minor (so we'll use 0 instead of current patch version).
    IFS='.' read -ra PROJECT_VERSION_SEGMENTS <<<"${base_version}"
    local MAJOR="${PROJECT_VERSION_SEGMENTS[0]}"
    local MINOR="${PROJECT_VERSION_SEGMENTS[1]}"
    local PATCH="0"
    # The $(()) converts ${MINOR} to a number.
    local BUMPED_MINOR=$((${MINOR} + 1))
    # NOTE: The "build" string starting the prerelease suffix is required for NuGet limitations to SemVer2 compatibility.
    calculated_version="${MAJOR}.${BUMPED_MINOR}.${PATCH}-build-${build_date_time}-sha-${git_hash}"
  else
    # The version is not in Major.Minor.Patch format.
    # NOTE: The "build" string starting the prerelease suffix is required for NuGet limitations to SemVer2 compatibility.
    calculated_version="${base_version}-build-${build_date_time}-sha-${git_hash}"
  fi

  echo "${calculated_version}"
}

export -f ci-semver-prerelease-minor
