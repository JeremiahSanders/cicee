# Publish Workflow for Pushing Docker Images to Docker Hub

## Prerequisites

* Required environment variables:
  * `DOCKER_IMAGE_REPOSITORY` - Docker image repository.
  * `DOCKER_USERNAME` - [Docker Hub][] username.
  * `DOCKER_PASSWORD` - [Docker Hub][] API token.

## High-level CI Workflow Composition

The `publish` workflow needs to:

* Push the Docker image to the Docker repository.

## CI Environment

The following [`ciEnvironment` configuration][opinionated project structure] is expected with this recipe.

```json
{
  "ciEnvironment": {
    "variables": [
      {
        "name": "DOCKER_IMAGE_REPOSITORY",
        "description": "Docker image repository",
        "required": true,
        "secret": false,
        "defaultValue": null
      },
      {
        "name": "DOCKER_USERNAME",
        "description": "Docker username",
        "required": true,
        "secret": false,
        "defaultValue": null
      },
      {
        "name": "DOCKER_PASSWORD",
        "description": "Docker password or API token",
        "required": true,
        "secret": true,
        "defaultValue": null
      }
    ]
  }
}
```

## Bash CI Workflow Compositions

In this example, images are pushed to the _default_ server, as configured in the Docker daemon. I.e., [Docker Hub][], most commonly.

### Using [Shell Library][shell library] Actions

```bash
#--
# Publish the project's artifact composition.
#--
ci-publish() {
  __conditionallyPushLatest() {
    if [[ "${RELEASE_ENVIRONMENT:-false}" = true ]]; then
      docker push "${DOCKER_IMAGE_REPOSITORY}:latest"
    fi
  }

  docker login --username "${DOCKER_USERNAME}" --password "${DOCKER_PASSWORD}" &&
    ci-docker-push &&
    __conditionallyPushLatest
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
# Publish the project's artifact composition.
#--
ci-publish() {
  __conditionallyPushLatest() {
    if [[ "${RELEASE_ENVIRONMENT:-false}" = true ]]; then
      docker push "${DOCKER_IMAGE_REPOSITORY}:latest"
    fi
  }

  docker login --username "${DOCKER_USERNAME}" --password "${DOCKER_PASSWORD}" &&
    __tryApplyFallbackDockerEnvironment &&
    require-var "DOCKER_IMAGE" &&
    docker push "${DOCKER_IMAGE}" &&
    __conditionallyPushLatest
}
```

[Docker Hub]: https://hub.docker.com/
[opinionated project structure]: ../../use/project-structure.md
[shell library]: ../../use/ci-library.md
