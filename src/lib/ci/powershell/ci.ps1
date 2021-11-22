#!/usr/bin/env pwsh

####
# Continuous integration action library for Powershell.
#   Contains "action" functions which perform steps in a continuous integration workflow.
#   By convention, each CI action function begins with "ci-".
####

# Infer this script has been sourced based upon CI_SCRIPT_PATH being non-empty.
# Note: This uses Env:, not $Env, because we're referring to the path in the Powershell Environment provider.
if (Test-Path Env:CI_SCRIPT_PATH) {
    # Actions are already sourced. Exit.
    Write-Output "CI library loaded at: ${Env:CI_SCRIPT_PATH}"
    exit 0
}

# CI library context
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDirectory = Split-Path $scriptPath -Parent
$Env:CI_SCRIPT_PATH = $scriptPath
$Env:CI_LIB_ROOT = $scriptDirectory

# The ". <path>" commands below utilize "dot sourcing".
#   See: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_scripts?view=powershell-7.2#script-scope-and-dot-sourcing

function Initialize-CiActions {
    param (
        $CiActionRoot
    )
    foreach($actionModule in (Get-ChildItem -Path $CiActionRoot -Filter "*.ps1" -Recurse)) {
        . $actionModule.FullName
    }
}

$actionsDirectory = Join-Path $Env:CI_LIB_ROOT "actions"
$utilsPath = Join-Path $Env:CI_LIB_ROOT "utils.ps1"

. $utilsPath && Initialize-CiActions $actionsDirectory
