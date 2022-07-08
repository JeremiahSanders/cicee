# Execute a CI Command in the Local Environment

This command is an entrypoint for executing CI commands. It differs from [`exec`][exec] in that `lib exec` executes a command in the _local_ environment, while [`exec`][exec] executes a command within a predictable, containerized environment. Additionally, `lib exec` specifically loads the CICEE [CI library][], and any actions (in `ci/libexec/actions/`) and workflows (in `ci/libexec/workflows/`) in the project directory.

```bash
$ cicee lib exec --help
Description:
  Execute a command within a shell which has the CICEE CI shell library and is initialized for the project
  environment.

Usage:
  cicee lib exec [options]

Options:
  -s, --shell <Bash> (REQUIRED)                 Shell template. [default: Bash]
  -c, --command <command> (REQUIRED)            Shell command
  -p, --project-root <project-root> (REQUIRED)  Project repository root directory [default:
                                                $(pwd)]
  -m, --metadata <metadata> (REQUIRED)          Project metadata file path. [default:
                                                $(pwd)/project-metadata.json]
  -?, -h, --help                                Show help and usage information
```

[exec]: ./execute.md
[CI library]: ./ci-library.md
