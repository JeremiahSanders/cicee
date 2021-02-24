# Continuous Integration Containerized Execution Environment (CICEE)

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
* [What Files and Directories Does CICEE Require?][]
* [Using `cicee`][using-cicee]

[How does CICEE work?]: docs/what/how-does-cicee-work.md
[Installation or update]: docs/use/installation-or-update.md
[What Files and Directories Does CICEE Require?]: docs/use/project-structure.md
[using-cicee]: docs/use/using-cicee.md 
