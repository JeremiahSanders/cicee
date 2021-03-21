# Using CICEE

Once installed, CICEE is accessible by name, with the `cicee` command.

```bash
$ cicee

-- cicee (v0.4.0) --

Required command was not provided.

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
  lib         Commands working with CICEE shell script library.
  template    Commands working with project continuous integration templates.
```

## Commands

* `env` - Commands which interact with the current environment.
* [`exec`][execute] - [Execute a command][execute] in a containerized execution environment.
* [`init`][initialize] - [Initialize project CI containerization files][initialize]. Creates suggested cicee files.
* [`lib`][lib] - Commands working with [CICEE shell script library][lib].
* [`template`][template] - Commands working with project [continuous integration templates][template].

[docker-compose-command]: https://docs.docker.com/compose/compose-file/compose-file-v3/#command
[docker-compose-entrypoint]: https://docs.docker.com/compose/compose-file/compose-file-v3/#entrypoint
[execute]: ./execute.md
[initialize]: ./initialize.md
[lib]: ./ci-library.md
[project's docker-compose file]: ../use/project-structure.md
[template]: ./template.md
