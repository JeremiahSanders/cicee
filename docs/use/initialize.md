# Initialize a Project for CICEE Use

Initializes a project directory, creating suggested CICEE files.

```bash
$ cicee init --help
Description:
  Initialize project. Creates suggested CICEE files.

Usage:
  cicee init [command] [options]

Options:
  -p, --project-root <project-root> (REQUIRED)  Project repository root directory [default:
                                                /c/code/TEMP_PROJECT_FOR_CICEE_TEST]
  -i, --image <image>                           Base CI image for $PROJECT_ROOT/ci/Dockerfile.
  -f, --force                                   Force writing files. Overwrites files which already exist. [default: False]
  -?, -h, --help                                Show help and usage information

Commands:
  repository  Initialize project repository. Creates suggested CICEE files and continuous integration scripts. Optionally
              includes CICEE CI library.
```
