# Continuous Integration Containerized Execution Environment (CICEE)

## What is CICEE?

CICEE is an opinionated orchestrator of continuous integration processes. CICEE [executes commands in a Docker container][cicee-exec], using the files in your project repository, and provides a convention-based structure for fulfilling dependencies.

CICEE also provides a [continuous integration shell function library][cicee-lib] to support the use of shell script-based continuous integration workflows.

### What does CICEE require? What are its dependencies?

* `bash`: bash shell
* `docker`: Docker command-line interface
* `docker-compose`: Docker Compose command-line interface (compose file version `3.7` support required)
* `dotnet`: .NET runtime (`6.x` supported)

## Why use CICEE?

CICEE users' most common use cases:

* Validating project code, e.g., during a pull request review, consistently on both developer workstations and continuous integration servers.
* Assembling distributable artifacts, e.g., Docker images or NPM packages.
* Running integration tests requiring dependencies, e.g., databases.
* Executing code cleanup, linting, reformatting, or other common development workflows, without prior tool installation.

## How do you use CICEE?

* [Installation or update][]
* [What Files and Directories Does CICEE Require?][project-structure]
* [Using `cicee`][using-cicee]


## Quickstart

### Step 0: Install CICEE

```bash
dotnet tool install -g cicee
```

> If you already have CICEE installed, but want to update to the latest release:
>
> ```bash
> dotnet tool update -g cicee
> ```

### Step 1: Add a Continuous Integration Containerized Execution Environment

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee init repository`][cicee-init-repository].

```bash
dotnet new tool-manifest && dotnet tool install --local cicee && dotnet cicee init repository
```

This adds:

* a .NET local tool installation of CICEE
* a `Dockerfile` which will provide all the tools needed to perform the project's continuous integration tasks.
* `docker-compose` files which define the continuous integration containerized execution environment.
* a small, flexible continuous integration workflow template. Three initial workflows are provided:
  * `compose`: Create the project's distributable artifacts. For example, render SASS to CSS, compile source code, build docker images, compress zip archives, package for NPM, etc.
  * `publish`: Publish the project's distributable artifacts to their repositories. For example, push docker images, publish a package to NuGet, etc.
  * `validate`: Validate the current project for correctness, completeness, or other rules. **Supports automated checks which should be executed during pull request review.**

### Step 2: _Try It Out_

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee exec`][cicee-exec] and provide one of the CI workflow entry points.

```bash
dotnet cicee exec --entrypoint ci/bin/validate.sh
```

### Next Step

* Update [continuous integration configuration][project-structure]. This is normally done in `project-metadata.json` (which was created by `cicee init repository`). However, _if there is no_ `project-metadata.json`, CICEE will read NPM's `package.json`, if present.
  * Update the project's name and description, if needed.
  * Update the current `Major.Minor.Patch` version.
  * Configure required environment variables and defaults.
* Set up the continuous integration workflow.

[cicee-exec]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/execute.md
[cicee-init-repository]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/initialize-repository.md
[cicee-lib]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/ci-library.md
[cicee-template-init]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/template-init.md
[cicee-template-lib]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/template-lib.md
[Installation or update]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/installation-or-update.md
[project-structure]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/project-structure.md
[using-cicee]: https://github.com/JeremiahSanders/cicee/tree/main/docs/use/using-cicee.md
