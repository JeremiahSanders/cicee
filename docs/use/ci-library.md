# Continuous Integration Action Library

CICEE includes a library of shell functions which provide patterned executions of various common continuous integration actions. E.g., "dotnet build", "docker push". By sourcing this function library in a continuous integration script, project continuous integration workflows can be templated for quick intialization.

## To Import the Continuous Integration Action Library

CICEE exposes a `lib` command to output the importable library script path for supported shells. The `lib` command includes subcommands for each supported shell.

| Shell  | Subcommand | Example command to import library |
| ------ | ---------- | --------------------------------- |
| `bash` | `bash`     | `source "$(cicee lib bash)"`      |

## Using the Continuous Integration Action Library

Once the action library is imported, the session or script has access to the continuous integration functions exposed.

### Core Actions

* `ci-env-display` - Display initialized CI environment.
* `ci-env-init` - Initialize CI environment. **ALL OTHER ACTIONS ASSUME THIS ENVIRONMENT.**
* `ci-env-require` - Validates that the CI environment is initialized. Exits with `1` if validation fails.
* `ci-env-reset` - Unsets predenfined environment variables set by ci-env-init. Does not reset project or local variables.
