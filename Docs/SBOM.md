# Software Bill of Materials (SBOM)

## Overview

A Software Bill of Materials (SBOM) is a machine-readable inventory of all software components, libraries, and dependencies included in an application. SBOMs are increasingly required for compliance, supply-chain security, and vulnerability management — especially in regulated industries and government contracts.

This document describes how SBOM generation is implemented in the DadABase project for both **GitHub Actions** and **Azure DevOps**.

---

## Table of Contents

1. [What is an SBOM?](#what-is-an-sbom)
2. [SBOM Formats](#sbom-formats)
3. [GitHub Actions Implementation](#github-actions-implementation)
4. [Azure DevOps Implementation](#azure-devops-implementation)
5. [Accessing Generated SBOMs](#accessing-generated-sboms)
6. [References](#references)

---

## What is an SBOM?

An SBOM provides a complete, formal, machine-readable list of the open-source and third-party components used in an application. It helps organizations:

- Identify and respond to newly disclosed vulnerabilities (e.g., Log4Shell)
- Fulfill compliance requirements (Executive Order 14028, NIST SSDF)
- Understand the full software supply chain
- Support software procurement decisions

---

## SBOM Formats

Two formats are widely adopted:

| Format | Description |
|--------|-------------|
| **SPDX** (Software Package Data Exchange) | Linux Foundation standard; ISO/IEC 5962:2021 |
| **CycloneDX** | OWASP standard; strongly typed XML/JSON; popular in DevSecOps tooling |

The DadABase pipelines generate SBOM output in **SPDX JSON** format by default, which is the format natively supported by GitHub's Dependency Graph.

---

## GitHub Actions Implementation

### Template File

The SBOM generation logic lives in a dedicated reusable workflow template:

```
.github/workflows/template-create-sbom.yml
```

This template uses the **[anchore/sbom-action](https://github.com/anchore/sbom-action)** GitHub Action, which is built on top of [Syft](https://github.com/anchore/syft) — a widely used, open-source SBOM generator.

### How It Works

1. The repository is checked out.
2. Syft scans the repository for all detectable packages and dependencies:
   - NuGet packages (`.csproj`, `packages.config`)
   - npm packages (`package.json`, `package-lock.json`)
   - GitHub Dependency Graph (if enabled)
3. An SBOM is generated in SPDX JSON format.
4. The SBOM is uploaded as a workflow artifact (retained for 90 days).

### Triggering SBOM Generation

SBOM generation is integrated into the scan workflow (`7-scan-code.yml`) and the reusable scan template (`template-scan-code.yml`).

**Workflow dispatch inputs** (in `7-scan-code.yml`):

| Input | Default | Description |
|-------|---------|-------------|
| `runSBOM` | `true` | Enable/disable SBOM generation |

The `template-scan-code.yml` accepts a `runSBOM` boolean input that is passed through from the calling workflow.

### Running Manually

1. Navigate to **Actions → 7. Scheduled Scan Code**
2. Click **Run workflow**
3. Check the **Generate Software Bill of Materials (SBOM)** box
4. Click **Run workflow**

### Required Repository Settings

For full SBOM coverage including the GitHub Dependency Graph:

1. Go to **Repository → Settings → Security → Code security**
2. Enable **Dependency graph**

### Template Inputs

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `continueOnSBOMError` | boolean | `true` | Whether to continue if SBOM generation fails |
| `sbomFormat` | string | `spdx-json` | Output format: `spdx-json`, `cyclonedx-json`, or `syft-json` |
| `artifactName` | string | `sbom` | Name for the uploaded artifact |

### Example: Calling the SBOM Template Directly

```yaml
jobs:
  generate-sbom:
    uses: ./.github/workflows/template-create-sbom.yml
    with:
      continueOnSBOMError: true
      sbomFormat: spdx-json
      artifactName: sbom
```

### Also Available: GitHub's Built-in SBOM Export

GitHub also offers a native SBOM export via the UI or API (does not require a workflow):

- **UI**: Repository → **Insights** → **Dependency graph** → **Export SBOM**
- **API**: `GET /repos/{owner}/{repo}/dependency-graph/sbom`

This produces an SPDX 2.3 JSON file directly from GitHub's Dependency Graph, but only covers dependencies GitHub has detected. The workflow-based approach (using Syft) provides more comprehensive coverage by scanning the repository contents directly.

---

## Azure DevOps Implementation

### Template File

The SBOM generation logic lives in a dedicated job template:

```
.azdo/pipelines/jobs/create-sbom-job.yml
```

This template uses the **[Microsoft SBOM Tool](https://github.com/microsoft/sbom-tool)** — an open-source, cross-platform SBOM generator developed by Microsoft that produces SPDX 2.2 JSON output.

### How It Works

1. The Microsoft SBOM Tool executable is downloaded from the latest GitHub release.
2. The tool scans the repository using component detectors for:
   - NuGet packages (`.csproj` files, `packages.config`)
   - npm packages (`package.json`, `package-lock.json`)
   - Other supported ecosystems (Python, Go, etc.)
3. An SBOM is generated in SPDX 2.2 JSON format.
4. The SBOM is published as a pipeline artifact named `sbom`.

> **Note**: The SBOM Tool performs component detection using source-based scanning. No build is required.

### Integration with the Scan Pipeline

SBOM generation is integrated as a separate stage in the scan pipeline:

- **Stage file**: `.azdo/pipelines/stages/scan-code-stages.yml`
- **Root pipeline**: `.azdo/pipelines/7-scan-code.yml`

The `GenerateSBOM` stage runs in parallel with the `ScanApplication` stage (no dependency).

### Pipeline Parameters

In `7-scan-code.yml`:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `runSBOM` | boolean | `true` | Enable/disable SBOM generation |

### Running Manually

1. Navigate to your Azure DevOps project → **Pipelines → 7. Scan Code**
2. Click **Run pipeline**
3. Check the **Generate Software Bill of Materials (SBOM)** checkbox
4. Click **Run**

### Job Template Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `environmentName` | `DEV` | Deployment environment name |
| `continueOnSBOMError` | `true` | Continue pipeline if SBOM generation fails |
| `packageName` | `DadABase` | SBOM package name field |
| `packageVersion` | `1.0.0` | SBOM package version field |
| `packageSupplier` | `DadABase` | SBOM supplier/organization field |

### Example: Calling the SBOM Job Template Directly from a Stage

```yaml
stages:
- stage: GenerateSBOM
  displayName: Generate SBOM
  jobs:
  - template: ../jobs/create-sbom-job.yml
    parameters:
      environmentName: 'DEV'
      packageName: 'MyApp'
      packageVersion: '2.0.0'
      packageSupplier: 'MyOrg'
      continueOnSBOMError: 'true'
```

### GitHub Advanced Security for Azure DevOps (GHAzDO) and SBOMs

If your Azure DevOps project has **GitHub Advanced Security for Azure DevOps (GHAzDO)** enabled, the SBOM and dependency information collected by the `AdvancedSecurity-Dependency-Scanning@1` task (already present in the scan job) is fed into the GHAzDO Dependency Scanning feature. This is separate from a standalone SBOM artifact but provides similar supply-chain visibility within the Azure DevOps interface.

For more information, see:
[GitHub Advanced Security for Azure DevOps - Dependency scanning](https://learn.microsoft.com/en-us/azure/devops/repos/security/github-advanced-security-dependency-scanning)

---

## Accessing Generated SBOMs

### GitHub Actions

After a scan workflow run completes:

1. Navigate to **Actions → 7. Scheduled Scan Code**
2. Click on the latest completed run
3. Scroll to **Artifacts**
4. Download the `sbom` artifact (contains `sbom.spdx.json`)

### Azure DevOps

After the scan pipeline run completes:

1. Navigate to your Azure DevOps project → **Pipelines → 7. Scan Code**
2. Click on the latest completed run
3. Click on the **GenerateSBOM** stage
4. Click the **Artifacts** tab
5. Download the `sbom` artifact (contains the `manifest.spdx.json` file)

---

## References

| Resource | Link |
|----------|------|
| GitHub SBOM Documentation | https://docs.github.com/en/code-security/supply-chain-security/understanding-your-software-supply-chain/exporting-a-software-bill-of-materials-for-your-repository |
| anchore/sbom-action | https://github.com/anchore/sbom-action |
| Syft (SBOM generator) | https://github.com/anchore/syft |
| Microsoft SBOM Tool | https://github.com/microsoft/sbom-tool |
| SPDX Specification | https://spdx.dev/ |
| CycloneDX Specification | https://cyclonedx.org/ |
| NIST SSDF (Secure Software Development Framework) | https://csrc.nist.gov/Projects/ssdf |
| GHAzDO Dependency Scanning | https://learn.microsoft.com/en-us/azure/devops/repos/security/github-advanced-security-dependency-scanning |
