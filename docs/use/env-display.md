# Display Project CI Environment

Display values of current project CI environment variables.

> **Note:** When CICEE is installed as a .NET local tool (i.e., your `${PROJECT_ROOT}/.config/dotnet-tools.json` contains a reference to `cicee`), all `$ cicee ..arguments..` commands become `$ dotnet cicee ..arguments..`. Additionally, you may need to run `dotnet tool restore`, to ensure the tool is installed.

```bash
$ cicee env display --help
Description:
  Display values of current project CI environment variables.

Usage:
  cicee env display [options]

Options:
  -m, --metadata <metadata> (REQUIRED)  Project metadata file path. [default: $(pwd)/.project-metadata.json]
  -?, -h, --help                        Show help and usage information
```
