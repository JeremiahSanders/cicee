FROM gnosian/ci-env-dotnet AS build-environment

# Add exception for known `cicee exec` mount directory. Without this, GitHub Actions fails to resolve current commit SHA (which defaults to "0000000").
RUN git config --global --add safe.directory /code
