# Loading Copilot Customizations in GitHub Copilot CLI

The CLI operates from a single working directory and does not read VS Code workspace files. Each type of Copilot customization has its own mechanism for loading from an external repo:

| Content | CLI Method |
|---------|-----------|
| **Skills** | `/skills add <path>` |
| **Agents** | Not really supported...* |
| **Instructions** | Not really supported...** |
| **Prompts** | Not supported in CLI |

## Skills

Use the `/skills add` command to load the shared team skills:

```shell
/skills add /path/to/my.copilot.skills/.github/skills
```

Other useful `/skills` commands in the CLI:

| Command | Purpose |
|---------|---------|
| `/skills list` | See all currently available skills |
| `/skills add` | Add an external skills directory |
| `/skills info` | View details about a skill and its location |
| `/skills reload` | Pick up newly added skills without restarting |
| `/skills` | Interactively toggle skills on or off |
| `/skills remove DIR` | Remove a manually-added skills directory |

> **Tip:** Skills placed in `~/.copilot/skills/` are loaded as **personal skills** across all projects. Skills in a repo's `.github/skills/` directory are **project-specific**.

## Agents *

The CLI discovers agents from fixed locations — there is no `/agent add` command.  If you don't have any Agents in your repo, you **could** load shared agents by creating a symlink to an agents directory in your team repository:

```powershell
mklink /D "%USERPROFILE%\.copilot\agents" "/path/to/my.copilot.skills/.github/agents"
```

> **Note:** If you already have a `~/.copilot/agents/` folder, you'll need to merge the contents rather than symlink the entire directory.

## Instructions **

You can't really point to both instructions, but you can set the `COPILOT_CUSTOM_INSTRUCTIONS_DIRS` environment variable to point to a set of shared instructions.  

```powershell
# Current session only
$env:COPILOT_CUSTOM_INSTRUCTIONS_DIRS = "/path/to/my.copilot.skills/.github/instructions"

# Permanent (user-level)
[Environment]::SetEnvironmentVariable("COPILOT_CUSTOM_INSTRUCTIONS_DIRS", "/path/to/my.copilot.skills/.github/instructions", "User")
```

Use `/instructions` in the CLI to view and toggle loaded instruction files.
