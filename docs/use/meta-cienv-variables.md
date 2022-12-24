# Interacting with CI Environment Variables in Project Metadata

The [CICEE CI shell library][ci-library], and CICEE environment commands (e.g., [`env display`][], [`env require`][]), depend upon CI environment variable configuration in [project metadata][]. The `meta cienv var` commands allow that configuration to be maintained via API.

## List CI Environment Variables (`ls`, `list`)

```shell
$ cicee meta cienv var ls --help
Description:
  Lists the project's CI environment variables.

Usage:
  cicee meta cienvironment variables list [options]

Options:
  -m, --metadata <metadata> (REQUIRED)  Project metadata file path. [default: $(pwd)/.project-metadata.json]
  -n, --name <name> (REQUIRED)          Environment variable name 'contains' filter, case insensitive. []
  -?, -h, --help                        Show help and usage information
```

## Add CI Environment Variable (`add`)

Use `add` to add new CI environment variables. If a `defaultValue` is provided, the [CICEE CI shell library][ci-library] will attempt to apply those default values when using [`cicee lib exec`][lib-exec].

> It is recommended to use `SCREAMING_SNAKE_CASE` formatting for variable names, matching CICEE's shell library conventions. E.g., `AZURE_DOMAIN`, `TARGET_AWS_S3_BUCKET`.
>
> **Safety Consideration:** Environment variables' `defaultValue` should **never** contain private or secret information, as it is expected to be committed to source control. When a variable is `secret`, either omit a `defaultValue`, or provide a known placeholder value which will fail predictably. E.g., a `GITHUB_ACCESS_TOKEN` environment variable might have the default value `access-token-was-not-specified`.

```shell
$ cicee meta cienv var add --help
Description:
  Adds a new project CI environment variable.

Usage:
  cicee meta cienvironment variables add [options]

Options:
  -m, --metadata <metadata> (REQUIRED)          Project metadata file path. [default: $(pwd)/.project-metadata.json]
  -n, --name <name> (REQUIRED)                  Environment variable name. []
  -d, --description <description>               Environment variable description. []
  -r, --required                                Is this environment variable required? [default: False]
  -s, --secret                                  Is this environment variable secret? [default: False]
  -v, --default, --defaultValue <defaultValue>  Default environment variable value. []
  -?, -h, --help                                Show help and usage information
```

## Update CI Environment Variable (`update`)

Use `update` to modify an existing project CI environment variable. Option values which are not specified are not modified. E.g., if the variable is currently required and `update` is used _without_ `--required`, the variable continues to be required.

> **Safety Consideration:** Environment variables' `defaultValue` should **never** contain private or secret information, as it is expected to be committed to source control. When a variable is `secret`, either omit a `defaultValue`, or provide a known placeholder value which will fail predictably. E.g., a `GITHUB_ACCESS_TOKEN` environment variable might have the default value `access-token-was-not-specified`.

```shell
$ cicee meta cienv var update --help
Description:
  Modifies an existing project CI environment variable.

Usage:
  cicee meta cienvironment variables update [options]

Options:
  -m, --metadata <metadata> (REQUIRED)          Project metadata file path. [default: $(pwd)/.project-metadata.json]
  -n, --name <name> (REQUIRED)                  Environment variable name. []
  -d, --description <description>               Environment variable description. []
  -r, --required                                Is this environment variable required? []
  -s, --secret                                  Is this environment variable secret? []
  -v, --default, --defaultValue <defaultValue>  Default environment variable value. []
  -?, -h, --help                                Show help and usage information
```

## Remove CI Environment Variable (`rm`, `remove`)

Use `remove` to remove a CI environment variable.

```shell
$ cicee meta cienv var rm --help    
Description:
  Removes a project CI environment variable.

Usage:
  cicee meta cienvironment variables remove [options]

Options:
  -m, --metadata <metadata> (REQUIRED)  Project metadata file path. [default: $(pwd)/.project-metadata.json]
  -n, --name <name> (REQUIRED)          Environment variable name. []
  -?, -h, --help                        Show help and usage information
```

[`env display`]: ./env-display.md
[`env require`]: ./env-require.md
[ci-library]: ./ci-library.md
[lib-exec]: ./lib-exec.md
[project metadata]: ./project-structure.md
