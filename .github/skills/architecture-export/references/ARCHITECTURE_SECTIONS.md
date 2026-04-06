# Architecture Document Sections

Defines the 19 canonical sections of the Architecture document (`docs/DadABase-Architecture-v{version}.md`). Each section lists its data sources, key content elements, and change detection logic used by the architecture-export skill.

---

## How to Use This Reference

During **Step 3 (Detect Changes)** of the architecture-export workflow, the skill reads each section's data sources, extracts current values, and compares them against what the architecture doc currently describes. Sections are flagged as:

- **🟢 Current** — No changes detected
- **🟡 Stale** — Minor updates (counts, values, new list items)
- **🔴 Outdated** — Structural changes, new features, removed components

---

## Section Catalog

### Section 1 — System Overview

| Property | Value |
|----------|-------|
| **Heading** | `## 1. System Overview` |
| **Key content** | System purpose, core metaphor (mountain climbing), persona count, key capabilities |
| **Change detection** | Compare persona count, feature list, agent description text |
| **Slide layout** | Content |

---

### Section 2 — Architecture Layers

| Property | Value |
|----------|-------|
| **Heading** | `## 2. Architecture Layers` |
| **Key content** | Layer diagram (Agent → Skills → MCP Tools), layer descriptions, skill registry |
| **Slide layout** | Diagram |

List all directories under `.github/skills/` to detect new or removed skills.

---

### Section 3 — File Organization

| Property | Value |
|----------|-------|
| **Heading** | `## 6. File Organization` |
| **Key content** | Directory tree, fiscal year/quarter structure, file naming conventions, key files at repo root |
| **Change detection** | Compare directory listing, check for new top-level files or directories, verify FY/Q structure |
| **Slide layout** | Content or Two-Column |

---

### Section 8 — Role Impact Categories (7 Total)

| Property | Value |
|----------|-------|
| **Heading** | `## 8. Role Impact Categories (7 Total)` |
| **Data sources** | `.github/skills/sherpa-logbook/references/LOGBOOK_SCHEMA.md`, `.github/agents/csa-sherpa.agent.md` |
| **Key content** | 7 categories: technical-design, customer-delivery, business-development, thought-leadership, team-enablement, learning, operational |
| **Change detection** | Compare category list and definitions, check for additions or removals |
| **Slide layout** | Two-Column |

---

### Section 9 — CSA Impact Story Types

| Property | Value |
|----------|-------|
| **Heading** | `## 9. CSA Impact Story Types (for Weekly Journeys)` |
| **Data sources** | `.github/skills/sherpa-logbook/SKILL.md` (journey workflow), `.github/skills/sherpa-logbook/references/LOGBOOK_SCHEMA.md` |
| **Key content** | Impact story categories used in weekly journey generation, story structure and examples |
| **Change detection** | Compare story type list, check journey template for new story formats |
| **Slide layout** | Content |

---

### Section 10 — Persona System

| Property | Value |
|----------|-------|
| **Heading** | `## 10. Persona System` |
| **Data sources** | `sherpa-personas.yaml`, `.github/skills/sherpa-admin/references/PERSONA_GUIDE.md`, `csa-profile.yaml` |
| **Key content** | 6 personas (Mountain Sherpa, Trail Runner, Alpine Climber, Backcountry Hiker, Base Camp Commander, Park Ranger), voice/tone differences, shared logbook terminology |
| **Change detection** | Count personas in `sherpa-personas.yaml`, compare persona names and descriptions |
| **Slide layout** | Table |

---

### Section 11 — Onboarding Wizard

| Property | Value |
|----------|-------|
| **Heading** | `## 11. Onboarding Wizard` |
| **Data sources** | `.github/skills/sherpa-admin/references/ONBOARDING_WIZARD.md`, `.github/skills/sherpa-admin/references/ONBOARDING_SETUP.md` |
| **Key content** | Wizard steps, profile creation flow, environment setup, prerequisite checks |
| **Change detection** | Compare wizard step count and sequence, check for new setup requirements |
| **Slide layout** | Content |

---

### Section 12 — Calendar Categories

| Property | Value |
|----------|-------|
| **Heading** | `## 12. Calendar Categories` |
| **Data sources** | `.github/skills/sherpa-logbook/SKILL.md` (calendar section), `sherpa-config.yaml` |
| **Key content** | Calendar category definitions, color mapping, how categories influence peak/valley classification |
| **Change detection** | Compare category list in config, check for new categories or changed mappings |
| **Slide layout** | Table |

---

### Section 13 — Satchel (M365 Asset Registry)

| Property | Value |
|----------|-------|
| **Heading** | `## 13. Satchel (M365 Asset Registry)` |
| **Data sources** | `sherpa-satchel.yaml`, `.github/skills/sherpa-logbook/references/SATCHEL_GUIDE.md` |
| **Key content** | Asset types (OneNote, SharePoint, Teams), mode system (always/contextual), how assets ground WorkIQ queries |
| **Change detection** | Compare asset type list, check mode definitions, count registered assets |
| **Slide layout** | Two-Column |

---

### Section 14 — Known Limitations

