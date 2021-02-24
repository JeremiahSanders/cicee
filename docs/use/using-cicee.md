# Using CICEE

Once installed, CICEE is accessible by name, with the `cicee` command.

```bash
$ cicee

-- cicee (v0.2.0) --

Required command was not provided.

cicee:
  cicee

Usage:
  cicee [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  exec    Execute a command in a containerized execution environment.
  init    Initialize project. Creates suggested cicee files.
```

## Initialize a Project Repository

To initialize a project repository, creating suggested files.

```bash
$ cicee init --help
init:
  Initialize project. Creates suggested cicee files.

Usage:
  cicee init [options]

Options:
  -p, --project-root <project-root> (REQUIRED)    Project repository root directory [default: present working directory]
  -i, --image <image>                             Base CI image for $PROJECT_ROOT/ci/Dockerfile.
  -f, --force                                     Force writing files. Overwrites files which already exist.
  -?, -h, --help                                  Show help and usage information
```

## Execute a Command

To execute a command, e.g., run a script.

```bash
$ cicee exec --help
exec:
  Execute a command in a containerized execution environment.

Usage:
  cicee exec [options]

Options:
  -p, --project-root <project-root> (REQUIRED)    Project repository root directory [default: present working directory]
  -c, --command <command>                         Execution command
  -e, --entrypoint <entrypoint>                   Execution entrypoint
  -i, --image <image>                             Execution image. Overrides $PROJECT_ROOT/ci/Dockerfile.
  -?, -h, --help                                  Show help and usage information
```

The `command` and `entrypoint` options directly set the `docker-compose` `command` ([reference][docker-compose-command]) and `entrypoint` ([reference][docker-compose-entrypoint]) arguments executed in the `ci-exec` docker-compose service.

Extend a service named `ci-exec` in the [project's docker-compose file][] to specify dependencies, pull environment variables, etc.

### Run a Script

Executing a script is most common `cicee` use. For example, running a script to build and publish a library to a repository, e.g., NPM, NuGet.

#### Example: Run a Pull Request Code Analysis

The execution below illustrates running a _user-defined script_ named `validate-for-pr.sh`, placed in the canonical `$PROJECT_ROOT/ci/bin` directory.

```bash
$ cicee exec -c ci/bin/validate-for-pr.sh 

-- cicee (v0.2.0) --

Beginning exec...

Project root: C:\temp\test-proj-with-dockerfile
Entrypoint  :
Command     : ci/bin/validate-for-pr.sh
CI Environment:
  No CI environment variables defined.
CICEE Execution Environment:
  CI_COMMAND      : ci/bin/validate-for-pr.sh
  CI_EXEC_CONTEXT : /c/temp/test-proj-with-dockerfile/ci
  LIB_ROOT        : /c/Users/USERNAME/.dotnet/tools/.store/cicee/0.2.0/cicee/0.2.0/tools/net5.0/any/lib
  PROJECT_NAME    : example-project
  PROJECT_ROOT    : /c/temp/test-proj-with-dockerfile

|__
Beginning Continuous Integration Containerized Execution...
__
  | Entrypoint   :
  | Command      : ci/bin/validate-for-pr.sh
  | Project Root : /c/temp/test-proj-with-dockerfile
  | CICEE Library: /c/Users/USERNAME/.dotnet/tools/.store/cicee/0.2.0/cicee/0.2.0/tools/net5.0/any/lib
  | Image        : Not applicable. Using Dockerfile.


[+] Building 0.1s (5/5) FINISHED
 => [internal] load build definition from Dockerfile                                                                                                             0.0s 
 => => transferring dockerfile: 38B                                                                                                                              0.0s 
 => [internal] load .dockerignore                                                                                                                                0.0s 
 => => transferring context: 2B                                                                                                                                  0.0s 
 => [internal] load metadata for mcr.microsoft.com/vscode/devcontainers/universal:linux                                                                          0.0s 
 => CACHED [1/1] FROM mcr.microsoft.com/vscode/devcontainers/universal:linux                                                                                     0.0s 
 => exporting to image                                                                                                                                           0.0s 
 => => exporting layers                                                                                                                                          0.0s 
 => => writing image sha256:acc8b32d896f67bff99c08976f9bf5c4c46c55462dc17960fd1927ecec06e23c                                                                     0.0s 
WARNING: The CI_ENTRYPOINT variable is not set. Defaulting to a blank string.
Pulling ci-exec ... done
WARNING: The CI_ENTRYPOINT variable is not set. Defaulting to a blank string.
Creating network "example-project_default" with the default driver
Creating volume "example-project_npmconfig" with default driver
Creating volume "example-project_nugetconfig" with default driver
Creating volume "example-project_nugetpackages" with default driver
Creating volume "example-project_nugetsourcecache" with default driver
Building ci-exec
Step 1/1 : FROM mcr.microsoft.com/vscode/devcontainers/universal:linux AS build-environment
 ---> b48d91e3646e

Successfully built b48d91e3646e
Successfully tagged example-project_ci-exec:latest
Creating example-project_ci-exec_1 ... done
Attaching to example-project_ci-exec_1
ci-exec_1  | Pulling dependencies...
ci-exec_1  |
ci-exec_1  | Linting...
ci-exec_1  |
ci-exec_1  | Running tests...
ci-exec_1  |
ci-exec_1  | Validation complete!
example-project_ci-exec_1 exited with code 0
Aborting on container exit...
WARNING: The CI_ENTRYPOINT variable is not set. Defaulting to a blank string.
Removing example-project_ci-exec_1 ... done
Removing network example-project_default
Removing volume example-project_npmconfig
Removing volume example-project_nugetconfig
Removing volume example-project_nugetpackages
Removing volume example-project_nugetsourcecache
```

[docker-compose-command]: https://docs.docker.com/compose/compose-file/compose-file-v3/#command
[docker-compose-entrypoint]: https://docs.docker.com/compose/compose-file/compose-file-v3/#entrypoint
[project's docker-compose file]: ../use/project-structure.md
