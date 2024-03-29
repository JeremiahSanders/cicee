# Initialize a Project for CICEE Use

Initializes a project directory, creating suggested CICEE files.

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee init --help
Description:
  Initialize project. Creates suggested CICEE files.

Usage:
  cicee init [command] [options]

Options:
  -p, --project-root <project-root> (REQUIRED)  Project repository root directory [default:
                                                $(pwd)]
  -i, --image <image>                           Base CI image for $PROJECT_ROOT/ci/Dockerfile.
  -f, --force                                   Force writing files. Overwrites files which already exist. [default: False]
  -?, -h, --help                                Show help and usage information

Commands:
  repository  Initialize project repository. Creates suggested CICEE files and continuous integration scripts. Optionally
              includes CICEE CI library.
```
