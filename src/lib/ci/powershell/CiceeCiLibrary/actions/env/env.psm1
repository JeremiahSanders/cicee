#!/usr/bin/env pwsh

####
# Continuous integration environment action library.
#   These actions are intended to support other actions and be components of workflows.
#
# Exported library functions:
#   * Assert-CiEnv     - Validates that the CI environment is initialized.
#   * Initialize-CiEnv - Initialize CI environment. *ALL OTHER ACTIONS ASSUME THIS ENVIRONMENT.*
#   * Reset-CiEnv      - Unsets predenfined environment variables set by Initialize-CiEnv. Does not reset project or local variables.
#   * Show-CiEnv       - Display initialized CI environment.
####

$scriptPath = $MyInvocation.MyCommand.Path
Write-Output "Executing: ${scriptPath}"

##
# CI Environment Initialization.
#   All CI actions implicitly require this function to be executed. Its purpose is to provide a consistent execution
#   environment for project-level activities. For example, by using a consistent BUILD_PACKAGED_DIST variable,
#   CI actions creating an output zip file, NPM package, or APK can all place their artifacts in consistent locations.
#   Design note: This function's name follows the CI workflow pattern because it is expected to be executed in a
#                workflow entrypoint script.
#
# Expected environment available to all CI actions, after Initialize-CiEnv is executed:
#  - Contextual -
#   PROJECT_NAME - Project name. By convention this should be in lower kebab case. I.e., multi-word-project-name. This will be used to pattern conventional output paths, e.g., as part of a zip archive file name.
#   PROJECT_ROOT - Project root directory.
#   PROJECT_METADATA - Project metadata path.
#   PROJECT_TITLE - Project human-readable name. Defaults to PROJECT_NAME.
#   PROJECT_VERSION - Project distributable Major.Minor.Patch version. I.e., 2.3.1.
#   PROJECT_VERSION_DIST - Project distributable version. Expected to be in the following format: Release versions: Major.Minor.Patch, e.g., 4.1.7. Pre-release versions: Major.Minor.Patch-sha-GitSha, e.g., 4.1.7-sha-a7328f. These formats are very important. They help ensure compatibility across .NET projects, .NET NuGet packages, and Docker tags.
#   CURRENT_GIT_BRANCH - Current Git branch.
#   CURRENT_GIT_HASH - Current Git hash.
#  - Conventional Output -
#   BUILD_DOCS="${BUILD_ROOT}/docs" - Project distributable documentation which would accompany packaged build output.
#   BUILD_PACKAGED_DIST="${BUILD_ROOT}/dist" - Project packaged build output. E.g., .NET NuGet packages, zip archives, AWS CDK cloud assemblies.
#   BUILD_ROOT - Project build output root directory.
#   BUILD_UNPACKAGED_DIST="${BUILD_ROOT}/app" - Project unpackaged build output. E.g., processed output which might be further packaged or compressed before distribution. E.g., .NET DLLs, Java class files.
#
# Workflow:
#   1 - Initialize convention-based CI enviroment variables.
#   2 - Attempt to load project metadata by convention.
#       Populates PROJECT_NAME, PROJECT_TITLE, and PROJECT_VERSION from JSON file. From "name", "title", and "version" properties, respectively.
#       Default location: PROJECT_ROOT/project-metadata.json
#       Additional paths: PROJECT_ROOT/.project-metadata.json, PROJECT_ROOT/ci/project-metadata.json, PROJECT_ROOT/ci/.project-metadata.json, PROJECT_ROOT/package.json
#   3 - Apply fallback values for expected metadata.
#       PROJECT_VERSION_DIST defaults to PROJECT_VERSION if RELEASE_ENVIRONMENT=true in the environment, or PROJECT_VERSION-sha-CURRENT_GIT_HASH if not.
#   4 - Source project environment file, if available; enabling setting project defaults and overrides to convention.
#       This file is assumed to be stored in version control and all environments (e.g., both local and build server) are assumed to have the same file.
#       Default location: PROJECT_ROOT/ci/env.project.ps1
#       Additional paths: PROJECT_ROOT/ci/env.ci.ps1, PROJECT_ROOT/ci/project.ps1, PROJECT_ROOT/env.project.ps1, PROJECT_ROOT/project.ps1
#   5 - Source local environment file, if available; enabling setting local defaults and overrides to convention.
#       This file is assumed to NOT BE stored in version control. No consistency between environments is assumed, though patterns and templates are recommended.
#       Default location: PROJECT_ROOT/ci/env.local.ps1
#       Additional paths: PROJECT_ROOT/ci/env.ps1, PROJECT_ROOT/env.local.ps1, PROJECT_ROOT/env.ps1
##
function Initialize-CiEnv {
    param (
        [string]$ProjectRoot
    )

    $initialPwd = $(Get-Location).Path
    if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
        if ([string]::IsNullOrWhiteSpace($Env:PROJECT_ROOT)) {
            Write-Output "Project root was not provided. Defaulting to ${initialPwd}."
            $ProjectRoot = $initialPwd
        }
        else {
            $ProjectRoot = $Env:PROJECT_ROOT
            Write-Output "Project root was not provided. Using environment PROJECT_ROOT: ${ProjectRoot}."
        }
    }
    else {
        Write-Output "Initializing CI environment using project root: ${ProjectRoot}"
    }

    #----
    # Tries to source a project environment file.
    #----
    function __loadProjectEnvironment {
        # Infer the project environment has been sourced based upon PROJECT_ENV_FILE being non-empty.
        if (-not [string]::IsNullOrWhiteSpace($Env:PROJECT_ENV_FILE)) {
            # Environment was already sourced.
            Write-Output "Project CI environment currently sourced: ${Env:PROJECT_ENV_FILE}"
            return
        }
  
        # Try to source several common project environment file locations, preferring those in the ci directory.
        # Final return 0 ensures we return success, even if no environment files were loaded.
        $projectEnvironmentLocations = "${Env:PROJECT_ROOT}/ci/env.project.ps1" , `
            "${Env:PROJECT_ROOT}/ci/env.ci.ps1" , `
            "${Env:PROJECT_ROOT}/ci/project.ps1" , `
            "${Env:PROJECT_ROOT}/env.project.ps1", `
            "${Env:PROJECT_ROOT}/project.ps1" 
        foreach ($possibleLocation in $projectEnvironmentLocations) {
            if (Test-Path $possibleLocation) {
                try {
                    . $possibleLocation
                    $Env:PROJECT_ENV_FILE = $possibleLocation
                    Write-Output "Sourced project CI environment: ${Env:PROJECT_ENV_FILE}"
                    return
                }
                catch {
                
                }
            }
        }

        Write-Output "No project environment initialization script found.`n  Recommended location: ${Env:PROJECT_ROOT}/ci/env.project.ps1"
    }

    #----
    # Tries to source a local environment file.
    #----
    function __loadLocalEnvironment {
        # Infer the project environment has been sourced based upon LOCAL_ENV_FILE being non-empty.
        if (-not [string]::IsNullOrWhiteSpace($Env:LOCAL_ENV_FILE )) {
            # Environment was already sourced.
            Write-Output "Local CI environment currently sourced: ${Env:LOCAL_ENV_FILE}"
            return
        }
  
        # Try to source several common local environment file locations, preferring those in the ci directory.
        $localEnvironmentLocations = "${Env:PROJECT_ROOT}/ci/env.local.ps1" , `
            "${Env:PROJECT_ROOT}/ci/env.ps1" , `
            "${Env:PROJECT_ROOT}/env.local.ps1" , `
            "${Env:PROJECT_ROOT}/env.ps1"
        foreach ($possibleLocation in $localEnvironmentLocations) {
            if (Test-Path $possibleLocation) {
                try {
                    . $possibleLocation
                    $Env:LOCAL_ENV_FILE = $possibleLocation
                    Write-Output "Sourced local CI environment: ${Env:LOCAL_ENV_FILE}"
                    return
                }
                catch {
                    # Fail silently so the next location may be checked.
                }
            }
        }
        
        Write-Output "No local environment initialization script found.`n  Recommended location: ${Env:PROJECT_ROOT}/ci/env.local.ps1"
    }
    
    #----
    # Initializes environment variables which are derived from other values.
    #----
    function __initializeDerivedEnvironment {
        $Env:PROJECT_ROOT = $ProjectRoot

        # Build output paths -- supports consistent output expectations.
        $Env:BUILD_ROOT = "${Env:PROJECT_ROOT}/build"
        $Env:BUILD_UNPACKAGED_DIST = "${Env:BUILD_ROOT}/app"
        $Env:BUILD_PACKAGED_DIST = "${Env:BUILD_ROOT}/dist"
        $Env:BUILD_DOCS = "${Env:BUILD_ROOT}/docs"

        if ( $null -ne $(Get-Command git) -and $(Test-Path "${Env:PROJECT_ROOT}/.git") -eq $true ) {
            # We have git installed
            $Env:CURRENT_GIT_BRANCH = $($(git branch).Split('`n') `
                | Where-Object -FilterScript { $_.Contains("*") } `
                | ForEach-Object { $_.Replace("*", "").Trim() })
            $Env:CURRENT_GIT_HASH = "$(git log --pretty=format:'%h' -n 1)"
        }
        else {
            $Env:CURRENT_GIT_BRANCH = "git-is-not-installed"
            $Env:CURRENT_GIT_HASH = "0000000"
        }
    }

    #----
    # Try to load project metadata file.
    #----
    function __tryLoadProjectMetadata {
  
        function __loadMetadataFromFile {
            [CmdletBinding()]
            param (
                [Parameter()]
                [String]
                $ProjectMetadata
            )
            
            if (Test-Path -Path $ProjectMetadata) {
                $projectMetadataContent = Get-Content -Path $ProjectMetadata | ConvertFrom-Json

                $Env:PROJECT_NAME = $projectMetadataContent.name
                $Env:PROJECT_TITLE = $projectMetadataContent.title
                $Env:PROJECT_VERSION = $projectMetadataContent.version
                $Env:PROJECT_METADATA = $ProjectMetadata

                $ciEnvironmentVariables = $projectMetadataContent.ciEnvironment.variables
                $variablesWithDefaults = ($ciEnvironmentVariables `
                    | Where-Object -FilterScript { -not ([string]::IsNullOrWhiteSpace($_.defaultValue)) })
  
                Foreach ($envVar in (($variablesWithDefaults))) {
                    # Only set the default if the value is currently unspecified.
                    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable(${envVar}.name))) {
                        [Environment]::SetEnvironmentVariable(${envVar}.name, ${envVar}.defaultValue)
                        Write-Output "$(${envVar}.name) was not provided in environment. Setting to default value: $([Environment]::GetEnvironmentVariable(${envVar}.name))"
                    }
                }
                Write-Output "Loaded project metadata from ${ProjectMetadata}"
            }
            else {
                throw "Metadata file ${ProjectMetadata} does not exist."
            }
        }
  
        $metadataLocations = "${Env:PROJECT_ROOT}/project-metadata.json" , `
            "${Env:PROJECT_ROOT}/.project-metadata.json" , `
            "${Env:PROJECT_ROOT}/ci/project-metadata.json" , `
            "${Env:PROJECT_ROOT}/ci/.project-metadata.json" , `
            "${Env:PROJECT_ROOT}/package.json" , `
            "${Env:PROJECT_ROOT}/src/package.json" 
        foreach ($metadataPath in $metadataLocations) {
            try {
                __loadMetadataFromFile -ProjectMetadata $metadataPath
                return
            }
            catch {
                # Fail silently so we can check the next file
            }
        }
    }

    #----
    # Apply defaults for project environment variables which were not configured.
    #----
    function __applyFallbackProjectEnvironmentValues {
        if (-not (Test-Path Env:PROJECT_NAME)) { $Env:PROJECT_NAME = "uknown-project" }
        if (-not (Test-Path Env:PROJECT_TITLE)) { $Env:PROJECT_TITLE = $Env:PROJECT_NAME }
        if (-not (Test-Path Env:PROJECT_VERSION)) { $Env:PROJECT_VERSION = "0.0.0" }

        if ($Env:RELEASE_ENVIRONMENT -eq $true) {
            if (-not (Test-Path Env:PROJECT_VERSION_DIST)) { $Env:PROJECT_VERSION_DIST = "${Env:PROJECT_VERSION}" }
        }
        else {
            if (-not (Test-Path Env:PROJECT_VERSION_DIST)) { 
                $prereleaseSuffix = "sha-${Env:CURRENT_GIT_HASH}"
                $Env:PROJECT_VERSION_DIST = "${Env:PROJECT_VERSION}-${prereleaseSuffix}" 
            }
        }
    }

    # ... now that the workflow functions are established, execute the initialization workflow...
    # 1 - Initialize convention-based CI enviroment variables.
    # 2 - Attempt to load project metadata by convention.
    # 3 - Apply fallback values for expected metadata.
    # 4 - Source project environment file, if available; enabling setting project defaults and overrides to convention.
    # 5 - Source local environment file, if available; enabling setting local defaults and overrides to convention.
    Write-Output "Initializing CI environment..."
    __initializeDerivedEnvironment
    __tryLoadProjectMetadata
    __applyFallbackProjectEnvironmentValues
    __loadProjectEnvironment
    __loadLocalEnvironment
    Write-Output "CI environment initialized."
}

##
# CI Environment Display.
#   Displays the current CI environment metadata and build output structure.
##
function Show-CiEnv {
    Write-Output "`n-- CI Environment -"
    Write-Output "Project            :  ${Env:PROJECT_NAME}"
    Write-Output "  Title            :  ${Env:PROJECT_TITLE}"
    Write-Output "  Version          :  ${Env:PROJECT_VERSION}"
    Write-Output "    Distribution   :  ${Env:PROJECT_VERSION_DIST}"
    Write-Output "  Root             :  ${Env:PROJECT_ROOT}`n"

    Write-Output "Git"
    Write-Output "  Branch           :  ${Env:CURRENT_GIT_BRANCH}"
    Write-Output "  Commit Hash      :  ${Env:CURRENT_GIT_HASH}`n"
  
    Write-Output "Build output       :  ${Env:BUILD_ROOT}"
    Write-Output "  Documentation    :  ${Env:BUILD_DOCS}"
    Write-Output "  Packaged         :  ${Env:BUILD_PACKAGED_DIST}"
    Write-Output "  Unpackaged       :  ${Env:BUILD_UNPACKAGED_DIST}`n"

    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable("PROJECT_METADATA"))) {
        return
    }
    else {
        if (-not $(Test-Path -Path $Env:PROJECT_METADATA)) {
            Write-Error "Failed to load project metadata from: ${Env:PROJECT_METADATA}"
            return
        }
    }

    $projectMetadataContent = Get-Content -Path $Env:PROJECT_METADATA | ConvertFrom-Json
    $ciEnvironmentVariables = $projectMetadataContent.ciEnvironment.variables

    if (($null -eq $ciEnvironmentVariables) -or (($ciEnvironmentVariables.Count) -eq 0)) {
        Write-Output "No CI environment variables defined."
    }
    else {
        # Loop through the CI environment variables.
        Write-Output "Variables"
        Foreach ($envVar in (($ciEnvironmentVariables) | Sort-Object -Property name)) {
            # If the variable is defined as "secret"...
            if (${envVar}.secret -eq $true) {
                # ... and if the variable has a value...
                if (-not [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable(${envVar}.name))) {
                    # ...record the value masked.
                    $varVal = "********"
                }
                else {
                    # Otherwise, record it empty.
                    $varVal = ""
                }
            }
            else {
                # Since the variable is NOT "secret", we display the value.
                $varVal = [Environment]::GetEnvironmentVariable(${envVar}.name)
            }
            if ((${envVar}.required) -and ([string]::IsNullOrWhiteSpace($varVal))) {
                # CI environment definition considers the variable required, but it is unset.
                $varVal = "!INVALID CI ENVIRONMENT! !VALUE REQUIRED BUT NOT PROVIDED!"
                [Console]::ForegroundColor = "red"
            }
            $displayValue = "  $(${envVar}.name.PadRight(17)): ${varVal}"
            Write-Output $displayValue
            [Console]::ResetColor()
        }
    }
    Write-Output "-------------------"
}

