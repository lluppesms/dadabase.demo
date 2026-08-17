# Copilot Instructions

The GitHub repository is `lluppesms/dadabase.demo`. The primary branch is `main`.

## Living Project Document

[MAP.md](../MAP.md) is the living project document for this repository.

Read `MAP.md` before exploring broadly. Update it whenever a source project, public behavior, data flow, configuration key, test location, workflow, pipeline, skill, agent, prompt, command, or branching/build convention changes. Prompt the user to review or update `MAP.md` any time the project is altered.

## Git Branch Policy

Do not commit or push changes unless directly instructed by the user. The human owner will review and merge PRs into main. Agents do not have permission to merge.

Never commit directly to `main` or `master`. Before committing:

1. Check the current branch with `git branch --show-current`.
2. If on `main` or `master`, create a feature branch:
   `git checkout -b feature/short-description`
3. Use `feature/`, `fix/`, or `chore/` branch names.
4. Open a pull request targeting `main`; do not merge directly.

## Focused Instructions

- Blazor or CSS: [instructions/blazor-css-instructions.md](instructions/blazor-css-instructions.md)
- C#: [instructions/csharp-code-style-instructions.md](instructions/csharp-code-style-instructions.md)
- .NET projects: [instructions/dotnet-project-structure-instructions.md](instructions/dotnet-project-structure-instructions.md)
- Bicep: [instructions/bicep-instructions.md](instructions/bicep-instructions.md)
- GitHub Actions: [instructions/github-actions-instructions.md](instructions/github-actions-instructions.md)
- Azure DevOps pipelines: [instructions/azure-devops-pipeline-instructions.md](instructions/azure-devops-pipeline-instructions.md)
- SQL/DACPAC: [instructions/sql-database-dacpac-instructions.md](instructions/sql-database-dacpac-instructions.md)
- Testing: [instructions/testing-instructions.md](instructions/testing-instructions.md)
- General practices: [instructions/general-best-practices-instructions.md](instructions/general-best-practices-instructions.md)

Apply the relevant focused instructions when generating or modifying code, infrastructure, workflows, tests, or documentation.
