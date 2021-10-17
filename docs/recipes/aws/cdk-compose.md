# Compose Workflow for AWS CDK Application

## Prerequisites

* AWS CDK application defining infrastructure. (Assumed to be at `${PROJECT_ROOT}/infrastructure/`.)
* Required environment variables:
  * `AWS_ACCOUNT` - AWS account.
  * `AWS_REGION` - AWS region, e.g., `us-east-1`.

## High-level CI Workflow Composition

The `compose` workflow needs to:

* Build the AWS CDK cloud assembly with `cdk synth`.

## CI Environment

The following [`ciEnvironment` configuration][opinionated project structure] is expected with this recipe.

```json
{
  "ciEnvironment": {
    "variables": [
      {
        "name": "AWS_ACCOUNT",
        "description": "AWS account ID, e.g., '9876543210987'",
        "required": true,
        "secret": false,
        "defaultValue": null
      },
      {
        "name": "AWS_REGION",
        "description": "AWS Region, e.g., 'us-east-1'",
        "required": true,
        "secret": false,
        "defaultValue": null
      }
    ]
  }
}
```

## Bash CI Workflow Compositions

> Build output:
>
> * _Project root_`/build/dist/cloud-assembly/` - AWS CDK cloud assembly.

### Using [Shell Library][shell library] Actions

```bash
#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  ci-aws-cdk-synth \
    --context "AwsAccount=${AWS_ACCOUNT}" \
    --context "AwsRegion=${AWS_REGION}"
}
```

### Using [Shell Library][shell library] Environment Variables

> This example is functionally equivalent to the "shell library actions" version above.

```bash
#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  cd "${PROJECT_ROOT}" &&
    cdk synth --output "${BUILD_PACKAGED_DIST}/cloud-assembly" \
      --context "AwsAccount=${AWS_ACCOUNT}" \
      --context "AwsRegion=${AWS_REGION}" &&
    cd -
}
```

[opinionated project structure]: ../../use/project-structure.md
[shell library]: ../../use/ci-library.md
