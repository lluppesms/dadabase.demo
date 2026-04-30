---
applyTo: "infra/bicep/**"
---

# Bicep Infrastructure Code Instructions

- Use parameterized modules for reusable infrastructure (e.g., `containerApp.bicep`, `containerRegistry.bicep`)
- Organize Bicep files into logical folders (e.g., `infra/bicep`)
- Use descriptive parameter and variable names in `snake_case`
- Include comments to explain resource purpose and configuration
- Use outputs for key resource values
- Follow Azure best practices for resource naming and tagging
- Add YAML or Bicep comments to explain configuration choices
