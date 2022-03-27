# Continuous Integration Containerized Execution Environment (CICEE)

[![NuGet](https://badgen.net/nuget/v/cicee/)](https://www.nuget.org/packages/cicee/)

## What is CICEE?

CICEE is an opinionated orchestrator of continuous integration processes. CICEE [executes commands in a Docker container][cicee-exec], using the files in your project repository, and provides a convention-based structure for fulfilling dependencies.

CICEE also provides a [continuous integration shell function library][cicee-lib] to support the use of shell script-based continuous integration workflows.

* [How does CICEE work?][]

### What does CICEE require? What are its dependencies?

* `bash`: bash shell
* `docker`: Docker command-line interface
* `docker-compose`: Docker Compose command-line interface (compose file version `3.7` support required)
* `dotnet`:- .NET runtime (`5` and `6` supported)

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

Execute [`cicee init`][cicee-init].

```bash
cicee init
```

This adds a `Dockerfile` which will provide all the tools needed to perform the project's continuous integration tasks.

It also creates `docker-compose` files which define the continuous integration containerized execution environment.

#### (Optional) Add CICEE Shell Library

CICEE's core runtime ability is `cicee exec`: executing a specified Docker `entrypoint` and `command` within the continuous integration containerized execution environment. However, that requires a `cicee` installation.

By installing the CICEE shell library, the same `cicee exec` process can be performed _without installing `cicee`_. For example, on a continuous integration build server.

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee template lib`][cicee-template-lib].

```bash
cicee template lib
```

### Step 2: Add a Continuous Integration Workflow Template

Open a Bash terminal session **in the root directory of the project**.

Execute [`cicee template init`][cicee-template-init].

```bash
cicee template init
```

This adds a small, flexible continuous integration workflow template. Three initial workflows are provided:

* `compose`: Create the project's distributable artifacts. For example, render SASS to CSS, compile source code, build docker images, compress zip archives, package for NPM, etc.
* `publish`: Publish the project's distributable artifacts to their repositories. For example, push docker images, publish a package to NuGet, etc.
* `validate`: Validate the current project for correctness, completeness, or other rules. **Supports automated checks which should be executed during pull request review.**

### Step 3: _Try It Out_

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

### Workflow Recipes

#### Validate Workflow Recipes

* [`create-react-app` Node.js React][validate-nodejs-create-react-app]
* [.NET][validate-dotnet] (applies to all currently-supported .NET languages and project types)

#### Compose Workflow Recipes

> Some projects may require only a single recipe, e.g., [.NET NuGet packages][compose-dotnet-nuget]. Other projects may require multiple, e.g., a [React SPA][compose-nodejs-create-react-app] hosted by an [ASP.NET application][compose-dotnet-executable] which is distributed as a [Docker image][compose-docker-image].

* [AWS CDK cloud assembly][compose-cdk]
* [`create-react-app` Node.js React][compose-nodejs-create-react-app]
* [Docker image][compose-docker-image]
* [.NET executable][compose-dotnet-executable] (e.g., ASP.NET application)
* [.NET NuGet package][compose-dotnet-nuget]
* [Zip archive][compose-zip] (compress other composed artifacts in preparation for distribution)

#### Publish Workflow Recipes

* [Copy zip archive to AWS S3][publish-aws-s3-zip]
* [Publish .NET NuGet package][publish-dotnet-nuget]
* [Push Docker image to AWS ECR][publish-docker-aws-ecr]
* [Push Docker image to Docker Hub][publish-docker-docker-hub]

### Project Environment Initialization Recipes

* [Customize or override build version][env-project-build-version]
* [Generate `NuGet.config` for private NuGet source][env-project-nuget-auth] (e.g., to enable _private_ package sources for `dotnet restore`)
* [Import (`bash` `source`) custom environment scripts][env-project-custom-env]
* [Login to AWS ECR][env-project-aws-ecr-login] (e.g., to use private images which are hosted in AWS ECR for base image in `ci/Dockerfile`)

### Example Combined Recipes

* [NuGet Library / .NET Tool][]
  * Combines:
    * Validate
      * [.NET][validate-dotnet]
    * Compose
      * [.NET NuGet package][compose-dotnet-nuget]
    * Publish
      * [Publish .NET NuGet package][publish-dotnet-nuget]
* [ASP.NET Docker Image / Self-contained Command Line Tool Docker Image][]
  * Combines:
    * Validate
      * [.NET][validate-dotnet]
    * Compose
      * [.NET executable][compose-dotnet-executable] (e.g., ASP.NET application)
      * [Docker image][compose-docker-image]
    * Publish
      * [Push Docker image to Docker Hub][publish-docker-docker-hub]
* [AWS CDK application hosting `create-react-app` SPA][create-react-app-aws-cdk]
  * Combines:
    * Validate
      * [`create-react-app` Node.js React][validate-nodejs-create-react-app]
    * Compose
      * [`create-react-app` Node.js React][compose-nodejs-create-react-app]
      * [AWS CDK cloud assembly][compose-cdk]
      * [Zip archive][compose-zip] (compress other composed artifacts in preparation for distribution)
    * Publish
      * [Copy zip archive to AWS S3][publish-aws-s3-zip]

[ASP.NET Docker Image / Self-contained Command Line Tool Docker Image]: ./docs/recipes/dotnet/docker-image.md
[cicee-exec]: docs/use/execute.md
[cicee-init]: docs/use/initialize.md
[cicee-lib]: docs/use/ci-library.md
[cicee-template-init]: docs/use/template-init.md
[cicee-template-lib]: docs/use/template-lib.md
[compose-cdk]: docs/recipes/aws/cdk-compose.md
[compose-docker-image]: docs/recipes/docker/compose-docker-image.md
[compose-dotnet-executable]: docs/recipes/dotnet/compose-project.md
[compose-dotnet-nuget]: docs/recipes/dotnet/compose-nuget.md
[compose-nodejs-create-react-app]: docs/recipes/nodejs/create-react-app-compose.md
[compose-zip]: docs/recipes/compose-zip.md
[create-react-app-aws-cdk]: docs/recipes/nodejs/create-react-app-aws-cdk.md
[env-project-aws-ecr-login]: docs/recipes/env/project/aws-ecr-login.md
[env-project-build-version]: docs/recipes/env/project/build-version.md
[env-project-custom-env]: docs/recipes/env/project/custom-env.md
[env-project-nuget-auth]: docs/recipes/env/project/nuget-private-source.md
[How does CICEE work?]: docs/what/how-does-cicee-work.md
[Installation or update]: docs/use/installation-or-update.md
[NuGet Library / .NET Tool]: ./docs/recipes/dotnet/nuget-library.md
[project-structure]: docs/use/project-structure.md
[publish-aws-s3-zip]: docs/recipes/aws/publish-s3-zip.md
[publish-docker-aws-ecr]: docs/recipes/docker/publish-image-aws-ecr.md
[publish-docker-docker-hub]: docs/recipes/docker/publish-image-docker-hub.md
[publish-dotnet-nuget]: docs/recipes/dotnet/publish-nuget.md
[Set up CI workflow for .NET project]: docs/recipes/dotnet/README.md
[using-cicee]: docs/use/using-cicee.md
[validate-dotnet]: docs/recipes/dotnet/validate.md
[validate-nodejs-create-react-app]: docs/recipes/nodejs/create-react-app-validate.md
