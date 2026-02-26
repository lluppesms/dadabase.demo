# Translate Workflow To Pipeline.Prompt

---
agent: agent

description: Translate a basic workflow at "$(git rev-parse --show-toplevel)\.github\workflows\01-level-workflow.yml" from GitHub Actions to Azure DevOps Pipelines

name: translate-workflow-to-pipeline

tools: ["azuredevops/*", "github/*"]

model: Claude Haiku 4.5 (copilot)
---

\n\nContext

To simplify the migration from GitHub Actions to Azure DevOps Pipelines, we need to convert existing GitHub workflow files into Azure DevOps pipeline YAML files.

\n\nIntent

As a developer responsible for maintaining CI/CD processes, I want to automate the translation of GitHub workflow files into Azure DevOps pipeline YAML files. This will help streamline our migration process and ensure consistency between the two systems.

\n\nSpecific Request

Translate the basic workflow at: "$(git rev-parse --show-toplevel)\.github\workflows\01-level-workflow.yml" to an equivalent azure devops pipeline YAML and save it in the directory "$(git rev-parse --show-toplevel)\.azure-pipelines\01-level-pipeline.yml".

\n\nGuidelines

\n\nEnsure that all GitHub Actions specific syntax and features are appropriately mapped to their Azure DevOps counterparts.
\n\nMaintain the logical structure and flow of the original workflow in the translated pipeline.
\n\nValidate the generated Azure DevOps pipeline YAML for correctness and completeness.
\n\nIf there are any features in the GitHub workflow that do not have a direct equivalent in Azure DevOps, provide a comment in the generated YAML indicating this.
\n\nPreserve any environment variables, secrets, and other configurations as much as possible.
\n\nIf the 01-level-pipeline.yml file already exists, overwrite it with the new translation.
\n\nCreate a new Azure DevOps project in the organization `autocloudarc-mcaps` with the project name: `translate-workflow-to-pipeline` and set its visibility to private. If the project already exists, skip the creation step.
\n\nNOTE: Manual step: Reference the personal access token stored in the github pat: `jet-brains-ide-generic-pat-classic` for authentication when importing the repository.

\n\nCaveats

\n\nSome GitHub Actions may not have direct equivalents in Azure DevOps; in such cases, provide alternative solutions or workarounds.
\n\nEnsure that any third-party actions used in the GitHub workflow are either replaced with native Azure DevOps tasks or documented for manual implementation.
\n\nPay attention to differences in syntax, especially for conditionals, loops, and job dependencies.
\n\nWhen displaying the default branch in Azure DevOps pipelines, do not attempt to display the default branch, since in Azure DevOps, we would have to fetch it via the Azure DevOps REST API or set it as a pipeline variable manually beforehand, which is outside the scope of this simple translation task.

\n\nImplementation Steps

\n\n**Create Azure DevOps Project**

\n\nUse the Azure DevOps CLI or REST API to create a new project named `translate-workflow-to-pipeline`
\n\nConfigure the project with the private visibility setting
\n\nEnsure the project is ready to accept pipeline definitions

\n\n**Validate Project Creation**

\n\nConfirm the project exists and is accessible
\n\nRetrieve the project ID for subsequent pipeline operations

\n\n**Translate and Deploy Pipeline**

\n\nRead the GitHub Actions workflow file from the specified path
\n\nConvert all workflow steps, jobs, and triggers to Azure DevOps pipeline syntax
\n\nCreate the Azure DevOps pipeline YAML in the `.azure-pipelines` directory
\n\nPush the pipeline definition to the newly created project

\n\n**Verify Pipeline Configuration**

\n\nValidate the generated pipeline YAML syntax
\n\nConfirm all variables, secrets, and dependencies are properly configured
\n\nTest pipeline creation in the Azure DevOps project

\n\nTools Required

\n\n`azuredevops/project/create` - Create the Azure DevOps project
\n\n`azuredevops/pipeline/create` - Create the pipeline from YAML
\n\n`github/file/read` - Read the source GitHub Actions workflow
\n\n`github/file/write` - Write the translated pipeline to repository

\n\nSuccess Criteria

\n\nAzure DevOps project `translate-workflow-to-pipeline` is successfully created
\n\nPipeline YAML is translated and saved to `.azure-pipelines/01-level-pipeline.yml`
\n\nPipeline is registered in the Azure DevOps project and ready for execution

\n
