# Compose Workflow for Zipping Build Artifacts

## Prerequisites

* `zip` command-line application.

> When using the default CI environment `Dockerfile` initialized by CICEE, `zip` is already installed.

## High-level CI Workflow Composition

The `compose` workflow needs to:

* Compress the build artifacts for distributability.

## Bash CI Workflow Compositions

> Build output:
>
> * _Project root_`/build/dist/compressed/` - Compressed zip archives.

### Using [Shell Library][shell library] Actions

> In the example below, we assume the contents of `${BUILD_UNPACKAGED_DIST}` (_Project root_`/build/app`) are the desired artifacts. The compressed contents are stored at _Project root_`/build/dist/compressed/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.zip`. Both of those paths may be customized as needed.

```bash
#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  # We cd into the build/dist/ directory first to reduce the parent directories included in the zip archive.
  # The zip command below uses the following options:
  #   -T Test zip file integrity.
  #   -r Recurse into directories.
  if [[ ! -d "${BUILD_PACKAGED_DIST}/compressed" ]]; then
    mkdir "${BUILD_PACKAGED_DIST}/compressed"
  fi
  cd "${BUILD_PACKAGED_DIST}" &&
    zip -T -r "${BUILD_PACKAGED_DIST}/compressed/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.zip" "${BUILD_UNPACKAGED_DIST}" &&
    cd -
}
```

[shell library]: ../../use/ci-library.md
