#!/usr/bin/env bash

# shellcheck disable=SC2155

###
# Display the project's continuous integration environment.
#   (CICEE v1.1.0)
###

printf "Project            :  %s\n" "${PROJECT_NAME}"
printf "  Title            :  %s\n" "${PROJECT_TITLE}"
printf "  Version          :  %s\n" "${PROJECT_VERSION}"
printf "    .NET Assembly  :  %s\n" "${PROJECT_VERSION_DOTNET}"
printf "    Distribution   :  %s\n" "${PROJECT_VERSION_DIST}"
printf "  Root             :  %s\n\n" "${PROJECT_ROOT}"

printf "Git branch         :  %s\n" "${CURRENT_GIT_BRANCH}"
printf "Git hash           :  %s\n\n" "${CURRENT_GIT_HASH}"

printf "Docker image       :  %s\n" "${DOCKER_IMAGE}"
printf "  Repository       :  %s\n" "${DOCKER_IMAGE_REPOSITORY}"
printf "  Tag              :  %s\n\n" "${DOCKER_IMAGE_TAG}"
