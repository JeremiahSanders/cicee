# Set Version in Project Metadata

Sets the `.version` property in a [project metadata][project-structure] file to a SemVer version (e.g., `2.3.1`).

> Project metadata JSON files are _modified_, unless `dry-run` option is enabled. This may reformat the document.

For example:

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee meta version set --help
Description:
  Sets version in project metadata.

Usage:
  cicee meta version set [options]

Options:
  -m, --metadata <metadata> (REQUIRED)  Project metadata file path. [default: $(pwd)/.project-metadata.json]
  -d, --dry-run                         Execute a 'dry run', i.e., skip writing files and similar destructive steps. [default: False]
  -v, --version <version> (REQUIRED)    New version in SemVer 2.0 release format. E.g., '2.3.1'. []
  -?, -h, --help                        Show help and usage information
```

[project-structure]: ./project-structure.md
