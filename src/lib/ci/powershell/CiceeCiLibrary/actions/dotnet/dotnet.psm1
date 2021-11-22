#!/usr/bin/env pwsh

####
# .NET continuous integration actions.
#   Contains "action" functions which interact with the .NET CLI (dotnet) and its related utilities, e.g., NuGet.
#   Unless specified otherwise, all functions target the PROJECT_ROOT and assume a .NET solution exists there.
#
# Exported library functions:
#   * Invoke-CiDotnetBuild     - Execute 'dotnet build'.
#   * Invoke-CiDotnetClean     - Execute 'dotnet clean'.
#   * Invoke-CiDotnetNugetPush - Execute 'dotnet nuget push'.
#   * Invoke-CiDotnetPack      - Execute 'dotnet pack'.
#   * Invoke-CiDotnetPublish   - Execute 'dotnet publish'.
#   * Invoke-CiDotnetRestore   - Execute 'dotnet restore'.
#   * Invoke-CiDotnetTest      - Execute 'dotnet test'.
#
# Conditionally-required Environment:
#   $NUGET_API_KEY - NuGet source API key. Required for pushing NuGet packages.
#   $NUGET_SOURCE  - NuGet source, e.g., https://api.nuget.org/v3/index.json . Required for pushing NuGet packages.
####

$scriptPath = $MyInvocation.MyCommand.Path
Write-Output "Executing: ${scriptPath}"

# .NET build
function Invoke-CiDotnetBuild {
    # Use of ${args} below uses the automatic variable provided by powershell. See `$args` in `$ get-help about_Automatic_Variables`.
    dotnet build "${Env:PROJECT_ROOT}" `
        -p:GenerateDocumentationFile=true `
        -p:Version="${Env:PROJECT_VERSION_DIST}" `
        ${args}
}

function Invoke-CiDotnetClean {
    # Use of ${args} below uses the automatic variable provided by powershell. See `$args` in `$ get-help about_Automatic_Variables`.
    dotnet clean "${Env:PROJECT_ROOT}" `
        ${args}
}

# .NET Project publish
function Invoke-CiDotnetPublish {
    # Use of ${args} below uses the automatic variable provided by powershell. See `$args` in `$ get-help about_Automatic_Variables`.
    dotnet publish "${Env:PROJECT_ROOT}/src" `
        --configuration Release `
        --output "${Env:BUILD_UNPACKAGED_DIST}" `
        -p:DocumentationFile="${Env:PROJECT_ROOT}/build/docs/${Env:PROJECT_NAME}-${Env:PROJECT_VERSION_DIST}.xml" `
        -p:Version="${Env:PROJECT_VERSION_DIST}" `
        ${args}
}
  
function Invoke-CiDotnetRestore {
    # Use of ${args} below uses the automatic variable provided by powershell. See `$args` in `$ get-help about_Automatic_Variables`.
    dotnet restore "${Env:PROJECT_ROOT}" `
        ${args}
}
  
function Invoke-CiDotnetTest {
    # Use of ${args} below uses the automatic variable provided by powershell. See `$args` in `$ get-help about_Automatic_Variables`.
    dotnet test "${Env:PROJECT_ROOT}" `
        ${args}
}

#--
# NuGet
#--

# .NET NuGet pack
function Invoke-CiDotnetPack {
    # Use of ${args} below uses the automatic variable provided by powershell. See `$args` in `$ get-help about_Automatic_Variables`.
    dotnet pack "${Env:PROJECT_ROOT}/src" `
        --configuration Release `
        --output "${Env:BUILD_PACKAGED_DIST}/nuget/" `
        -p:DocumentationFile="${Env:BUILD_DOCS}/${Env:PROJECT_NAME}-${Env:PROJECT_VERSION_DIST}.xml" `
        -p:PackageVersion="${Env:PROJECT_VERSION_DIST}" `
        -p:Version="${Env:PROJECT_VERSION_DIST}" `
        ${args}
}
  
# .NET NuGet push - requires environment $NUGET_SOURCE and NUGET_API_KEY
function Invoke-CiDotnetNugetPush {
    $nugetPackageDirectory = Join-Path $Env:BUILD_PACKAGED_DIST "nuget"
    foreach($packageFile in (Get-ChildItem -Path $nugetPackageDirectory -Filter "*.nupkg" -Recurse)) {
        $packagePath = $packageFile.FullName

        dotnet nuget push "${packagePath}" --api-key "${Env:NUGET_API_KEY}" --source "${Env:NUGET_SOURCE}"

        Write-Output "Pushed '${packagePath}'"
    }
}

Export-ModuleMember -Function Invoke-CiDotnetBuild
Export-ModuleMember -Function Invoke-CiDotnetClean
Export-ModuleMember -Function Invoke-CiDotnetPublish
Export-ModuleMember -Function Invoke-CiDotnetRestore
Export-ModuleMember -Function Invoke-CiDotnetTest
Export-ModuleMember -Function Invoke-CiDotnetPack
Export-ModuleMember -Function Invoke-CiDotnetNugetPush
