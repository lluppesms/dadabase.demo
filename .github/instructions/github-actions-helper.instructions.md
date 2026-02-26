# Github Actions Helper.Instructions

---
applyTo: ".github/workflows/**/*.yml"
---

When generating or improving GitHub Actions workflows:

\n\nSecurity First

\n\nUse GitHub secrets for sensitive data, never hardcode credentials
\n\nPin third-party actions to specific commits by using the SHA value (e.g., `- uses: owner/some-action@a824008085750b8e136effc585c3cd6082bd575f`)
\n\nConfigure minimal permissions for GITHUB_TOKEN required for the workflow

\n\nPerformance Essentials

\n\nCache dependencies with `actions/cache` or built-in cache options
\n\nAdd `timeout-minutes` to prevent hung workflows
\n\nUse matrix strategies for multi-environment testing

\n\nBest Practices

\n\nUse descriptive names for workflows, jobs, and steps
\n\nInclude appropriate triggers: `push`, `pull_request`, `workflow_dispatch`
\n\nAdd `if: always()` for cleanup steps that must run regardless of failure

\n\nExample Pattern

```yaml

name: CI

on: [push, pull_request]

jobs:

  test:

    runs-on: ubuntu-latest

    timeout-minutes: 10

    steps:
\n\nuses: actions/checkout@v5
\n\nuses: actions/setup-node@v4

        with:

          node-version: 20

          cache: npm
\n\nrun: npm ci
\n\nrun: npm test

```text
\n
