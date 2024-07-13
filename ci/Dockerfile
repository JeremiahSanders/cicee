FROM gnosian/ci-env-dotnet AS build-environment

# Add exception for known `cicee exec` mount directory. Without this, GitHub Actions fails to resolve current commit SHA (which defaults to "0000000").
RUN git config --global --add safe.directory /code

# .NET identifiers
ARG DOTNET_7_VERSION=7.0.410
ARG DOTNET_7_SHA_AMD64=20b8e02979328e4c4a14493f7791ed419aabd0175233db80cd60e2c004b829b3e8301281ea86b27ba818372473accf5a6d553e5354c54917c8e84d25f5855caa
ARG DOTNET_7_SHA_ARM64=2db6a3b9a532d2f59a2b459e634206913a9585c821f3f578a421e3bae346a92dd9b85b76ebde343ca3057275f7ec4d0bca71cbb7f2badb6dcdb516244e84da46

# Install .NET 7 SDK
#   See: https://github.com/dotnet/dotnet-docker/blob/4a40f7eeecad2a3f15541fbb84962f8789d23cb0/src/sdk/7.0/bullseye-slim/amd64/Dockerfile
#        https://github.com/dotnet/dotnet-docker/blob/4a40f7eeecad2a3f15541fbb84962f8789d23cb0/src/sdk/7.0/bullseye-slim/arm64v8/Dockerfile
RUN if [ "$(dpkg --print-architecture)" = "arm64" ]; then \
  curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/${DOTNET_7_VERSION}/dotnet-sdk-${DOTNET_7_VERSION}-linux-arm64.tar.gz \
  && echo "${DOTNET_7_SHA_ARM64} dotnet.tar.gz" | sha512sum -c - ; \
  else \
  curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/${DOTNET_7_VERSION}/dotnet-sdk-${DOTNET_7_VERSION}-linux-x64.tar.gz \
  && echo "${DOTNET_7_SHA_AMD64} dotnet.tar.gz" | sha512sum -c - ; \
  fi \
  && mkdir -p /usr/share/dotnet \
  && tar -oxzf dotnet.tar.gz -C /usr/share/dotnet ./packs ./sdk ./sdk-manifests ./templates ./LICENSE.txt ./ThirdPartyNotices.txt \
  && rm dotnet.tar.gz \
  # Trigger first run experience by running arbitrary cmd
  && dotnet help