| Property | Value |
|----------|-------|
| **Heading** | `## 14. Known Limitations` |
| **Data sources** | `.github/skills/sherpa-logbook/SKILL.md`, `sherpa-admin-tasks.md`, `CHANGELOG.md` |
| **Key content** | Current system limitations, workarounds, planned fixes |
| **Change detection** | Check admin tasks for resolved items, scan CHANGELOG for limitation fixes, compare limitation list |
| **Slide layout** | Content |

---

### Section 15 — Integration Points

| Property | Value |
|----------|-------|
| **Heading** | `## 15. Integration Points` |
| **Data sources** | `.mcp.json`, `.github/skills/*/SKILL.md`, `DEPENDENCIES.md` |
| **Key content** | MCP servers (MSX-MCP, WorkIQ, Fabric/Power BI), external service connections, data flow between integrations |
| **Change detection** | Compare MCP server list in `.mcp.json`, check for new skill integrations, verify dependency graph |
| **Slide layout** | Diagram |

---

### Section 16 — Configuration

| Property | Value |
|----------|-------|
| **Heading** | `## 16. Configuration` |
| **Data sources** | `sherpa-config.yaml`, `csa-profile.yaml`, `sherpa-personas.yaml`, `sherpa-satchel.yaml`, `sherpa-logbook/account-registry.yaml` |
| **Key content** | All configuration files, their purpose, key settings, update procedures |
| **Change detection** | Compare config file list, check for new config keys or removed settings |
| **Slide layout** | Table |

---

### Section 17 — Version Control

| Property | Value |
|----------|-------|
| **Heading** | `## 17. Version Control` |
| **Data sources** | `CHANGELOG.md`, `DEPENDENCIES.md`, `sherpa-config.yaml` (sherpa_version) |
| **Key content** | Semantic versioning rules (MAJOR/MINOR/PATCH), version bump workflow, change management process, version stamp locations |
| **Change detection** | Compare current version in `sherpa-config.yaml`, check for version control process changes in CHANGELOG |
| **Slide layout** | Content |

---

### Section 18 — Version History

| Property | Value |
|----------|-------|
| **Heading** | `## 18. Version History` |
| **Data sources** | `CHANGELOG.md` |
| **Key content** | Condensed version history table (version, date, highlights), recent changes summary |
| **Change detection** | Compare latest version in CHANGELOG against architecture doc's recorded history |
| **Slide layout** | Table |

---

### Section 19 — Reserved for Expansion

| Property | Value |
|----------|-------|
| **Heading** | `## 19. (Reserved)` |
| **Data sources** | None |
| **Key content** | Placeholder section for future architecture topics |
| **Change detection** | N/A — always 🟢 Current unless content is added |
| **Slide layout** | N/A |

This section is reserved for future expansion. When a new architecture topic warrants a dedicated section, it is added here before extending the section count.

---

## Data Source Summary

All files scanned during architecture document refresh:

| File / Path | Sections |
|-------------|----------|
| `.github/agents/csa-sherpa.agent.md` | 1, 2, 8 |
| `.github/skills/sherpa-logbook/SKILL.md` | 2, 4, 5, 7, 9, 12, 14 |
| `.github/skills/*/SKILL.md` | 2, 15 |
| `.github/skills/sherpa-logbook/references/LOGBOOK_SCHEMA.md` | 3, 4, 5, 8, 9 |
| `.github/skills/sherpa-logbook/references/SHERPA_TERMINOLOGY.md` | 3 |
| `.github/skills/sherpa-admin/references/PERSONA_GUIDE.md` | 10 |
| `.github/skills/sherpa-admin/references/ONBOARDING_WIZARD.md` | 11 |
| `.github/skills/sherpa-admin/references/ONBOARDING_SETUP.md` | 11 |
| `.github/skills/sherpa-logbook/references/HOK_TRACKING.md` | 7 |
| `.github/skills/sherpa-logbook/references/SATCHEL_GUIDE.md` | 13 |
| `csa-profile.yaml` | 1, 10, 16 |
| `sherpa-personas.yaml` | 10, 16 |
| `sherpa-config.yaml` | 1, 12, 16, 17 |
| `sherpa-satchel.yaml` | 13, 16 |
| `sherpa-logbook/` (directory structure) | 6 |
| `sherpa-logbook/account-registry.yaml` | 16 |
| `sherpa-tasks.md` | 14 |
| `sherpa-admin-tasks.md` | 14 |
| `CHANGELOG.md` | 14, 17, 18 |
| `DEPENDENCIES.md` | 15, 17 |
| `.mcp.json` | 2, 15 |

---

## Change Detection Process

For each section during Step 3 of the workflow:

1. **Read data sources** — Load all files listed for the section
2. **Extract current values** — Parse counts, lists, names, definitions from source files
3. **Read architecture doc section** — Extract the same data points from the existing doc
4. **Compare** — Diff values, counts, and structure
5. **Classify** — Apply flag based on delta severity:
   - 🟢 **Current**: All values match (or trivial whitespace/formatting only)
   - 🟡 **Stale**: Values differ but structure unchanged (e.g., count went from 5→6, new list item added)
   - 🔴 **Outdated**: Structure changed (e.g., new subsection needed, feature removed, workflow steps changed)
6. **Log delta** — Record what specifically changed for the Step 10 completion report