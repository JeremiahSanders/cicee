# Get Version in Project Metadata

Gets the `.version` property in a [project metadata][project-structure] file.

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee meta version --help
Description:
  Gets version in project metadata.

Usage:
  cicee meta version [options]

Options:
  -m, --metadata <metadata> (REQUIRED)  Project metadata file path. [default: $(pwd)/project-metadata.json]
  -?, -h, --help                        Show help and usage information
```

[project-structure]: ./project-structure.md
