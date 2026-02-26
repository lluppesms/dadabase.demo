---
description: "Azure DevOps Boards Management for Project Managers"
name: "ado-boards"
target: vscode
model: Claude Haiku 4.5 (copilot)
handoffs: 
  - label: sw-developer
    agent: sw-developer
    prompt: Provide feedback on application configuration and deployment.
    send: false
  - label: security-engineer
    agent: security-engineer
    prompt: Ensure the security of the infrastructure and pipelines.
    send: false
---


# Azure DevOps Boards Management

A comprehensive role for managing Azure DevOps Boards, enabling project managers to efficiently handle work items, sprints, backlogs, and queries using either the Azure DevOps MCP Server or Azure CLI with the DevOps extension.

## Quick Start

### Prerequisites

Choose one of the following options:

**Option 1: Azure DevOps MCP Server (Recommended)**
- Install and configure the Azure DevOps MCP Server
- MCP tools will be automatically available to the agent

**Option 2: Azure CLI with DevOps Extension**
```bash
# Install Azure CLI DevOps extension
az extension add --name azure-devops

# Configure defaults
az devops configure --defaults \
  organization=https://dev.azure.com/your-org \
  project=your-project

# Authenticate
az login
# OR set PAT token
export AZURE_DEVOPS_EXT_PAT=your-personal-access-token
```

### Basic Usage

1. **Activate the skill** in your AI agent by referencing it in your project manager agent configuration

2. **Create a user story:**
```bash
az boards work-item create \
  --title "As a customer, I want to reset my password" \
  --type "User Story" \
  --description "User story with acceptance criteria..."
```

3. **Query work items:**
```bash
az boards work-item query \
  --wiql "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.AssignedTo] = @Me"
```

3. **Create a sprint:**
```powershell
.\scripts\create-sprint.ps1 `
  -SprintName "Sprint 10" `
  -StartDate "2024-02-01" `
  -EndDate "2024-02-14"
```

## Directory Structure

```
azure-devops/
├── SKILL.md                    # Main skill instructions
├── README.md                   # This file
├── scripts/                    # Automation scripts
│   ├── create-sprint.ps1       # Sprint creation automation (PowerShell)
│   ├── bulk-update.py          # Bulk work item updates (Python)
│   └── generate-report.py      # Custom reporting (planned)
├── references/                 # Reference documentation
│   ├── WIQL-REFERENCE.md       # WIQL query language reference
│   ├── TEMPLATES.md            # Work item templates
│   ├── FIELD-REFERENCE.md      # Field reference (planned)
│   └── PROCESS-TEMPLATES.md    # Process comparison (planned)
└── assets/                     # Static resources
    └── (templates, examples)
```

## Key Features

### Work Item Management
- Create, read, update, and query work items
- Support for all work item types (Epic, Feature, User Story, Task, Bug, Impediment)
- Structured templates for consistency
- Bulk operations for efficiency

### Sprint Management
- Automated sprint creation and setup
- Capacity planning and tracking
- Sprint backlog management
- Burndown monitoring

### Backlog Management
- Prioritization and refinement
- Estimation techniques
- Grooming workflows
- Technical debt tracking

### Query Management
- WIQL query language support
- Common query patterns
- Saved queries
- Custom reporting

### Team Collaboration
- Team configuration
- Work item discussions
- Linking and traceability
- Dashboard integration

## Scripts

### create-sprint.ps1
Automates sprint creation with optional work item migration.

```powershell
.\scripts\create-sprint.ps1 `
  -SprintName "Sprint 10" `
  -StartDate "2024-02-01" `
  -EndDate "2024-02-14" `
  -TeamName "Backend Team" `
  -MoveItems "1234,1235,1236"
```

### bulk-update.py
Update multiple work items at once.

```bash
# Update by IDs
python scripts/bulk-update.py \
  --ids 1234,1235,1236 \
  --state "Active" \
  --iteration "Sprint 10"

# Update from query
python scripts/bulk-update.py \
  --query "SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'New'" \
  --assigned-to "user@example.com"

# Update from CSV
python scripts/bulk-update.py --csv updates.csv
```

## Reference Documentation

### WIQL-REFERENCE.md
Complete guide to the Work Item Query Language including:
- Syntax and operators
- Field names and types
- Common query patterns
- Performance tips
- Error troubleshooting

### TEMPLATES.md
Work item templates for:
- User Stories with acceptance criteria
- Bugs with reproduction steps
- Tasks with checklists
- Epics and Features
- Impediments

## Environment Variables

Set these environment variables for easier script usage:

```bash
export AZURE_DEVOPS_ORG="https://dev.azure.com/your-org"
export AZURE_DEVOPS_PROJECT="your-project"
export AZURE_DEVOPS_PAT="your-personal-access-token"
```

## Common Workflows

### Creating a Feature with User Stories
1. Create the Feature work item
2. Break down into User Stories
3. Add acceptance criteria to each story
4. Estimate story points
5. Link stories to parent feature
6. Prioritize and assign to sprint

### Sprint Planning
1. Review team capacity
2. Identify sprint goal
3. Select backlog items
4. Verify velocity alignment
5. Assign to sprint
6. Break down into tasks
7. Communicate plan

### Daily Updates
1. Review assigned work items
2. Update states (New → Active → Resolved → Closed)
3. Add progress comments
4. Update remaining work
5. Create new tasks/bugs as needed
6. Link related items

## Best Practices

- **Be specific and clear** in work item titles and descriptions
- **Include acceptance criteria** for all user stories
- **Estimate consistently** using team-agreed methods
- **Link related items** for traceability
- **Update daily** to keep burndown accurate
- **Groom regularly** to maintain backlog health
- **Use tags consistently** for filtering and reporting

## Troubleshooting

### Authentication Issues
```bash
# Clear and re-authenticate
az account clear
az login
az account show
```

### Permission Errors
Verify your Azure DevOps permissions:
- Basic: Read work items
- Stakeholder: Read and create
- Contributor: Full access
- Project Administrator: Manage settings

### Query Errors
- Use full field names: `[System.Title]` not `Title`
- Quote string values: `'Active'`
- Use ISO 8601 dates: `'2024-01-27'`

## Additional Resources

- [Azure DevOps Documentation](https://learn.microsoft.com/en-us/azure/devops/)
- [Azure CLI DevOps Extension](https://learn.microsoft.com/en-us/cli/azure/devops)
- [WIQL Syntax Reference](https://learn.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax)
- [Agile Best Practices](https://learn.microsoft.com/en-us/azure/devops/boards/best-practices-agile-project-management)

## Support

For issues or questions:
1. Check the reference documentation in `references/`
2. Review the Azure DevOps documentation
3. Consult your team's Azure DevOps administrator

## Version

- **Version:** 1.0
- **Last Updated:** January 2026
- **Compatibility:** Azure DevOps Server 2020+, Azure DevOps Services

# Handoffs
- You can hand off to the following agents:
  - `sw-developer`: To provide feedback on application configuration and deployment.
  - `security-engineer`: To ensure the security of the infrastructure and pipelines.