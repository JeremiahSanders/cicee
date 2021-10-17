# Validate Workflow for `create-react-app` SPA

## Prerequisites

* [`create-react-app`][create-react-app] application. (The recipe assumes the default project configuration, e.g., `package.json` at project root, application code in `src/`.)

## High-level CI Workflow Composition

The `validate` workflow needs to:

* Ensure the project(s) can `npm run build`.
* Ensure the project(s) successfully pass `npm run test`.

## Bash CI Workflow Compositions

### Using [Shell Library][shell library] Environment Variables

```bash
#--
# Restore dependencies.
#--
__restoreDependencies() {
  if [[ "${RELEASE_ENVIRONMENT:-false}" = true ]]; then
    npm ci
  else
    npm install
  fi
}

#--
# Validate the project's source, e.g. run tests, linting.
#--
ci-validate() {
  # NOTE: The 'npm run build' command below uses BUILD_PATH to output to the canonical CICEE location for unpackaged distributables.
  #   Use of BUILD_PATH with react-scripts build (triggered by npm run build) requires react-scripts 4.0.2 (2021-02-03) or later.
  #   When using earlier versions of react-scripts, you may need to add '&& mv "${BUILD_ROOT}" "${BUILD_UNPACKAGED_DIST}"' _after npm run build_ to achieve the same effect.
  __restoreDependencies &&
    BUILD_PATH="${BUILD_UNPACKAGED_DIST}" npm run build &&
    CI=true npm run test
}
```

[create-react-app]: https://create-react-app.dev/
[shell library]: ../../use/ci-library.md
