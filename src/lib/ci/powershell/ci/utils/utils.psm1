#!/usr/bin/env pwsh

$scriptPath = $MyInvocation.MyCommand.Path
Write-Output "Executing: ${scriptPath}"

function Assert-EnvironmentVariables {
    param (
        [string[]]$RequiredVariables
    )

    $missingVariables = $RequiredVariables.Where({(-not (Test-Path (Join-Path "Env:" $_)))})

    if( $missingVariables.count -gt 0 )
    {
        $failureDisplay = "Missing required environment variables: ${missingVariables}"
        throw $failureDisplay
    }
}

Export-ModuleMember -Function Assert-EnvironmentVariables
