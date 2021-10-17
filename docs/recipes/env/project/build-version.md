# Customizing Project CI Environment Build Version

The [CI environment][ci-library] provides two version-related environment variables: `PROJECT_VERSION`, project distributable `Major.Minor.Patch` semantic version, and `PROJECT_VERSION_DIST`, project distributable version.

`PROJECT_VERSION` is the current project semantic version, as defined in [project metadata][project-structure].

`PROJECT_VERSION_DIST` is, by default, dependent upon `RELEASE_ENVIRONMENT`. If `RELEASE_ENVIRONMENT` is `true`, then `PROJECT_VERSION_DIST` matches `PROJECT_VERSION`. If `RELEASE_ENVIRONMENT` is unset (i.e., the default) or any value other than `true`, then `PROJECT_VERSION_DIST` is `${PROJECT_VERSION}-sha-${CURRENT_GIT_HASH}`.

## Goal

The goal of this recipe is to customize the `PROJECT_VERSION_DIST`. Specifically, to use a different versioning mechanism for prerelease builds (i.e., the default build environment).

### Possible Reasons / Use Cases

Common use cases are:

* Consistent, sortable prerelease versions.
  * The example below uses "epoch seconds", the elapsed seconds since `1970-01-01 00:00:00 UTC`, to fulfill this use case.
  * Alternative implementations may use the build execution UTC date and time, i.e., `$(TZ="utc" date "+%Y-%m-%d-%H-%M-%S")`, which would yield a distributable version like `1.0.3-build-2021-10-16-18-42-01`.
    * **Note**: The prerelease version should **only** follow the format of `${PROJECT_VERSION}-${SUFFIX}`, where `${SUFFIX}` is **only** _alphanumeric_ and _hyphen_ (`-`) characters. This convention exists for wide compatibility with infrastructure, e.g., compatibility with `dotnet` project versions, `npm` package versions, `docker` image tags.
* Calculate a different `PROJECT_VERSION` to use when generating `PROJECT_VERSION_DIST`.
  * This use case supports improved compatibility with the expectations of infrastructure.
    * For example, .NET understands a version of `1.0.3-sha-abcdef0` to _precede_ `1.0.3`. The default `PROJECT_VERSION_DIST` would actually represent code added _after_ `1.0.3` was merged to `main`. Thus, from the `dotnet` tooling perspective, builds executed with code _added after `1.0.3`_, **but** _not yet merged to `main`_, must have a version of `1.0.4-sha-` to be considered newer than `1.0.3`, or prerelease current.
  * A hypothetical implementation may do so by examining the `git` commits which are present in the current branch but which _are not present_ in the repository's `main` branch. Those commit messages may then be parsed to determine the _current, effective_ semantic version.

## Example

In this example, the distribution version will be changed to `${PROJECT_VERSION}-build-${BUILD_EXECUTION_EPOCH_SECONDS}`, using the elapsed seconds since `1970-01-01 00:00:00 UTC` as a consistent, ever incrementing build number.

> ### Prerequisite
>
> Create a project CI environment script, saved to `${PROJECT_ROOT}/ci/env.project.sh`. This script should be committed to source control.

### Project Environment `env.project.sh` Script

```bash
#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

export PROJECT_VERSION_DIST="${PROJECT_VERSION}-build-$(date +'%s')"
```

[ci-library]: ../../../use/ci-library.md
[project-structure]: ../../../use/project-structure.md
