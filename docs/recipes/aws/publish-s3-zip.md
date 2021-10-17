# Publish Workflow for Copying Zip Archives to AWS S3

This type of application outputs an Amazon Web Services (AWS) Cloud Development Kit (CDK) cloud assembly. The cloud assembly, when deployed, hosts a single-page application (SPA) created with `create-react-app` using Node.js.

## Prerequisites

* `aws` CLI
  * The `aws` CLI implicitly requires authorization for S3 actions. Configuring `aws` credentials is out of scope for this recipe.
* Required environment variables:
  * `S3_PUBLISH_BUCKET` - AWS S3 Bucket to which artifacts will be copied.

## High-level CI Workflow Composition

The `publish` workflow needs to:

* Copy the compressed artifacts to AWS S3 for archival.

## CI Environment

The following [`ciEnvironment` configuration][opinionated project structure] is expected with this recipe.

```json
{
  "ciEnvironment": {
    "variables": [
      {
        "name": "S3_PUBLISH_BUCKET_SUFFIX",
        "description": "S3 Published Artifact Bucket Suffix (combines with S3_PUBLISH_BUCKET to create S3 object prefix)",
        "required": false,
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

### Using [Shell Library][shell library] Actions

> This example assumes that the zip artifact which should be copied is located at _project root_`/build/dist/compressed/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.zip`. Update the source path as needed.

```bash
#--
# Publish the project's artifact composition.
#--
ci-publish() {
  aws s3 cp \
    "${BUILD_PACKAGED_DIST}/compressed/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.zip" \
    "s3://${S3_PUBLISH_BUCKET}/${S3_PUBLISH_BUCKET_SUFFIX:-${PROJECT_NAME}}/${PROJECT_NAME}-${PROJECT_VERSION_DIST}.zip"
}
```

[opinionated project structure]: ../../use/project-structure.md
[shell library]: ../../use/ci-library.md
