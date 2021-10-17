# Customizing Project CI Environment to `docker login` to AWS ECR

This recipe uses the project CI environment initialization script to `docker login` to [AWS ECR][] during CI workflow initialization.

## Goal

The goal of this recipe is to ensure the CI workflows have authorization to interact with [AWS ECR][].

## Possible Reasons / Use Cases

Common use cases are:

* Enable the project's `ci/Dockerfile` to use a _private_ base image hosted in AWS ECR.
* Enable the `compose` and `publish` CI workflows to build and push images with implicit authentication.

## Example

> ### Prerequisite
>
> Create a project CI environment script, saved to `${PROJECT_ROOT}/ci/env.project.sh`. This script should be committed to source control.

### Project Environment `env.project.sh` Script Using [Shell Library][shell library] Actions

```bash
#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

ci-aws-ecr-docker-login
```

### Project Environment `env.project.sh` Script Using `aws` and `docker`

```bash
#!/usr/bin/env bash

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

aws ecr get-login-password --region "${AWS_REGION}" |
  docker login --username AWS --password-stdin "${AWS_ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com"
```

[AWS ECR]: https://aws.amazon.com/ecr/
[shell library]: ../../../use/ci-library.md
