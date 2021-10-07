# Initialize Project with CICEE Execution Library

Initialize project CI with CICEE execution library. Supports `cicee exec`-like behavior without CICEE installation. Installs in `${PROJECT_ROOT}/ci/lib`.

```bash
$ cicee template lib --help
lib:
  Initialize project CI with CICEE execution library. Supports 'cicee exec'-like behavior without CICEE installation.

Usage:
  cicee template lib [options]

Options:
  -p, --project-root <project-root> (REQUIRED)    Project repository root directory [default: present working directory]
  -s, --shell <bash>                              Shell template.
  -f, --force                                     Force writing files. Overwrites files which already exist.
  -?, -h, --help                                  Show help and usage information
```

## CICEE Execution Library Template

The CICEE execution library supports the core functionality of `cicee execc`.

### Using the Bash Template

The Bash CICEE execution library is installed at `${PROJECT_ROOT}/ci/lib/cicee-exec.sh`.

#### Parameters

The following parameters are supported by `${PROJECT_ROOT}/ci/lib/cicee-exec.sh`. All parameters are expected as environment variables.

| Parameter         | Description                                                                             | Default                            |
| ----------------- | --------------------------------------------------------------------------------------- | ---------------------------------- |
| `CI_COMMAND`      | CI [Docker Compose command][].                                                          | _n/a_                              |
| `CI_ENTRYPOINT`   | CI [Docker Compose entrypoint][].                                                       | _n/a_                              |
| `CI_EXEC_CONTEXT` | CI execution context directory.                                                         | `${PROJECT_ROOT}/ci`               |
| `CI_EXEC_IMAGE`   | CI environment Docker image. If provided, this overrides the project's `ci/Dockerfile`. | _n/a_                              |
| `LIB_ROOT`        | CICEE CI library directory.                                                             | `${PROJECT_ROOT}/ci/lib}`          |
| `PROJECT_NAME`    | [Docker Compose project name][].                                                        | Name of present working directory. |
| `PROJECT_ROOT`    | Project root directory.                                                                 | Present working directory.         |

## Examples

> Examples assumed to be executed from the project root.

### Example Execution of Validate Workflow

```bash
$ CI_COMMAND="ci/bin/validate.sh" ./ci/lib/cicee-exec.sh

|__
Beginning Continuous Integration Containerized Execution...
__
  | Entrypoint   : 
  | Command      : ci/bin/validate.sh
  | Project Root : /code/project
  | CICEE Library: /code/project/ci/lib
  | Image        : Not applicable. Using Dockerfile.


Initializing CI environment...
```

### Example Execution of Publish Workflow

```bash
$ CI_ENTRYPOINT="ci/bin/publish.sh" ./ci/lib/cicee-exec.sh

|__
Beginning Continuous Integration Containerized Execution...
__
  | Entrypoint   : ci/bin/publish.sh
  | Command      :
  | Project Root : /code/project
  | CICEE Library: /code/project/ci/lib
  | Image        : Not applicable. Using Dockerfile.


Initializing CI environment...
```

### Example Execution of a One-off Command

In this example, a `dotnet build` is executed in a `mcr.microsoft.com/dotnet/sdk:5.0` container, using the project code.

```bash
$ CI_ENTRYPOINT="dotnet" CI_COMMAND="build" CI_EXEC_IMAGE="mcr.microsoft.com/dotnet/sdk:5.0" ./ci/lib/cicee-exec.sh

|__
Beginning Continuous Integration Containerized Execution...
__
  | Entrypoint   : dotnet
  | Command      : build
  | Project Root : /code/project
  | CICEE Library: /code/project/ci/lib
  | Image        : mcr.microsoft.com/dotnet/sdk:5.0


Initializing CI environment...
```

[Docker Compose command]: https://docs.docker.com/compose/compose-file/compose-file-v3/#command
[Docker Compose entrypoint]: https://docs.docker.com/compose/compose-file/compose-file-v3/#entrypoint
[Docker Compose project name]: https://docs.docker.com/compose/reference/envvars/#compose_project_name
