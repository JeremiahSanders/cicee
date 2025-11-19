FROM gnosian/ci-env-dotnet AS build-environment

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS net-sdk-6

FROM build-environment AS with-net-6
COPY --from=net-sdk-6 /usr/share/dotnet /usr/share/dotnet

# Add exception for known `cicee exec` mount directory. Without this, GitHub Actions fails to resolve current commit SHA (which defaults to "0000000").
RUN git config --global --add safe.directory /code
