# Validate Workflow for .NET

This validation workflow pattern should work all types of .NET projects which use the `dotnet` CLI.

## Prerequisites

* .NET solution file (i.e., `*.sln`), saved in the project root directory.
* .NET project (e.g., `*.csproj`), referenced by the .NET solution.

## High-level CI Workflow Composition

The `validate` workflow needs to:

* Ensure the project(s) can `dotnet build`.
* Ensure the project(s) successfully pass `dotnet test`.

## Bash CI Workflow Compositions

> Build output:
>
> * _Project root_`/build/`
>   * `app/` - Output of `dotnet publish`.
>   * `dist/`
>     * `nuget/` - NuGet package(s) generated by `dotnet pack`.
>   * `docs/` - Generated XML documentation.

### Using [Shell Library][shell library] Actions

> The CI [shell library][] functions used below assume that the project is structured:
>
> * _Project root_`/` - Project root directory.
>   * `*.sln` - .NET solution file (e.g., `MyAwesomeProject.sln`).

```bash
#--
# Validate the project's source, e.g. run tests, linting.
#--
ci-validate() {
  ci-dotnet-restore &&
    ci-dotnet-build &&
    ci-dotnet-test
}
```

### Using [Shell Library][shell library] Environment Variables

> This example is functionally equivalent to the "shell library actions" version above.

```bash
#--
# Validate the project's source, e.g. run tests, linting.
#--
ci-validate() {
  dotnet restore "${PROJECT_ROOT}" &&
    dotnet build "${PROJECT_ROOT}" \
    -p:GenerateDocumentationFile=true \
    -p:Version="${PROJECT_VERSION_DIST}" &&
    dotnet test "${PROJECT_ROOT}"
}
```

[shell library]: ../../use/ci-library.md
