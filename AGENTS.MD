## AGENTS Guidelines for This Repository

This repository is a Unity project. When working with an AI coding agent in this repo, follow the guidelines below.

## Generative AI Policy (Team Rules)

- **Allowed use**: Coding help only (C# scripts, logic, debugging, refactors, documentation for code).
- **Not allowed**: Any GenAI use for **Assets, Art, or Sounds** (including creating, editing, generating, or “improving” any images, audio, animations, models, VFX, or other content files).
- **Honor rule (learning first)**: Do not use GenAI the first time you solve a *type* of problem. Use GenAI only after you’ve already learned/fixed that kind of issue before (e.g., from prior work or tutorials) and you’re using it to save time.

If you’re unsure whether something counts as “asset/art/sound”, treat it as **not allowed**.

## Unity / Editor Constraints

- **Unity version**: Code must compile and run under **Unity 6000.0.50f1**.
- **Do not churn scenes/prefabs**: Avoid unrelated changes in `.unity`, `.prefab`, `.asset`, and Project Settings files. Only touch them when the task requires it.
- **Keep changes minimal**: Prefer small, reviewable diffs; avoid “drive-by” reformatting across many files.

## Coding Conventions (C#)

- **Comments**: Only add comments when intent is non-obvious or there’s a constraint/trade-off. Avoid narrating code.
- **Style**:
  - Tabs for indentation.
  - Allman braces for types/methods/blocks.
- **Naming**:
  - `PascalCase` for types and methods.
  - `camelCase` for locals/fields/parameters.
  - Keep method names focused on a single responsibility; split large multi-task methods.
- **Readability**: Keep logic straightforward; extract helpers instead of deeply nested code. Use parentheses in math/conditions when it improves clarity.

## Practical Workflow Notes

- Prefer changes in `Assets/Scripts/**` for gameplay/UI logic.
- When adding new scripts, put them in the closest existing folder by feature (don’t create new folder trees unless necessary).
- Ensure the project still builds/plays after changes (no compile errors).
