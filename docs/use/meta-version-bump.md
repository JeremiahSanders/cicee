# Increment (Bump) Version in Project Metadata

Bumps the `.version` property in a [project metadata][project-structure] file by a SemVer increment: `major` (breaking changes), `minor` (non-breaking enhancements), or `patch` (fixes).

> Project metadata JSON files are _modified_, unless `dry-run` option is enabled. This may reformat the document.

For example:

> Given a version of `2.3.1`: a `patch` bump would be `2.3.2`, a `minor` bump would be `2.4.0`, and a `major` bump would be `3.0.0`.

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee meta version bump --help
Description:
  Increments version in project metadata.

Usage:
  cicee meta version bump [options]

Options:
  -m, --metadata <metadata> (REQUIRED)            Project metadata file path. [default:
                                                  $(pwd)/.project-metadata.json]
  -d, --dry-run                                   Execute a 'dry run', i.e., skip writing files and similar destructive
                                                  steps. [default: False]
  -i, --increment <Major|Minor|Patch> (REQUIRED)  SemVer increment by which to modify version. E.g., 'Minor' would bump
                                                  2.3.1 to 2.4.0. [default: Minor]
  -?, -h, --help                                  Show help and usage information
```

[project-structure]: ./project-structure.md
