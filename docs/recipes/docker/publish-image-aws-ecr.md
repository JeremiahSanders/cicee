# Publish Workflow for Pushing Docker Images to AWS Elastic Container Registry (ECR)

## Prerequisites

* Required environment variables:
  * `AWS_ACCOUNT` - AWS account ID.
  * `AWS_REGION` - AWS region, e.g., `us-east-1`.
  * `DOCKER_IMAGE_REPOSITORY` - Docker image repository.

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
        "name": "AWS_ACCOUNT",
        "description": "AWS account, e.g., 890123456789",
        "required": true,
        "secret": false,
        "defaultValue": null
      },
      {
        "name": "AWS_REGION",
        "description": "AWS region, e.g., us-east-1",
        "required": true,
        "secret": false,
        "defaultValue": null
      },
      {
        "name": "DOCKER_IMAGE_REPOSITORY",
        "description": "Docker image repository",
        "required": true,
        "secret": false,
        "defaultValue": null
      }
    ]
  }
}
```

## Bash CI Workflow Compositions

In this example, images are pushed to [AWS ECR][].

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

  ci-aws-ecr-docker-login &&
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
  require-var "AWS_REGION" "AWS_ACCOUNT" &&
    aws ecr get-login-password --region "${AWS_REGION}" |
      docker login --username AWS --password-stdin "${AWS_ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com" &&
    __tryApplyFallbackDockerEnvironment &&
    require-var "DOCKER_IMAGE" &&
    docker push "${DOCKER_IMAGE}" &&
    __conditionallyPushLatest
}
```

[AWS ECR]: https://aws.amazon.com/ecr/
[opinionated project structure]: ../../use/project-structure.md
[shell library]: ../../use/ci-library.md
