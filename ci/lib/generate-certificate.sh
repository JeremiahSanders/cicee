#!/usr/bin/env bash

###
# Generates a certificate using 'dotnet dev-certs'.
#   (CICEE v1.1.0)
#   Assumes CI environment (execution of ci/lib/ci-env-load.sh)
###

if [[ $# -eq 0 ]];
  then
    printf "No directory provided.\nProvide one string argument to be used as destination directory.\n"
    exit 1
fi

if [[ -f "$1" ]];
  then
    printf "Directory must be provided."
    exit 1
fi

if [[ ! -d "$1" ]];
  then
    mkdir "$1"
fi

# Hash the current date, convert it to base64, and take the first 32 characters.
RANDOM_PASSWORD="$(date +%s | sha256sum | base64 | head -c 32)"
dotnet dev-certs https --export-path "$1/certificate.pfx" -p "$RANDOM_PASSWORD" \
&& printf "%s" "$RANDOM_PASSWORD" > "$1/password.txt" \
&& printf "Generated certificate.\nCertificate: %s\nPassword: %s" "$1/certificate.pfx" "$1/password.txt"
