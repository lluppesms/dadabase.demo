---
mode: agent
model: Claude Sonnet 3.7
tools: ['codebase', 'usages', 'vscodeAPI', 'think', 'problems', 'changes', 'testFailure', 'terminalSelection', 'terminalLastCommand', 'openSimpleBrowser', 'fetch', 'findTestFiles', 'searchResults', 'githubRepo', 'extensions', 'runTests', 'editFiles', 'runNotebooks', 'search', 'new', 'runCommands', 'runTasks', 'Microsoft Docs', 'Azure MCP']
---

# User Input Phase
The application should be hosted using an Azure App Service and deployed using Bicep Templates.  The data should be stored in a serverless Cosmos database.

Create two files under the root-folder/reports: Report-Status.md and Application-Assessment-Report.md
  - Make the Report-Status.md and Application-Assessment-Report.md look pretty and easy to read, using headings, bullet points, and other formatting options as appropriate.

  - Those files must be created and must have the collected information from the user and a high-level plan that will be used by the Phase2-AssessProject.prompt.md

Suggest that the next step is to do the assessment, and mention `/phase2-assessproject` is the command to start the migration process.

## Agent Role

The agent will guide the user through the migration process by asking targeted questions and collecting necessary information. After gathering the required details, the agent provides recommendations aligned with Azure best practices.
