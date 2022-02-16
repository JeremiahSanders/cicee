# Continuous Integration Action Library

CICEE includes a library of shell functions which provide patterned executions of various common continuous integration actions. E.g., "dotnet build", "docker push". By sourcing this function library in a continuous integration script, project continuous integration workflows can be templated for quick intialization.

```bash
$ cicee lib --help
lib:
  Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source "$(cicee lib --shell bash)"'.

Usage:
  cicee lib [options]

Options:
  -s, --shell <bash>    Shell template.
  -?, -h, --help        Show help and usage information
```

> Currently, Bash is the only supported shell. The `--shell` option is not required. When not provided, `--shell` defaults to `bash`.

## Why use the Continuous Integration Action Library?

This library of functions provides patterned executions of common continuous integration actions. For example, executing `ci-dotnet-pack` doesn't just run `dotnet pack` - it provides several build parameters which ensure consistent package naming, versioning, and output location.

These actions are designed to encourage consistent patterns across projects, across languages, and across tools. Generated documentation goes into a specific location, regardless of source language - `PROJECT_ROOT/build/docs`. Packaged artifacts, e.g., zip files, NuGet packages, NPM packages, go into a specific, structured location, regardless of language - `PROJECT_ROOT/build/dist`.

## To Import the Continuous Integration Action Library

CICEE exposes a `lib` command to output the importable library script path, for supported shells. The `lib` command provides a `--shell` (short `-a`) option to request a specific template.

| Shell  | `--shell` Value | Example command to import library |
| ------ | --------------- | --------------------------------- |
| `bash` | `bash`          | `source "$(cicee lib)"`           |

## Using the Continuous Integration Action Library

Once the action library is imported, the session or script has access to the continuous integration functions exposed.

### Core Actions

* `ci-env-display` - Display initialized CI environment.
* `ci-env-init` - Initialize CI environment. **ALL OTHER ACTIONS ASSUME THIS ENVIRONMENT.**
* `ci-env-require` - Validates that the CI environment is initialized. Exits with `1` if validation fails.
* `ci-env-reset` - Unsets predenfined environment variables set by ci-env-init. Does not reset project or local variables.

#### Core Required Variables

| Variable       | Description                                   | Default value              |
| -------------- | --------------------------------------------- | -------------------------- |
| `PROJECT_ROOT` | The root directory of the project repository. | Present working directory. |

### Amazon Web Services (AWS) Actions

Contains functions which interact with Amazon Web Services CLI (aws) and CDK CLI (cdk).

* `ci-aws-cdk-deploy` - Execute `cdk deploy`. Arguments passed to this function are passed to `cdk`.
* `ci-aws-cdk-synth` - Execute `cdk synth`. Arguments passed to this function are passed to `cdk`.
* `ci-aws-ecr-docker-login` - Authenticate with AWS ECR and use authorization to execute docker login.

#### AWS Required Variables

| Variable      | Description            |
| ------------- | ---------------------- |
| `AWS_ACCOUNT` | AWS account ID/number. |
| `AWS_REGION`  | AWS region.            |

### Docker Actions

Contains functions which interact with Docker and Docker Compose.

* `ci-docker-build` - Execute 'docker build'.
* `ci-docker-push` - Execute 'docker push'.

#### Docker Required Variables

| Variable       | Description       |
| -------------- | ----------------- |
| `DOCKER_IMAGE` | Docker image tag. |

### GitHub Actions

Contains functions supporting the use of GitHub.

* `ci-github-nuget-push` - Use curl to push NuGet packages to GitHub packages. Does not use 'dotnet nuget push' to avoid authorization errors.
* `ci-github-nuget-config-generate` - Generate NuGet.Config at a provided location (default: `PROJECT_ROOT`) with authenticated GitHub packages source.

#### GitHub Required Variables

| Variable            | Description                              |
| ------------------- | ---------------------------------------- |
| `GITHUB_AUTH_TOKEN` | GitHub API authorization token.          |
| `GITHUB_OWNER`      | GitHub repository owner, i.e., username. |

### .NET Actions

Contains functions which interact with the .NET CLI (dotnet) and its related utilities, e.g., NuGet. Unless specified otherwise, all functions target the `PROJECT_ROOT` and assume a .NET solution exists there.

* `ci-dotnet-build` - Execute 'dotnet build'. Arguments passed to this function are passed to `dotnet`.
* `ci-dotnet-clean` - Execute 'dotnet clean'. Arguments passed to this function are passed to `dotnet`.
* `ci-dotnet-nuget-push` - Execute 'dotnet nuget push'.
* `ci-dotnet-pack` - Execute 'dotnet pack'. Arguments passed to this function are passed to `dotnet`.
* `ci-dotnet-publish` - Execute 'dotnet publish'. Arguments passed to this function are passed to `dotnet`.
* `ci-dotnet-restore` - Execute 'dotnet restore'. Arguments passed to this function are passed to `dotnet`.
* `ci-dotnet-test` - Execute 'dotnet test'. Arguments passed to this function are passed to `dotnet`.

#### .NET Conditionally-required Variables

| Variable        | Description                                                                                     |
| --------------- | ----------------------------------------------------------------------------------------------- |
| `NUGET_API_KEY` | NuGet source API key. Required for pushing NuGet packages.                                      |
| `NUGET_SOURCE`  | NuGet source, e.g., `https://api.nuget.org/v3/index.json`. Required for pushing NuGet packages. |

### Utility Actions

