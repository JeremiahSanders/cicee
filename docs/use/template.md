# Initialize Project Continuous Integration Scripts

To initialize a project repository with continuous integration workflow scripts.

```bash
$ dotnet run --project src -- template init --help
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

The following workflow entrypoints are initialized:

* `validate.sh` - Validate the project code.
  * Expected use: Execute during pull request review to provide static code analysis and run tests.
* `compose.sh`  - Builds the project distributable artifacts.
  * E.g., NuGet packages, Docker images, zip archives
  * Expected use: Execute locally to create distributable artifacts for local use or manual validation.
* `publish.sh`  - Builds and publishes the project distributable artifacts.
  * E.g., push a Docker image, publish an NPM package
  * Expected use: Execute after a project repository merge creates a new project release. E.g., after a merge to 'main' or 'trunk'
