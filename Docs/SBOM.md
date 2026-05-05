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
5. [SBOM Vulnerability Reporting](#sbom-vulnerability-reporting)
6. [Accessing Generated SBOMs](#accessing-generated-sboms)
7. [References](#references)

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

## SBOM Vulnerability Reporting

Once an SBOM is generated, it can be scanned for known vulnerabilities (CVEs) using **[Grype](https://github.com/anchore/grype)** — an open-source vulnerability scanner by Anchore that cross-references SBOM packages against the NVD, OSV, and other vulnerability databases.

### GitHub Actions

The SBOM generation and vulnerability report are combined in a single reusable workflow template:

```
.github/workflows/template-create-sbom.yml
```

It is called from `template-scan-code.yml` when the `runSBOM` input is enabled. Vulnerability scanning runs automatically as part of the same job — no separate input is needed.

#### How It Works

1. The SBOM is generated and saved locally as `sbom.spdx.json`.
2. The SBOM artifact is uploaded for retention.
3. `anchore/scan-action` (which wraps Grype) scans the SBOM against vulnerability databases.
4. A **table summary** is printed to the workflow log.
5. The results are exported as a **SARIF file** and uploaded to the **GitHub Security tab** (requires GitHub Advanced Security).
6. Grype is also run in JSON mode to generate a **styled HTML report**.
7. Both the SARIF and HTML report are uploaded as workflow artifacts (`sbom-vulnerability-report`).

#### Running the Vulnerability Report

1. Navigate to **Actions → 7. Scheduled Scan Code**
2. Click **Run workflow**
3. Check **Generate Software Bill of Materials (SBOM)**
4. Click **Run workflow**

#### Viewing the Results

| Location | What you see |
|----------|-------------|
| **Security → Code scanning** | Grype vulnerability findings as SARIF alerts, filterable by severity, package, and CVE |
| **Workflow Artifacts → `sbom-vulnerability-report`** | `grype-results.sarif` + `sbom-vulnerability-report.html` (styled HTML with severity breakdown) |

#### Required Repository Settings

To upload SARIF results to the Security tab:

- **Public repositories**: Available by default.
- **Private repositories**: Requires GitHub Advanced Security (GHAS) to be enabled.
  Repository → Settings → Security → Code security → GitHub Advanced Security → Enable

#### Template Inputs

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `continueOnError` | boolean | `true` | Whether to continue if the scan fails |
| `artifactName` | string | `sbom` | Name of the SBOM artifact to scan (must match what `template-create-sbom.yml` produced) |

---

### Azure DevOps

Vulnerability scanning is integrated directly into the SBOM generation job (`create-sbom-job.yml`) and runs automatically after the SBOM is published.

#### How It Works

1. After the SBOM is generated, the **Grype** Windows binary is downloaded from the latest GitHub release.
2. Grype scans the `manifest.spdx.json` file:
   - A **table summary** is printed to the pipeline log.
   - A **SARIF file** (`grype-results.sarif`) is generated for security tooling integration.
   - A **JSON report** (`grype-results.json`) is generated for HTML conversion.
3. A PowerShell script converts the Grype JSON output into a **styled HTML report** (`sbom-vulnerability-report.html`).
4. Both the SARIF and HTML report are published as a pipeline artifact named `sbom-vulnerability-report`.

#### Accessing the Vulnerability Report

1. Navigate to your Azure DevOps project → **Pipelines → 7. Scan Code**
2. Click on the latest completed run
3. Click on the **GenerateSBOM** stage
4. Click the **Artifacts** tab
5. Download the `sbom-vulnerability-report` artifact (contains `grype-results.sarif` and `sbom-vulnerability-report.html`)

#### GitHub Advanced Security for Azure DevOps (GHAzDO) — SARIF Upload

If your Azure DevOps project has **GHAzDO** enabled, you can push the Grype SARIF output to the GHAzDO dashboard using the `AdvancedSecurity-Publish@1` task. This is not currently configured but can be added to the job template as an optional step.

---

## Visualizing SBOM Data — Other Options

| Option | Description |
|--------|-------------|
| **GitHub Dependency Graph** | After SBOM generation, the `anchore/sbom-action` automatically submits the SBOM to GitHub's Dependency Graph. View at **Repository → Insights → Dependency graph → Dependencies**. |
| **GitHub Dependency Review** | On pull requests (GHAS required), GitHub flags newly introduced vulnerable dependencies before merge. |
| **SPDX Tools Online** | Upload `sbom.spdx.json` to [tools.spdx.org](https://tools.spdx.org/app/validate/) for validation and human-readable display. |
| **Anchore SBOM Viewer** | Drag-and-drop SPDX/CycloneDX viewer at [sbom.anchore.io](https://sbom.anchore.io/). |

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
| anchore/scan-action | https://github.com/anchore/scan-action |
| Syft (SBOM generator) | https://github.com/anchore/syft |
| Grype (vulnerability scanner) | https://github.com/anchore/grype |
| Microsoft SBOM Tool | https://github.com/microsoft/sbom-tool |
| SPDX Specification | https://spdx.dev/ |
| CycloneDX Specification | https://cyclonedx.org/ |
| NIST SSDF (Secure Software Development Framework) | https://csrc.nist.gov/Projects/ssdf |
| GHAzDO Dependency Scanning | https://learn.microsoft.com/en-us/azure/devops/repos/security/github-advanced-security-dependency-scanning |
