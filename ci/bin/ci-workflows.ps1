#!/usr/bin/env pwsh

###
# Project CI Workflow Composition Library.
#   Contains functions which execute the project's high-level continuous integration tasks.
#
# How to use:
#   Update the "workflow compositions" in this file to perform each of the named continuous integration tasks.
#   Add additional workflow functions as needed. Note: Functions must be executed
###

# Context
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDirectory = Split-Path $scriptPath -Parent
$ciDirectory = Split-Path $scriptDirectory -Parent
$projectRoot = Split-Path $ciDirectory -Parent

# Load the CI action library.
$powershellModulePath = $(dotnet run --project $(Join-Path $projectRoot "src") --framework "net6.0" -- lib --shell pwsh)
Import-Module $powershellModulePath

####
#-- BEGIN Workflow Compositions
#     These commands are executed by CI entrypoint scripts, e.g., publish.sh.
#     By convention, each CI workflow function begins with "ci-".
####

#--
# Validate the project's source, e.g. run tests, linting.
#--
function Invoke-CiValidate {
  Invoke-CiDotnetRestore && `
    Invoke-CiDotnetBuild && `
    Invoke-CiDotnetTest
}

#--
# Compose the project's artifacts, e.g., compiled binaries, Docker images.
#--
function Invoke-CiCompose {
  Invoke-CiDotnetPublish --framework "net6.0" && `
    Invoke-CiDotnetPack
}

#--
# Publish the project's artifact composition.
#--
function Invoke-CiPublish {
  Invoke-CiDotnetNugetPush
}

####
#-- END Workflow Compositions
####
