# Continuous Integration Containerized Execution Environment (CICEE)

[![NuGet](https://badgen.net/nuget/v/cicee/)](https://www.nuget.org/packages/cicee/)

## Quickstart

### Step 1: Install CICEE

```bash
dotnet tool install -g cicee
```

> If you already have CICEE installed, but want to date to the latest release:
>
> ```bash
> dotnet tool update -g cicee
> ```

### Step 2: Add a Continuous Integration Containerized Execution Environment

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee init`][cicee-init].

```bash
cicee init
```

This adds a `Dockerfile` which will provide all the tools needed to perform the project's continuous integration tasks.

It also creates `docker-compose` files which define the continuous integration containerized execution environment.

#### (Optional) Add CICEE Shell Library

CICEE's core runtime ability is `cicee exec`: executing a specified Docker `entrypoint` and `command` within the continuous integration containerized execution environment. However, that requires a `cicee` installation.

By installing the CICEE shell library, the same `cicee exec` process can be performed _without `cicee`_. For example, on a continuous integration build server.

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee template lib`][cicee-template-lib].

```bash
cicee template lib
```

### Step 3: Add a Continuous Integration Workflow Template

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee template init`][cicee-template-init].

```bash
cicee template init
```

This adds a small, flexible continuous integration workflow template. Three initial workflows are provided: 

* `compose`: Create the project's distributable artifacts. For example, render SASS to CSS, compile source code, build docker images, compress zip archives, package for NPM, etc.
* `publish`: Publish the project's distributable artifacts to their repositories. For example, push docker images, publish a package to NuGet, etc.
* `validate`: Validate the current project for correctness, completeness, or other rules. **Supports automated checks which should be executed during pull request review.**

### Step 4: Try It Out!

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee exec`][cicee-exec] and provide one of the CI workflow entry points.

```bash
cicee exec --entrypoint ci/bin/validate.sh
```

### Next Step

* Update [continuous integration configuration][project-structure]. This is normally done in `project-metadata.json` (which was created by `cicee template init`). However, _if there is no_ `project-metadata.json`, CICEE will read NPM's `package.json`, if present.
  * Update the project's name and description, if needed.
  * Update the current `Major.Minor.Patch` version.
  * Configure required environment variables and defaults.
* Set up the continuous integration workflow.

### Next Step Recipes

* [Set up CI workflow for .NET project][]
  * [NuGet Library / .NET Tool][]
  * [ASP.NET Docker Image / Self-contained Command Line Tool Docker Image][]

## What is CICEE?

CICEE is an opinionated orchestrator of continuous integration processes. CICEE executes commands in a Docker container, using the files in your project repository, and provides a convention-based structure for fulfilling dependencies.

* [How does CICEE work?][]

### What does CICEE require? What are its dependencies?

* `bash`
* `docker`
* `docker-compose` (compose file version `3.7` support required)
* `dotnet` - .NET `5` runtime.

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

[ASP.NET Docker Image / Self-contained Command Line Tool Docker Image]: ./docs/recipes/dotnet/docker-image.md
[cicee-exec]: docs/use/execute.md
[cicee-init]: docs/use/initialize.md
[cicee-template-init]: docs/use/template-init.md
[cicee-template-lib]: docs/use/template-lib.md
[How does CICEE work?]: docs/what/how-does-cicee-work.md
[Installation or update]: docs/use/installation-or-update.md
[NuGet Library / .NET Tool]: ./docs/recipes/dotnet/nuget-library.md
[project-structure]: docs/use/project-structure.md
[Set up CI workflow for .NET project]: docs/recipes/dotnet/README.md
[using-cicee]: docs/use/using-cicee.md
