# ------------------------------------------------------------------------------------------------------------------------
# BETA: (Untested!) Review-PrWithLLM.ps1
# ------------------------------------------------------------------------------------------------------------------------
# Azure DevOps PR Review Task with LLM Integration
# Usage: Run in Azure DevOps pipeline with required variables
# ------------------------------------------------------------------------------------------------------------------------

param(
    [string]$Organization,
    [string]$Project,
    [string]$RepositoryId,
    [string]$PullRequestId,
    [string]$SystemAccessToken,
    [string]$LLMEndpoint,
    [string]$LLMApiKey
)

function Write-Log($msg) {
    Write-Host "[PR-Review] $msg"
}

# Helper: Call Azure DevOps REST API
function Invoke-AdoApi {
    param(
        [string]$Uri
    )
    $headers = @{ Authorization = "Bearer $SystemAccessToken" }
    Invoke-RestMethod -Uri $Uri -Headers $headers -Method Get
}

# Helper: Call LLM (Azure OpenAI)
function Invoke-LLM {
    param(
        [string]$Prompt
    )
    $headers = @{ "api-key" = $LLMApiKey; "Content-Type" = "application/json" }
    $body = @{ "messages" = @(@{ "role" = "user"; "content" = $Prompt }) } | ConvertTo-Json -Depth 5
    $response = Invoke-RestMethod -Uri $LLMEndpoint -Headers $headers -Method Post -Body $body
    return $response.choices[0].message.content
}

# 1. Fetch PR details
$prUri = "https://dev.azure.com/$Organization/$Project/_apis/git/repositories/$RepositoryId/pullRequests/$PullRequestId?api-version=7.1-preview.1"
$pr = Invoke-AdoApi -Uri $prUri
Write-Log "Fetched PR: $($pr.title)"

# 2. Fetch PR changes
$changesUri = "https://dev.azure.com/$Organization/$Project/_apis/git/repositories/$RepositoryId/pullRequests/$PullRequestId/changes?api-version=7.1-preview.1"
$changes = Invoke-AdoApi -Uri $changesUri

# 3. Build summary prompt
$changedFiles = $changes.changes | ForEach-Object { $_.item.path }
$diffSummary = "Changed files:`n" + ($changedFiles -join "`n")
$prompt = @"
You are a code reviewer. Summarize the following pull request for a DevOps pipeline report. 
Include what was changed (files, features, or logic)
Do not review the code or comment on quality, just summarize the changes and intent based on the PR title and changed files.
Start with a single brief paragraph summary of the PR, then move into more detail if necessary.
The summary should be concise, and the details ideally no more than a few paragraphs.
These paragraphs could include lists of changed files or features.

PR Title: $($pr.title)
PR Description: $($pr.description)
$diffSummary
"@

Write-Log "Sending prompt to LLM..."
$llmSummary = Invoke-LLM -Prompt $prompt

# 4. Output summary to pipeline log and markdown file
Write-Log "LLM Summary:\n$llmSummary"
$summaryPath = "$(Build.ArtifactStagingDirectory)\pr-summary.md"
$llmSummary | Out-File -FilePath $summaryPath -Encoding utf8
Write-Host "##vso[task.setvariable variable=PrReviewSummary]$llmSummary"
Write-Host "##vso[artifact.upload artifactname=pr-summary;]$summaryPath"

# If PR description is empty, update it with the LLM summary
if ([string]::IsNullOrWhiteSpace($pr.description)) {
    Write-Log "PR description is empty. Updating with LLM summary..."
    $patchUri = "https://dev.azure.com/$Organization/$Project/_apis/git/repositories/$RepositoryId/pullRequests/$PullRequestId?api-version=7.1-preview.1"
    $headers = @{ Authorization = "Bearer $SystemAccessToken"; "Content-Type" = "application/json" }
    $body = @{ description = $llmSummary } | ConvertTo-Json
    try {
        $null = Invoke-RestMethod -Uri $patchUri -Headers $headers -Method Patch -Body $body
        Write-Log "PR description updated successfully."
    } catch {
        Write-Log "Failed to update PR description: $_"
    }
}

Write-Log "PR review summary complete."
