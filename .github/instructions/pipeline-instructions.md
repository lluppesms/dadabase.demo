---
applyTo: ".github/workflows/**,**/.azuredevops/pipelines/**"
---

# Pipeline Instructions

These guidelines apply to both GitHub Actions workflows and Azure DevOps pipelines.

## General Pipeline Guidelines

- Use clear, descriptive workflow/pipeline names and job names
- Reference solution and project files with relative paths
- Use environment variables and secrets for sensitive data — never hardcode credentials
- Keep steps modular and reusable; use templates for repeated patterns
- Use marketplace actions/tasks where available
- Add comments to explain non-obvious steps or configuration

## GitHub Actions Specific

- Pin third-party actions to specific commit SHAs (e.g., `uses: owner/action@<sha>`) rather than mutable tags
- Configure minimal `permissions` for `GITHUB_TOKEN`
- Cache dependencies with `actions/cache` or built-in cache options in setup actions
- Add `timeout-minutes` to jobs to prevent hung workflows
- Use matrix strategies for multi-environment or multi-version testing
- Add `if: always()` for cleanup steps that must run regardless of failure

## Azure DevOps Specific

- Use YAML pipeline templates stored in the `.azuredevops/pipelines` folder
- Use variable groups and pipeline secrets for sensitive data
- Use stage/job/step templates for shared logic across pipelines
