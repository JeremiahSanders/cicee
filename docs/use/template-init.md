# Initialize Project Continuous Integration Scripts

Initializes a project repository with continuous integration workflow scripts.

```bash
$ cicee template init --help
init:
  Initialize project CI scripts.

Usage:
  cicee template init [options]

Options:
  -p, --project-root <project-root> (REQUIRED)    Project repository root directory [default: present working directory]
  -f, --force                                     Force writing files. Overwrites files which already exist.
  -?, -h, --help                                  Show help and usage information
```

## Project Continuous Integration Template

### Entrypoints

CI entrypoints are executable shell scripts which execute one or more CI workflows (implemented as shell functions) using `cicee lib exec`. They are expected to be executed from a development environment properly configured for the project **and/or** from within a containerized CI environment (created by [`cicee exec`][]).

The following CI entrypoints are initialized in `${PROJECT_ROOT}/ci/bin/`:

* `validate.sh` - Validate the project code, by executing the `ci-validate` and `ci-compose` workflows, below.
  * Expected use: Execute during pull request review to provide static code analysis and run tests; perform a dry-run composition.
* `publish.sh`  - Builds and publishes the project distributable artifacts, by executing the `ci-compose` and `ci-publish` workflows, below.
  * E.g., push a Docker image, publish an NPM package
  * Expected use: Execute after a project repository merge creates a new project release. E.g., after a merge to 'main' or 'trunk'

### Workflows

CI workflows are shell scripts which define and export a shell `function`. The body of the shell function assumes the CICEE [CI library][] is loaded when it is executed, and that `ci-env-init` (described in [CI library][]) was executed.

Canonically, the function names are prefixed with `ci-` and the file name matches the function name, for ease of maintenance.

The following CI workflows are initialized in `${PROJECT_ROOT}/ci/libexec/workflows/`:

* `ci-validate.sh` - Validate the project code.
  * E.g., compile source, run tests, lint
* `ci-compose.sh`  - Builds the project distributable artifacts.
  * E.g., NuGet packages, Docker images, zip archives
* `ci-publish.sh`  - Builds and publishes the project distributable artifacts.
  * E.g., push a Docker image, publish an NPM package

[`cicee exec`]: ./execute.md
[CI library]: ./ci-library.md
