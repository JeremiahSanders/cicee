# Require Environment Contains Variables

Require that the environment contains all required variables. For example, require that a database `CONNECTION_STRING` is set.

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee env require --help
Description:
  Require that the environment contains all required variables.

Usage:
  cicee env require [options]

Options:
  -p, --project-root <project-root>      Project repository root directory [default:
                                         $(pwd)]
  -f, -m, --file, --metadata <metadata>  Project metadata file. [default:
                                         $(pwd)/.project-metadata.json]
  -?, -h, --help                         Show help and usage information
```

## Configuring Project CI Environment

See `project metadata` in [project structure][].

## Example

The following example shows a `project-metadata.json` for a .NET library published to nuget.org. In this example, the `NUGET_API_KEY` is required.

```json
{
    "ciEnvironment": {
        "variables": [
            {
                "description": "NuGet API Key",
                "name": "NUGET_API_KEY",
                "required": true,
                "secret": true
            }
        ]
    },
    "name": "neat-project-name",
    "title": "The NEAT Project",
    "version": "0.7.2"
}
```

When the required variables are not present:

```bash
$ cicee env require

-- cicee (v0.12.1) --

Environment validation failed.
  Reason: Missing environment variables: NUGET_API_KEY
```

When the required variables are present:

```bash
$ cicee env require

-- cicee (v0.12.1) --

Environment validation succeeded.
  Metadata file: /home/user/neat-project-name/project-metadata.json
```

[project structure]: ./project-structure.md
