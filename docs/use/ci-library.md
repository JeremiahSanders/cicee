# Continuous Integration Action Library

CICEE includes a library of shell functions which provide patterned executions of various common continuous integration actions. E.g., "dotnet build", "docker push". By sourcing this function library in a continuous integration script, project continuous integration workflows can be templated for quick intialization.

```bash
$ cicee lib --help
lib:
  Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source "$(cicee lib --shell bash)"'.

Usage:
  cicee lib [options]

Options:
  -s, --shell <bash>    Shell template.
  -?, -h, --help        Show help and usage information
```

> Currently, Bash is the only supported shell. The `--shell` option is not required. When not provided, `--shell` defaults to `bash`.

## To Import the Continuous Integration Action Library

CICEE exposes a `lib` command to output the importable library script path, for supported shells. The `lib` command provides a `--shell` (short `-a`) option to request a specific template.

| Shell  | `--shell` Value | Example command to import library |
| ------ | --------------- | --------------------------------- |
| `bash` | `bash`          | `source "$(cicee lib)"`           |

## Using the Continuous Integration Action Library

Once the action library is imported, the session or script has access to the continuous integration functions exposed.

### Core Actions

* `ci-env-display` - Display initialized CI environment.
* `ci-env-init` - Initialize CI environment. **ALL OTHER ACTIONS ASSUME THIS ENVIRONMENT.**
* `ci-env-require` - Validates that the CI environment is initialized. Exits with `1` if validation fails.
* `ci-env-reset` - Unsets predenfined environment variables set by ci-env-init. Does not reset project or local variables.
