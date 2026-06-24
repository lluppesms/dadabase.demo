# Copilot Instructions

The github repo is lluppesms/dadabase.demo and the primary branch that I work off of is main.

## ⚠️ Git Branch Policy — AGENTS MUST FOLLOW THIS

Do not commit or push changes unless directly instructed by the user.  If instructed to commit, then follow these guidelines.

**NEVER commit directly to `main` or `master`.** This is a strict rule for all agents and automated tools.

Before making any commits or file changes:
1. **Check the current branch**: `git branch --show-current`
2. **If on `main` or `master`, create and switch to a feature branch first**:
   ```
   git checkout -b feature/short-description-of-task
   ```
3. **All work must be committed to the feature branch**, not to `main`/`master`.
4. When finished, open a Pull Request targeting `main` — do not merge directly.

Branch naming convention: `feature/short-description`, `fix/short-description`, or `chore/short-description`.

The human owner will review and merge PRs into `main`. Agents do not have permission to merge.

## File Organization
- Keep related files together
- Use meaningful file names
- Follow consistent folder structure
- Group components by feature when possible

## Project Structure
- Any actual source code should be located in the src folder. Organize each project into its own folder within src.
- Any infrastructure code should be located in the infra folder, and put each type of IaC code into it's own folder, such as Bicep in the infra/bicep folder and Terraform in the infra/tf folder.
- Any code for the GitHub Actions workflows should be located in the .github/workflows folder.
- Any code for Azure DevOps pipelines should be located in the .azuredevops/pipelines folder.
- Keep documentation and images in a Docs folder.

## Blazor & CSS

When making changes to Blazor components or CSS, refer to [.github/instructions/blazor-css-instructions.md](.github/instructions/blazor-css-instructions.md) for detailed guidelines on component structure, scoped CSS, theming, and CSS best practices.

## C# Code Style

When writing or modifying C# code, refer to [.github/instructions/csharp-code-style-instructions.md](.github/instructions/csharp-code-style-instructions.md) for naming conventions, `using` directive organization, namespace structure, and folder layout.

## Bicep Infrastructure

When writing or modifying Bicep IaC files, refer to [.github/instructions/bicep-instructions.md](.github/instructions/bicep-instructions.md) for module structure, naming, comments, and Azure best practices.

## GitHub Actions & Azure DevOps Pipelines

When creating or modifying CI/CD pipeline files, refer to [.github/instructions/pipeline-instructions.md](.github/instructions/pipeline-instructions.md) for workflow structure, secrets handling, action pinning, and reuse patterns.

## Testing

When writing tests, refer to [.github/instructions/testing-instructions.md](.github/instructions/testing-instructions.md) for test framework, project location, and scope guidelines.

## General Best Practices

For error handling, performance, security, accessibility, and documentation standards, refer to [.github/instructions/general-best-practices-instructions.md](.github/instructions/general-best-practices-instructions.md).

---

Apply these conventions when generating new code, infrastructure, or workflow files to ensure consistency with the existing project style.
