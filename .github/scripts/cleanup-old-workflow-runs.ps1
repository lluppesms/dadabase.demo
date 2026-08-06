[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [Alias('WorkflowPath')]
    [string]$WorkflowName,

    [Parameter()]
    [int]$Keep = 10
)

$ErrorActionPreference = 'Stop'

if ($Keep -lt 0) {
    throw '-Keep must be zero or greater.'
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'GitHub CLI (gh) is required. Install it and run gh auth login first.'
}

try {
    gh auth status | Out-Null
}
catch {
    throw 'You are not logged into GitHub CLI. Run gh auth login first.'
}

try {
    $repository = (gh repo view --json nameWithOwner --jq '.nameWithOwner').Trim()
}
catch {
    throw 'Could not resolve the current repository from gh. Run this command from inside a GitHub repo.'
}

if ([string]::IsNullOrWhiteSpace($repository)) {
    throw 'Could not resolve current repository. Run this command from inside a GitHub repo.'
}

$workflowId = $WorkflowName
if ($workflowId -like '.github/workflows/*') {
    $workflowId = Split-Path -Leaf $workflowId
}

function Invoke-GitHubApi {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('GET', 'DELETE')]
        [string]$Method,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    Write-Host "$Method $Path"
    if ($Method -eq 'GET') {
        $json = gh api --method GET --header 'X-GitHub-Api-Version: 2022-11-28' $Path
        return $json | ConvertFrom-Json
    }

    gh api --method DELETE --header 'X-GitHub-Api-Version: 2022-11-28' $Path | Out-Null
}

$perPage = 100
$page = 1
$runs = @()

do {
    $path = "repos/$repository/actions/workflows/$workflowId/runs?per_page=$perPage&page=$page"
    $response = Invoke-GitHubApi -Method GET -Path $path
    $pageRuns = @($response.workflow_runs)

    if ($pageRuns.Count -eq 0) {
        break
    }

    $runs += $pageRuns
    $page += 1
} while ($pageRuns.Count -eq $perPage)

$sortedRuns = $runs | Sort-Object -Property created_at -Descending
$candidateRuns = @($sortedRuns | Select-Object -Skip $Keep)
$cutoffUtc = (Get-Date).ToUniversalTime().AddHours(-48)
$oldRuns = @(
    $candidateRuns | Where-Object {
        ([DateTimeOffset]::Parse($_.created_at).UtcDateTime -lt $cutoffUtc)
    }
)
$recentProtectedCount = $candidateRuns.Count - $oldRuns.Count

Write-Host "Found $($sortedRuns.Count) runs for $workflowId in $repository."
Write-Host "Keep rule protects newest $Keep run(s)."
Write-Host "Time rule protects $recentProtectedCount candidate run(s) from the last 48 hours."
Write-Host "Deleting $($oldRuns.Count) run(s) older than 48 hours."

foreach ($run in $oldRuns) {
    $deletePath = "repos/$repository/actions/runs/$($run.id)"
    Invoke-GitHubApi -Method DELETE -Path $deletePath
    Write-Host "Deleted run $($run.id) created $($run.created_at) status=$($run.status) conclusion=$($run.conclusion)"
}