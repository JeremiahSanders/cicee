# Customizing Project CI Environment to Load Additional Initialization Script

CICEE's [library][ci-library] provides a base CI environment initialization process. It also provides extensibility points for customizing this process - both shared across environments (in `${PROJECT_ROOT}/ci/env.project.sh`) and private to a local environment (in `${PROJECT_ROOT}/ci/env.local.sh`). This recipe _expands_ upon that.

## Goal

The goal of this recipe is to load (`bash` `source`) an additional, custom initialization shell script during CI environment initialization.

## Possible Reasons / Use Cases

* Reuse an existing script.
  * When adopting CICEE and its [CI library][ci-library] in an _existing_ project, an existing initialization script may exist. For example, it might load the repository's project name or version from proprietary sources.

## Example

### Imagined Arrangement

In this example, the _imagined_ project contains:

* A custom initialization script, located at `${PROJECT_ROOT}/bin/load-env.sh`.
  * This script `export`s some custom environment variables: `REPO_NAME` and `APP_VERSION`. We will also imagine that it performs other important tasks which we want to maintain, but which are not _directly_ used by CICEE or its [library][ci-library].

This example will load the existing initialization script, and re-export the proprietary environment variables with the names required by CICEE.

> ### Prerequisite
>
> Create a project CI environment script, saved to `${PROJECT_ROOT}/ci/env.project.sh`. This script should be committed to source control.

### Project Environment `env.project.sh` Script

> When using this recipe, update the value of `PROPRIETARY_INITIALIZATION_SCRIPT_PATH` with the appropriate path for the project. Additionally, remove the `export` lines. Replace them with project-appropriate additional steps, if needed.

```bash
#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

PROPRIETARY_INITIALIZATION_SCRIPT_PATH="${PROJECT_ROOT}/bin/load-env.sh"
if [[ -f "${PROPRIETARY_INITIALIZATION_SCRIPT_PATH}" ]]; then
  # The proprietary initialization script exists.

  # Execute the script within this shell session.
  source "${PROPRIETARY_INITIALIZATION_SCRIPT_PATH}"

  # Export CI environment values from the proprietary initialization script using the corresponding, canonical CICEE names.
  export PROJECT_NAME="${REPO_NAME}"
  export PROJECT_VERSION_DIST="${APP_VERSION}"
fi
```

[ci-library]: ../../../use/ci-library.md