##
# CI Environment Require.
#   Validates that the CI environment is initialized and all required variables set.
##
function Assert-CiEnv {
    Assert-EnvironmentVariables `
        "BUILD_DOCS", `
        "BUILD_PACKAGED_DIST", `
        "BUILD_ROOT", `
        "BUILD_UNPACKAGED_DIST", `
        "PROJECT_NAME", `
        "PROJECT_ROOT", `
        "PROJECT_TITLE", `
        "PROJECT_VERSION_DIST", `
        "PROJECT_VERSION"

    if ((-not ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable("PROJECT_METADATA"))))) {
        $projectMetadataContent = Get-Content -Path $Env:PROJECT_METADATA | ConvertFrom-Json
        $ciEnvironmentVariables = $projectMetadataContent.ciEnvironment.variables
        $requiredVariables = ($ciEnvironmentVariables `
            | Where-Object -FilterScript { $_.required }
            | ForEach-Object { "$($_.name)" })
        Assert-EnvironmentVariables $requiredVariables
    }

    Write-Output "Successfully validated CI environment variables."
}

##
# CI Environment Reset.
#   Removes standard environment initialization variables.
##
function Reset-CiEnv {
    $Env:BUILD_DOCS = $null
    $Env:BUILD_PACKAGED_DIST = $null
    $Env:BUILD_ROOT = $null
    $Env:BUILD_UNPACKAGED_DIST = $null
    $Env:CURRENT_GIT_BRANCH = $null
    $Env:CURRENT_GIT_HASH = $null
    $Env:LOCAL_ENV_FILE = $null
    $Env:PROJECT_ENV_FILE = $null
    $Env:PROJECT_METADATA = $null
    $Env:PROJECT_NAME = $null
    $Env:PROJECT_ROOT = $null
    $Env:PROJECT_TITLE = $null
    $Env:PROJECT_VERSION = $null
    $Env:PROJECT_VERSION_DIST = $null
    Write-Output "Successfully reset standard CICEE CI environment variables."
}

Export-ModuleMember -Function Assert-CiEnv
Export-ModuleMember -Function Initialize-CiEnv
Export-ModuleMember -Function Reset-CiEnv
Export-ModuleMember -Function Show-CiEnv
