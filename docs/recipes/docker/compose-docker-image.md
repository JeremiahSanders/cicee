# Compose Workflow for Building a Docker Image

## Prerequisites

* `Dockerfile`, saved in the project root directory.

## High-level CI Workflow Compositions

The `compose` workflow needs to:

* Build the Docker image.

## Bash CI Workflow Compositions

### Using [Shell Library][shell library] Actions

```bash
#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  ci-dotnet-publish &&
    ci-docker-build
}
```

### Using [Shell Library][shell library] Environment Variables

> This example is functionally equivalent to the "shell library actions" version above.
>
> This example uses a few [shell library][] actions (i.e., shell functions), e.g., `require-var`.

```bash
__tryApplyFallbackDockerEnvironment() {
  require-var "PROJECT_VERSION_DIST" "PROJECT_NAME"
  DOCKER_IMAGE_TAG="${DOCKER_IMAGE_TAG:-${PROJECT_VERSION_DIST}}"
  DOCKER_IMAGE_REPOSITORY="${DOCKER_IMAGE_REPOSITORY:-${PROJECT_NAME}}"
  DOCKER_IMAGE="${DOCKER_IMAGE_REPOSITORY}:${DOCKER_IMAGE_TAG}"
}

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  __tryApplyFallbackDockerEnvironment &&
    require-var "DOCKER_IMAGE" &&
    docker build \
      --rm \
      --tag "${DOCKER_IMAGE}" \
      -f "${PROJECT_ROOT}/Dockerfile" \
      "${PROJECT_ROOT}"
}
```

[shell library]: ../../use/ci-library.md
