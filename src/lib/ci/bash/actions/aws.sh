#!/usr/bin/env bash
# shellcheck disable=SC2155

####
# AWS continuous integration actions.
#   Contains "action" functions which interact with Amazon Web Services CLI (aws) and CDK CLI (cdk).
#
# Action library functions:
#   * _ci_CdkDeploy - Execute 'cdk deploy'.
#   * _ci_CdkSynth - Execute 'cdk synth'.
#   * _ci_DockerLoginAwsEcr - Authenticate with AWS ECR and use authorization to execute docker login.
#
# Required environment:
#   $AWS_ACCOUNT   - AWS account ID/number.
#   $AWS_REGION    - AWS region.
####

set -o errexit  # Fail or exit immediately if there is an error.
set -o nounset  # Fail if an unset variable is used.
set -o pipefail # Fail pipelines if any command errors, not just the last one.

###############################################################################
# BEGIN AWS CI workflow actions.
###

#-
# AWS CLI (aws)
#-

# Docker login to AWS ECR - requires environment $AWS_ACCOUNT and $AWS_REGION.
_ci_DockerLoginAwsEcr() {
  require-var "AWS_REGION" "AWS_ACCOUNT"
  aws ecr get-login-password --region "${AWS_REGION}" |
    docker login --username AWS --password-stdin "${AWS_ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com"
}

#-
# AWS Cloud Development Kit CLI (cdk)
# -

# AWS CDK deploy
_ci_CdkDeploy() {
  cdk deploy --app "${BUILD_PACKAGED_DIST}/cloud-assembly" "$@"
}

# AWS CDK synth
_ci_CdkSynth() {
  cd "${PROJECT_ROOT}" &&
    cdk synth --output "${BUILD_PACKAGED_DIST}/cloud-assembly" "$@" &&
    cd -
}

export -f _ci_CdkDeploy
export -f _ci_CdkSynth
export -f _ci_DockerLoginAwsEcr
