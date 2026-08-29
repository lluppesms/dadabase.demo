#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# SPDX-License-Identifier: MIT
#Requires -Version 7.0

<#
.SYNOPSIS
    Exports GitHub Actions repository and environment configuration as gh CLI commands.
.DESCRIPTION
        Reads Actions variables and secret names from the current repository, then writes commands
        for the repository active when the commands are run. GitHub does not expose secret values,
        so generated secret commands use '*' as a placeholder.
.EXAMPLE
        ./.github/scripts/Export-GitHubActionsConfiguration.ps1
.NOTES
        Requires GitHub CLI authentication and must run from a GitHub repository directory.
#>
[CmdletBinding()]
    param()

$ErrorActionPreference = 'Stop'

#region Functions

function ConvertTo-PowerShellLiteral {
    <#
    .SYNOPSIS
        Converts text to a single-quoted PowerShell string literal.
.PARAMETER Value
        Text to quote.
.OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [string]$Value
    )

    return "'$($Value -replace "'", "''")'"
}

function ConvertTo-PowerShellLiteralNoQuotes {
    <#
    .SYNOPSIS
        Converts text to a PowerShell string literal.
.PARAMETER Value
        Text to quote.
.OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [string]$Value
    )

    return "$($Value -replace "'", "''")"
}

function Invoke-GitHubApi {
    <#
    .SYNOPSIS
        Retrieves every page from a GitHub REST API endpoint.
.PARAMETER Endpoint
        Repository-relative REST endpoint.
.OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Endpoint
    )

    $RawResponse = & gh api --paginate --slurp $Endpoint
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI failed while reading '$Endpoint'."
    }

    if ([string]::IsNullOrWhiteSpace(($RawResponse -join [Environment]::NewLine))) {
        return @()
    }

    return @($RawResponse -join [Environment]::NewLine | ConvertFrom-Json)
}

function Get-GitHubActionSecrets {
    <#
    .SYNOPSIS
        Gets secret metadata from a GitHub Actions scope.
.PARAMETER Endpoint
        Secrets endpoint for the repository or an environment.
.OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Endpoint
    )

    $PagedEndpoint = '{0}?per_page=100' -f $Endpoint
    $Secrets = @(Invoke-GitHubApi -Endpoint $PagedEndpoint | ForEach-Object { $_.secrets } | Where-Object { $null -ne $_ })
    return $Secrets
}

function Get-GitHubActionVariables {
    <#
    .SYNOPSIS
        Gets variables from a GitHub Actions scope.
.PARAMETER Endpoint
        Variables endpoint for the repository or an environment.
.OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Endpoint
    )

    $PagedEndpoint = '{0}?per_page=100' -f $Endpoint
    $Variables = @(Invoke-GitHubApi -Endpoint $PagedEndpoint | ForEach-Object { $_.variables } | Where-Object { $null -ne $_ })
    return $Variables
}

function Write-ConfigurationCommands {
    <#
    .SYNOPSIS
        Writes gh secret and variable commands for one GitHub Actions scope.
.PARAMETER Secrets
        Secret metadata to export.
.PARAMETER Variables
        Variables to export.
.PARAMETER EnvironmentName
        Optional GitHub Actions environment name.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyCollection()]
        [object[]]$Secrets,

        [Parameter(Mandatory = $false)]
        [AllowEmptyCollection()]
        [object[]]$Variables,

        [Parameter(Mandatory = $false)]
        [string]$EnvironmentName
    )

    $EnvironmentArgument = if ($EnvironmentName) {
        " --env $(ConvertTo-PowerShellLiteralNoQuotes -Value $EnvironmentName)"
    }
    else {
        ''
    }

    foreach ($Secret in $Secrets | Sort-Object -Property name) {
        Write-Output "gh secret set$EnvironmentArgument $($Secret.name) -b '****'"
    }

    foreach ($Variable in $Variables | Sort-Object -Property name) {
        $ValueArgument = ConvertTo-PowerShellLiteral -Value $Variable.value
        Write-Output "gh variable set$EnvironmentArgument $($Variable.name) -b $ValueArgument"
    }
}

#endregion Functions

#region Main Execution

if ($MyInvocation.InvocationName -ne '.') {
    try {
        if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
            throw 'GitHub CLI (gh) is required. Install it and run gh auth login before retrying.'
        }

        $SourceRepository = & gh repo view --json nameWithOwner --jq '.nameWithOwner'
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($SourceRepository)) {
            throw 'Could not determine the current GitHub repository. Run the script from a GitHub repository directory.'
        }

        $SourceOrganization = ($SourceRepository -split '/', 2)[0]
        # Write-Host "Connecting to GitHub organization: $SourceOrganization" -ForegroundColor Cyan
        # Write-Host "Current repository: $SourceRepository" -ForegroundColor Cyan
        $RepositoryEndpoint = "repos/$SourceRepository"
        Write-Host ""
        Write-Host "# ================================================================================" -ForegroundColor Cyan
        Write-Host "# Show GitHub Actions Secrets and Variables in $SourceRepository"                   -ForegroundColor Cyan
        Write-Host "# As of $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"                                  -ForegroundColor Cyan
        Write-Host "# ================================================================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "# --------------------------------------------------------------------------------"  -ForegroundColor Yellow
        Write-Host "# Repository Level Secrets and Variables"                                            -ForegroundColor Yellow
        Write-Host "# --------------------------------------------------------------------------------"  -ForegroundColor Yellow
        Write-ConfigurationCommands -Secrets (Get-GitHubActionSecrets -Endpoint "$RepositoryEndpoint/actions/secrets") -Variables (Get-GitHubActionVariables -Endpoint "$RepositoryEndpoint/actions/variables")

        $EnvironmentsEndpoint = '{0}/environments?per_page=100' -f $RepositoryEndpoint
        $Environments = Invoke-GitHubApi -Endpoint $EnvironmentsEndpoint |
            ForEach-Object { $_.environments } |
            Sort-Object -Property name

        foreach ($Environment in $Environments) {
            $EnvironmentName = $Environment.name
            $EncodedEnvironmentName = [uri]::EscapeDataString($EnvironmentName)
            $EnvironmentEndpoint = "$RepositoryEndpoint/environments/$EncodedEnvironmentName"

            Write-Host ""
            Write-Host "# --------------------------------------------------------------------------------" -ForegroundColor Green
            Write-Host "# === Environment: $EnvironmentName ==="                                            -ForegroundColor Green
            Write-Host "# --------------------------------------------------------------------------------" -ForegroundColor Green
            Write-ConfigurationCommands -Secrets (Get-GitHubActionSecrets -Endpoint "$EnvironmentEndpoint/secrets") -Variables (Get-GitHubActionVariables -Endpoint "$EnvironmentEndpoint/variables") -EnvironmentName $EnvironmentName
        }
        Write-Host ""
    }
    catch {
        Write-Error -ErrorAction Continue "Export-GitHubActionsConfiguration failed: $($_.Exception.Message)"
        exit 1
    }
}

#endregion Main Execution