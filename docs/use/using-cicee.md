# Using CICEE

Once installed, CICEE is accessible by name, with the `cicee` command.

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
  * [`require`][env-require] - Require that the environment contains all required variables.
* [`exec`][execute] - [Execute a command][execute] in a containerized execution environment.
* [`init`][initialize] - [Initialize project CI containerization files][initialize]. Creates suggested cicee files.
* [`lib`][lib] - Commands working with [CICEE shell script library][lib].
* `meta` - Commands working with project metadata.
  * [`version`][meta-version] - Gets `.version` in project metadata.
    * [`bump`][meta-version-bump] - Bumps the `.version` in project metadata by a SemVer increment.
* `template` - Commands working with project [continuous integration templates][template].
  * [`init`][template-init] - Initialize a project repository with continuous integration workflow scripts.
  * [`lib`][template-lib] - Initialize project CI with CICEE execution library. Supports `cicee exec`-like behavior without CICEE installation.

[docker-compose-command]: https://docs.docker.com/compose/compose-file/compose-file-v3/#command
[docker-compose-entrypoint]: https://docs.docker.com/compose/compose-file/compose-file-v3/#entrypoint
[env-require]: ./env-require.md
[execute]: ./execute.md
[initialize]: ./initialize.md
[lib]: ./ci-library.md
[meta-version]: ./meta-version.md
[meta-version-bump]: ./meta-version-bump.md
[project's docker-compose file]: ../use/project-structure.md
[template-init]: ./template-init.md
[template-lib]: ./template-lib.md
