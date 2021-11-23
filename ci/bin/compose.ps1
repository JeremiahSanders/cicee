#!/usr/bin/env pwsh

###
# Build the project's artifact composition.
#
# How to use:
#   Customize the `Invoke-CiCompose` workflow (function) defined in ci-workflows.ps1.
###

# Context
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDirectory = Split-Path $scriptPath -Parent
$ciDirectory = Split-Path $scriptDirectory -Parent
$projectRoot = Split-Path $ciDirectory -Parent

$ciceeCiPowershellModulePath = $(dotnet run --project "$(Join-Path $projectRoot "src")" --framework "net6.0" -- lib --shell pwsh)

# 1 - Load the CICEE CI action library (PowerShell module).
# 2 - Load the project CI workflow library.
# 3 - Initialize the CI environment.
# 4 - Display the CI environment, for logging.
# 5 - Assert that the CI environment is configured. (To fail early in cases of misconfiguration.)
# 6 - Execute `Invoke-CiCompose`, defined in `ci-workflows.ps1`.
Import-Module "${ciceeCiPowershellModulePath}" && `
  . $(Join-Path $scriptDirectory "ci-workflows.ps1") && `
  Initialize-CiEnv && `
  Show-CiEnv && `
  Assert-CiEnv && `
  Write-Output "`nBeginning artifact composition...`n" && `
  Invoke-CiCompose && `
  Write-Output "`nComposition complete!`n"