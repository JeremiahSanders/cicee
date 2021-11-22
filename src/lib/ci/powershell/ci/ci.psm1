#!/usr/bin/env pwsh

####
# Continuous integration action library for Powershell.
#   Contains "action" functions which perform steps in a continuous integration workflow.
#   By convention, each CI action function begins with "ci-".
####

# Infer this script has been sourced based upon CI_SCRIPT_PATH being non-empty.
# Note: This uses Env:, not $Env, because we're referring to the path in the Powershell Environment provider.
# if (Test-Path Env:CI_SCRIPT_PATH) {
#     # Actions are already sourced. Exit.
#     Write-Output "CI library loaded at: ${Env:CI_SCRIPT_PATH}"
#     exit 0
# }

# CI library context
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDirectory = Split-Path $scriptPath -Parent
$powershellLibDirectory = Split-Path $scriptDirectory -Parent
$Env:CI_SCRIPT_PATH = $scriptPath
$Env:CI_LIB_ROOT = $powershellLibDirectory

$actionsDirectory = Join-Path $Env:CI_LIB_ROOT "actions"
$utilsPath = Join-Path $Env:CI_LIB_ROOT "utils.ps1"

Write-Host "Testing"

Import-Module -Name  $utilsPath
foreach($actionModule in (Get-ChildItem -Path $actionsDirectory -Filter "*.psm1" -Recurse)) {
    Import-Module -Name $actionModule.FullName
    $exportedModuleFunctions = Get-Command -Module $tempModuleName | ForEach-Object { "$($_.name)" }
    foreach($exportedFunction in $exportedModuleFunctions){
        Export-ModuleMember -Function $exportedFunction
        Write-Host "Imported ${exportedFunction} from $(${actionModule}.FullName)"
    }
}
