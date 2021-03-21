#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

###
# Run project with "template init -f" and revert ci/bin/ci-workflows.sh to current HEAD.
# I.e., updates the project to use the current CICEE template.
###

dotnet run --project src -- template init -f &&
  git checkout HEAD ci/bin/ci-workflows.sh