* `require-var` - Require that one or more variables are set. Required variables are provided as string arguments. E.g. "PROJECT_ROOT", "NPM_API_KEY".

## CI Execution Environment

### Initialization

Once the CI library is loaded, the continuous integration execution environment can be initialized. This is done with the `ci-env-init` function.

All CI actions implicitly require this function to be executed. Its purpose is to provide a consistent execution environment for project-level activities. For example, by using a consistent `BUILD_PACKAGED_DIST` variable, CI actions creating an output zip file, NPM package, or APK can all place their artifacts in consistent locations.

Expected environment available to all CI actions, after `ci-env-init` is executed:

_Contextual_

* `PROJECT_NAME` - Project name. By convention this should be in lower kebab case. I.e., multi-word-project-name. This will be used to pattern conventional output ths, e.g., as part of a zip archive file name.
* `PROJECT_ROOT` - Project root directory.
* `PROJECT_TITLE` - Project human-readable name. Defaults to PROJECT_NAME.
* `PROJECT_VERSION` - Project distributable Major.Minor.Patch semantic version. I.e., 2.3.1.
* `PROJECT_VERSION_DIST` - Project distributable version. Expected to be in the following format: Release versions: `Major.Minor.Patch`, e.g., `4.1.7`. Pre-release versions: `Major.Minor.Patch-hyphenated-alphanumeric-suffix`, E.g., `4.1.7-alpha`. These formats are very important. They help ensure compatibility across .NET projects, .NET NuGet packages, and Docker tags.
  * If not overridden, the library initializes this with `Major.Minor.Patch-BuildDateUtc-BuildTimeUtc-sha-GitSha`, e.g., `4.1.7-20220315-142217-sha-a7328f`.
  * If the `PROJECT_VERSION` can be parsed as a `Major.Minor.Patch` version, e.g., `4.1.7`, then the library will infer this to be a _prerelease build_ for the _next minor version_, to increase clarity of ordering when distributing prerelease versions. E.g., given `PROJECT_VERSION` of `4.1.7`, a prerelease build would be similar to `4.2.0-20220315-142217-sha-a7328f`.
* `CURRENT_GIT_BRANCH` - Current Git branch.
* `CURRENT_GIT_HASH` - Current Git hash.

_Configuration_

The `CIENV_VARIABLES*` variables below are all derived from loading `.ciEnvironment.variables` JSON path from a project metadata file (if one exists) using `jq`.

* `CIENV_VARIABLES` - Array of project-specific CI environment variable names.
* `CIENV_VARIABLES_PUBLIC` - Array of project-specific CI environment variable names which are NOT marked secret (by their .secret JSON path property).
* `CIENV_VARIABLES_REQUIRED` - Array of project-specific CI environment variable names which are marked required (by their .required JSON path property).
* `CIENV_VARIABLES_SECRET` - Array of project-specific CI environment variable names which ARE marked secret (by their .secret JSON path property).

_Conventional Output_

* `BUILD_DOCS="${BUILD_ROOT}/docs"` - Project distributable documentation which would accompany packaged build output.
* `BUILD_PACKAGED_DIST="${BUILD_ROOT}/dist"` - Project packaged build output. E.g., .NET NuGet packages, zip archives, AWS CDK cloud assemblies.
* `BUILD_ROOT` - Project build output root directory.
* `BUILD_UNPACKAGED_DIST="${BUILD_ROOT}/app"` - Project unpackaged build output. E.g., processed output which might be further packaged or compressed before distribution. E.g., .NET DLLs, Java class files.

#### Initialization Workflow

1. Initialize convention-based CI enviroment variables.
2. Attempt to load project metadata by convention.
    * Populates `PROJECT_NAME`, `PROJECT_TITLE`, and `PROJECT_VERSION` from JSON file. From "name", "title", and "version" properties, respectively.
    * Default location: `PROJECT_ROOT/project-metadata.json`
    * Additional paths: `PROJECT_ROOT/.project-metadata.json`, `PROJECT_ROOT/ci/project-metadata.json`, `PROJECT_ROOT/ci/.project-metadata.json`, `PROJECT_ROOT/package.json`
3. Apply fallback values for expected metadata.
    * `PROJECT_VERSION_DIST` defaults to `PROJECT_VERSION` if `RELEASE_ENVIRONMENT=true` in the environment, or `PROJECT_VERSION-sha-CURRENT_GIT_HASH` if not.
4. Source project environment file, if available; enabling setting project defaults and overrides to convention.
    * This file is assumed to be stored in version control and all environments (e.g., both local and build server) are assumed to have the same file.
    * Default location: `PROJECT_ROOT/ci/env.project.sh`
    * Additional paths: `PROJECT_ROOT/ci/env.ci.sh`, `PROJECT_ROOT/ci/ci.env`, `PROJECT_ROOT/ci/project.sh`, `PROJECT_ROOT/env.project.sh`, `PROJECT_ROOT/ci.env`, `PROJECT_ROOT/project.sh`
5. Source local environment file, if available; enabling setting local defaults and overrides to convention.
    * This file is assumed to NOT BE stored in version control. No consistency between environments is assumed, though patterns and templates are recommended.
    * Default location: `PROJECT_ROOT/ci/env.local.sh`
    * Additional paths: `PROJECT_ROOT/ci/env.sh`, `PROJECT_ROOT/ci/.env`, `PROJECT_ROOT/env.local.sh`, `PROJECT_ROOT/env.sh`, `PROJECT_ROOT/.env`
