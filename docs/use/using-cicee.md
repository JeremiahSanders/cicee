# Using CICEE

Once installed, CICEE is accessible by name, with the `cicee` command.

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee --help
cicee:
  cicee

Usage:
  cicee [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  env         Commands which interact with the current environment.
  exec        Execute a command in a containerized execution environment.
  init        Initialize project. Creates suggested cicee files.
  lib         Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source "$(cicee lib --shell bash)"'.
  template    Commands working with project continuous integration templates.
```

## Commands

* `env` - Commands which interact with the current environment.
  * [`display`][env-display] - Display the project's CI environment variables with current values.
  * [`require`][env-require] - Require that the environment contains all required variables.
* [`exec`][execute] - [Execute a command][execute] in a containerized execution environment.
* [`init`][initialize] - [Initialize project CI containerization files][initialize]. Creates suggested cicee files.
* [`lib`][lib] - Commands working with [CICEE shell script library][lib].
  * [`exec`][lib-exec] - Execute a CI Command in the local environment.
* `meta` - Commands working with project metadata.
  * `cienv` (`cienvironment`) - Commands working with the CI environment in project metadata.
    * [`var`][meta-cienv-variables] ([`variables`][meta-cienv-variables]) - Commands working with the CI environment variables in project metadata.
      * [`add`][meta-cienv-variables] - Add a new CI environment variable.
      * [`ls`][meta-cienv-variables] ([`list`][meta-cienv-variables]) - List the CI environment variables.
      * [`rm`][meta-cienv-variables] ([`remove`][meta-cienv-variables]) - Remove a CI environment variable.
      * [`update`][meta-cienv-variables] - Update a CI environment variable.
  * [`version`][meta-version] - Gets `.version` in project metadata.
    * [`bump`][meta-version-bump] - Bumps the `.version` in project metadata by a SemVer increment.
* `template` - Commands working with project continuous integration templates.
  * [`init`][template-init] - Initialize a project repository with continuous integration workflow scripts.
  * [`lib`][template-lib] - Initialize project CI with CICEE execution library. Supports `cicee exec`-like behavior without CICEE installation.

[env-display]: ./env-display.md
[env-require]: ./env-require.md
[execute]: ./execute.md
[initialize]: ./initialize.md
[lib-exec]: ./lib-exec.md
[lib]: ./ci-library.md
[meta-cienv-variables]: ./meta-cienv-variables.md
[meta-version-bump]: ./meta-version-bump.md
[meta-version]: ./meta-version.md
[template-init]: ./template-init.md
[template-lib]: ./template-lib.md
