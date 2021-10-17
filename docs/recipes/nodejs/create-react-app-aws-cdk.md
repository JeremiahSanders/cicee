# AWS CDK Application Hosting `create-react-app` SPA

This type of application outputs an Amazon Web Services (AWS) Cloud Development Kit (CDK) cloud assembly. The cloud assembly, when deployed, hosts a single-page application (SPA) created with `create-react-app` using Node.js.

## Prerequisites

* `create-react-app` application. (The recipe assumes the default project configuration, e.g., `package.json` at project root, application code in `src/`.)
* AWS CDK application defining infrastructure. (Assumed to be at `infrastructure/`.)
* Required environment variables:
  * `AWS_ACCOUNT` - AWS account.
  * `AWS_REGION` - AWS region, e.g., `us-east-1`.

## High-level CI Workflow Compositions

The `validate` workflow needs to:

* Ensure the project(s) can `npm run build`.
* Ensure the project(s) successfully pass `npm run test`.

The `compose` workflow needs to:

* Build the React application with `npm run build`.
* Build the AWS CDK cloud assembly with `cdk synth`.
* Compress the AWS CDK cloud assembly for distributability.

The `publish` workflow needs to:

* Copy the compressed cloud assembly to AWS S3 for archival.

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
      },
      {
        "name": "S3_PUBLISH_BUCKET",
        "description": "S3 Published Artifact Bucket",
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
> * _Project root_`/build/`
>   * `app/` - Output of `npm run build`.
>   * `dist/`
>     * `cloud-assembly/` - AWS CDK cloud assembly.
>     * `compressed/` - Zip compressed AWS CDK cloud assembly.

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

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
ci-compose() {
  __zipCloudAssembly() {
    # We cd into the build/dist/ directory first to reduce the parent directories included in the zip archive.
    # The zip command below uses the following options:
    #   -T Test zip file integrity.
    #   -r Recurse into directories.
    if [[ ! -d "${BUILD_PACKAGED_DIST}/compressed" ]]; then
      mkdir "${BUILD_PACKAGED_DIST}/compressed"
    fi
    cd "${BUILD_PACKAGED_DIST}" &&
      zip -T -r "${BUILD_PACKAGED_DIST}/compressed/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.cloud-assembly.zip" "${BUILD_PACKAGED_DIST}/cloud-assembly" &&
      cd -
  }
  # NOTE: The 'npm run build' command below uses BUILD_PATH to output to the canonical CICEE location for unpackaged distributables.
  #   Use of BUILD_PATH with react-scripts build (triggered by npm run build) requires react-scripts 4.0.2 (2021-02-03) or later.
  #   When using earlier versions of react-scripts, you may need to add '&& mv "${BUILD_ROOT}" "${BUILD_UNPACKAGED_DIST}"' _after npm run build_ to achieve the same effect.
  # The zip command below uses the following options:
  #   -T Test zip file integrity.
  #   -r Recurse into directories.
  __restoreDependencies &&
    BUILD_PATH="${BUILD_UNPACKAGED_DIST}" npm run build &&
    ci-aws-cdk-synth \
      --context "AwsAccount=${AWS_ACCOUNT}" \
      --context "AwsRegion=${AWS_REGION}" &&
    __zipCloudAssembly
}

#--
# Publish the project's artifact composition.
#--
ci-publish() {
  __copyCloudAssemblyZipToS3() {
    aws s3 cp \
      "${BUILD_PACKAGED_DIST}/compressed/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.cloud-assembly.zip" \
      "s3://${S3_PUBLISH_BUCKET}/${PROJECT_NAME}/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.cloud-assembly.zip"
  }
  __copyCloudAssemblyZipToS3
}
```

[opinionated project structure]: ../../use/project-structure.md
[shell library]: ../../use/ci-library.md
