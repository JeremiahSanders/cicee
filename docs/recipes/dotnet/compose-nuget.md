# Compose Workflow for .NET NuGet Packages

## High-level CI Workflow Composition

The `compose` workflow needs to:

* Create the NuGet package.

## Bash CI Workflow Compositions

> Build output:
>
> * _Project root_`/build/`
>   * `app/` - Output of `dotnet publish`.
>   * `dist/`
>     * `nuget/` - NuGet package(s) generated by `dotnet pack`.
>   * `docs/` - Generated XML documentation.

### Using [Shell Library][shell library] Actions

> The CI [shell library][] assumes that the project is structured:
>
> * _Project root_`/` - Project root directory.
>   * `*.sln` - .NET solution file (e.g., `MyAwesomeProject.sln`).
>   * `src/` - Directory containing the primary .NET project (e.g., `.csproj`, `.fsproj`).

```bash
#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  ci-dotnet-publish && ci-dotnet-pack
}
```

### Using [Shell Library][shell library] Environment Variables

> This example is functionally equivalent to the "shell library actions" version above.

```bash
#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  dotnet publish "${PROJECT_ROOT}/src" \
    --configuration Release \
    --output "${BUILD_UNPACKAGED_DIST}" \
    -p:DocumentationFile="${PROJECT_ROOT}/build/docs/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.xml" \
    -p:Version="${PROJECT_VERSION_DIST}" &&
    dotnet pack "${PROJECT_ROOT}/src" \
    --configuration Release \
    --output "${BUILD_PACKAGED_DIST}/nuget/" \
    -p:DocumentationFile="${BUILD_DOCS}/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.xml" \
    -p:PackageVersion="${PROJECT_VERSION_DIST}" \
    -p:Version="${PROJECT_VERSION_DIST}"
}
```

[shell library]: ../../use/ci-library.md
