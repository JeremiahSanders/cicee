# Compose Workflow for `create-react-app` SPA

## Prerequisites

* `create-react-app` application. (The recipe assumes the default project configuration, e.g., `package.json` at project root, application code in `src/`.)

## High-level CI Workflow Composition

The `compose` workflow needs to:

* Build the React application with `npm run build`.

## Bash CI Workflow Compositions

> Build output:
>
> * _Project root_`/build/app/` - Output of `npm run build`.

### Using [Shell Library][shell library] Actions

```bash
__restoreDependencies() {
  if [[ "${RELEASE_ENVIRONMENT:-false}" = true ]]; then
    npm ci
  else
    npm install
  fi
}

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  # NOTE: The 'npm run build' command below uses BUILD_PATH to output to the canonical CICEE location for unpackaged distributables.
  #   Use of BUILD_PATH with react-scripts build (triggered by npm run build) requires react-scripts 4.0.2 (2021-02-03) or later.
  #   When using earlier versions of react-scripts, you may need to add '&& mv "${BUILD_ROOT}" "${BUILD_UNPACKAGED_DIST}"' _after npm run build_ to achieve the same effect.
  __restoreDependencies &&
    BUILD_PATH="${BUILD_UNPACKAGED_DIST}" npm run build
}
```

[opinionated project structure]: ../../use/project-structure.md
[shell library]: ../../use/ci-library.md
