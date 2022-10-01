# Installation or Update

CICEE is distributed as a .NET tool, via [NuGet][].

When CICEE is used as a _global tool_, it enables you to run `cicee exec` outside the context of project repositories which use the CICEE [CI library][].

When CICEE is used as a _local tool_, it enables you to use the CICEE [continuous integration workflow execution patterns][template init] in a project repository.

## Installing CICEE as a Global Tool (for [`cicee exec`][exec])

To install:

```bash
$ dotnet tool install --global cicee
You can invoke the tool using the following command: cicee
Tool 'cicee' (version '1.4.0') was successfully installed.
```

To update / upgrade:

```bash
$ dotnet tool update --global cicee
Tool 'cicee' was successfully updated from version '1.2.0' to version '1.4.0'.
```

## Installing as a Local Tool (for [`dotnet cicee lib exec`][lib exec])

Two steps are required when installing CICEE as a local tool:

1. Create a .NET local tool manifest file at the project repository.
2. Install `cicee` as a local tool.

> All commands below are assumed to be executed from the project repository root (referred to as "`${PROJECT_ROOT}`" within the documentation and [CI library][]).

### Create .NET local tool manifest file

```bash
$ dotnet new tool-manifest
The template "Dotnet local tool manifest file" was created successfully.
```

### Install `cicee` as a local tool

To install:

```bash
$ dotnet tool install --local cicee
You can invoke the tool from this directory using the following commands: 'dotnet tool run cicee' or 'dotnet cicee'.
Tool 'cicee' (version '1.4.0') was successfully installed. Entry is added to the manifest file 
${PROJECT_ROOT}/.config/dotnet-tools.json.
```

To update / upgrade:

```bash
$ dotnet tool update --local cicee
Tool 'cicee' was successfully updated from version '1.2.0' to version '1.4.0'.
```

[CI library]: ./ci-library.md
[exec]: execute.md
[lib exec]: lib-exec.md
[NuGet]: https://www.nuget.org/packages/cicee/
[template init]: ./template-init.md
