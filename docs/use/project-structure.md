# Project Structure

## What Files and Directories Does CICEE Require?

CICEE expects that a project repository is structured in the following manner.

* _Project repository root_
  * `ci/`
    * `bin/` - _Optional._ Conventional location of executable scripts intended to be run in a continuous integration environment. Examples: compile source script, run automated tests script, publish to repository script.
    * `docker-compose.dependencies.yml` - _Optional._ Docker-compose services definitions providing dependencies for continuous integration environment. Examples: databases.
    * `docker-compose.project.yml` - _Optional._ Project-specific continuous integration environment definition. Supports declaring dependencies, specifying default environment variables, etc.
    * `Dockerfile` - _Required, unless `exec` is executed with `--image`._ - Dockerfile defining continuous integration environment.

## What Files and Directories Does CICEE Recommend For a Project CI Workflow?

CICEE includes a template for continuous integration workflows. This template initializes portions of the recommended project configuration.

* _Project repository root_
  * `ci/`
    * `bin/` - CI entrypoints. These shell scripts execute one or more CI workflows (in `ci/libexec/workflows`, implemented as shell functions) using `cicee lib exec`.
      * `publish.sh` - Build and publish the project's artifact composition. Examples: creating and publishing a NuGet package, building and pushing a Docker image. Intended for use by a continuous integration build agent or similar infrastructure. Generally used upon the merge of a pre-release feature branch into development (e.g., `dev` or `develop` branch) or a release branch (e.g., into `main` or `trunk`). Default implementation run `ci-compose` and `ci-publish` workflows.
      * `validate.sh` - Build and validate the project's source. Intended for use during the validation of a pull request. Default implementation runs `ci-validate` and `ci-compose` workflows, to both validate and create local build output.
    * `env.local.sh` - _Optional._ Local environment initialization script. Expected to be **ignored** by source control. Sourced by CICEE CI library during entrypoint script execution.
    * `env.project.sh` - _Optional._ Team/shared environment initialization script. Expected to be _stored_ in source control. Sourced by CICEE CI library during entrypoint script execution.
    * `libexec/`
      * `ci-workflows.sh` - _Optional._ Project-specific library of continuous integration workflows (implemented as shell functions). These workflows are intended to be executed in entrypoint scripts. Examples: validate project for a pull request, create a distribution package (e.g., NuGet or NPM package), and publish the package to a repository. This is an _alternative_ to placing individual workflow scripts in `workflows/`.
      * `actions/` - _Optional._ Project-specific CI action functions. Each shell script in this folder is inferred to define and export a named function which executes a reusable unit of CI work. These functions are expected to be executed by one or more workflows (in `workflows/`). E.g., `compress-artifacts.sh` might export a `compress-artifacts` function which uses `zip` to compress build artifacts.
      * `workflows/` - Project-specific CI workflows. Each shell script in this folder is inferred to define and export a named function which executes a composable CI workflow. These are an _alternative_ to using `ci/libexec/ci-workflows.sh`. These functions are expected to be executed by one or more entrypoints (in `ci/bin/`). E.g., `ci-validate-for-pull-request.sh` might export a `ci-validate-for-pull-request` function which runs linting and executes unit tests.
        * `ci-compose.sh` - Build the project's artifact composition. Intended for use in local debugging, build validation, or when another continuous integration tool will process the build output.
        * `ci-publish.sh` - Build and publish the project's artifact composition. Examples: creating and publishing a NuGet package, building and pushing a Docker image. Intended for use by a continuous integration build agent or similar infrastructure. Generally used upon the merge of a pre-release feature branch into development (e.g., `dev` or `develop` branch) or a release branch (e.g., into `main` or `trunk`).
        * `ci-validate.sh` - Build and validate the project's source. Intended for use during the validation of a pull request.
  * `project-metadata.json` (or `package.json`) - Defines metadata about the project. E.g., name, release version, and CI environment expectations. (NOTE: `project-metadata.json` is the canonical file containing this metadata. However, NPM's `package.json` will be read if `project-metadata.json` is unavailable. _Initialized by [`init repository`][] and [`template init`][]._)

### Project Metadata

The following example shows an expected `project-metadata.json`.

```json
{
    "ciEnvironment": {
        "variables": [
            {
                "description": "NuGet API Key",
                "name": "NUGET_API_KEY",
                "required": true,
                "secret": true
            },
            {
                "default": "https://api.nuget.org/v3/index.json",
                "description": "NuGet source",
                "name": "NUGET_SOURCE",
                "required": true,
                "secret": false
            }
        ]
    },
    "name": "neat-project-name",
    "title": "The NEAT Project",
    "version": "0.7.2"
}
```

* `ciEnvironment` - An object defining metadata regarding the expected or required continuous integration environment.
  * `variables` - An array of CI variable definitions.
* `name` - Project name. Expected to be in `lower-kebab-case`.
* `title` - Human-readable project title.
* `version` - Project release version. Expected to be in `Major.Minor.Patch` format, e.g., `3.1.6`.

#### CI Environment Variable Definition

```json
{
    "default": "https://api.nuget.org/v3/index.json",
    "description": "NuGet source",
    "name": "NUGET_SOURCE",
    "required": true,
    "secret": false
}
```

* `default` - _Optional._ Default value for the variable.
* `description` - _Optional._ Human-readable description for the variable.
* `name` - **Required**. Variable name. E.g., `PROJECT_NAME`, `REMOTE_DEPENDENCY_URL`.
* `required` - _Optional._ Is the variable required?
* `secret` - _Optional._ Is the variable's value secret?

[`init repository`]: ./initialize-repository.md
[`template init`]: ./template-init.md
