---
name: write-skill
description: Creates new Claude Code skills (SKILL.md files) from task descriptions. Use when the user wants to create a skill, write a skill, add a slash command, automate a workflow, or make a repeatable task into a skill.
argument-hint: <skill-name> <one-line description>
---

# Write a New Skill

Create a new Claude Code skill by writing a `SKILL.md` file that teaches Claude how to perform a repeatable task. For the full official best practices, see: https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices

## Input

`$ARGUMENTS` should contain the skill name and a brief description, e.g. `rename-vars Rename decompiled shader variables to meaningful names`. If not provided, ask the user what task the skill should automate.

Parse the arguments as:
- **First token**: the skill name (kebab-case, e.g. `rename-vars`)
- **Remaining tokens**: a one-line description of what the skill does

If the user hasn't provided enough detail about what the skill should do, ask clarifying questions before writing:
- What is the input? (file path, directory, etc.)
- What transformation or task does it perform?
- What is the expected output?
- Are there any constraints or things it should NOT do?

## Skill File Format

Every skill lives at `.claude/skills/<skill-name>/SKILL.md` and follows this structure:

### Frontmatter (YAML)

```yaml
---
name: <skill-name>
description: <What the skill does AND when to use it. Max 1024 chars.>
argument-hint: <hint showing expected arguments, e.g. "<input-file.ext>">
---
```

**Frontmatter field reference** (all optional except `description` which is recommended):

| Field | Description |
|-------|-------------|
| `name` | Lowercase letters, numbers, hyphens only. Max 64 chars. No reserved words ("anthropic", "claude"). Defaults to directory name. |
| `description` | What the skill does AND when to use it. Written in **third person**. Max 1024 chars. |
| `argument-hint` | Hint shown during autocomplete, e.g. `[issue-number]` or `<filename>`. |
| `disable-model-invocation` | `true` to prevent Claude from auto-triggering. Use for side-effect-heavy workflows like deploy or commit. |
| `user-invocable` | `false` to hide from the `/` menu. Use for background knowledge Claude should apply automatically. |
| `allowed-tools` | Tools Claude can use without asking permission when this skill is active, e.g. `Read, Grep, Glob`. |
| `context` | `fork` to run in an isolated subagent context. |
| `agent` | Subagent type when `context: fork` is set (`Explore`, `Plan`, `general-purpose`, or a custom agent). |
| `model` | Model to use when this skill is active. |

### Body (Markdown)

Use this section template. Include only sections relevant to the skill:

```markdown
# <Title>

<1-2 sentence summary of what the skill does.>

## Input

`$ARGUMENTS` is <describe what the user passes>. If not provided, ask the user.

## Background

<Optional. Explain WHY this task exists — what problem it solves, what context
is needed to understand the transformation.>

## Process

### 1. <First step>
<What to read, parse, or identify.>

### 2. <Second step>
<The core transformation or analysis.>

### 3. <Third step>
<Writing output, cleanup, etc.>

## What NOT to change

<Explicit guardrails. List things that are out of scope or must be preserved.>

## Output

<Describe the expected output: file paths, formats, structure.>

## Verification

<How to confirm the skill completed correctly. Actionable checks only.>
```

### Supporting files

Skills can include additional files in their directory. Reference them from `SKILL.md` so Claude knows what they contain and when to load them:

```
my-skill/
├── SKILL.md           # Main instructions (required, keep under 500 lines)
├── reference.md       # Detailed docs (loaded on demand)
├── examples.md        # Example input/output pairs
└── scripts/
    └── helper.py      # Utility script (executed, not loaded into context)
```

Keep references **one level deep** from SKILL.md. Avoid chains like SKILL.md → advanced.md → details.md.

## Writing Guidelines

### Write a "pushy" description

Claude tends to under-trigger skills. Descriptions should include what the skill does AND when to use it, with key trigger terms:

```yaml
# Good — includes trigger phrases
description: Extracts text and tables from PDF files, fills forms, merges documents. Use when working with PDF files or when the user mentions PDFs, forms, or document extraction.

# Bad — too vague
description: Helps with documents
```

Always write descriptions in **third person** ("Processes files..." not "I can help you process files...").

### Be concise — Claude is already smart

Only add context Claude doesn't already have. Challenge each paragraph: "Does Claude really need this explanation?" Don't explain what PDFs are; do explain your project's specific naming conventions.

### Be specific and concrete

- Use real examples from the codebase where possible (file paths, code snippets, naming patterns)
- Show input/output examples for non-obvious transformations
- Specify exact naming conventions, file locations, and formats

### Constrain scope tightly

- Each skill should do ONE well-defined thing
- Include a "What NOT to change" section to prevent scope creep
- If the user's description implies multiple skills, suggest splitting them and write only one

### Match freedom to fragility

- **High freedom** (text instructions) for tasks where multiple approaches are valid
- **Medium freedom** (pseudocode/templates) when a preferred pattern exists
- **Low freedom** (exact scripts) when operations are fragile and error-prone

### Use `$ARGUMENTS` for input

- Always document what `$ARGUMENTS` should contain
- Always include a fallback: "If not provided, ask the user"
- Use `$ARGUMENTS[0]`, `$ARGUMENTS[1]` or `$0`, `$1` for positional access

### Keep verification actionable

- Each verification bullet should be something Claude can actually check (grep for a pattern, count files, compare line counts)
- Avoid subjective checks like "looks correct"

### Use consistent terminology

Pick one term for each concept and use it throughout. Don't alternate between "endpoint", "URL", "route", and "path" for the same thing.

### Reference other skills when relevant

If the skill builds on output from another skill, name it (e.g. "produced by the `merge-variants` skill"). This establishes pipeline order without duplicating instructions.

## Process

### 1. Parse the arguments

Extract the skill name and description from `$ARGUMENTS`.

### 2. Discuss scope with the user

If the description is brief, have a short conversation to understand:
- The exact input and output
- The transformation steps
- Edge cases and constraints
- Whether it relates to existing skills in the pipeline

### 3. Draft the SKILL.md

Write the skill file following the format and guidelines above. Place it at `.claude/skills/<skill-name>/SKILL.md`.

### 4. Review with the user

Present a summary of the skill:
- Name and description
- What it takes as input
- What it produces as output
- Key constraints

Ask if anything needs adjustment before finalizing.

## What NOT to do

- Do NOT create skills that are too broad (e.g. "refactor everything") — split into focused skills
- Do NOT duplicate logic that already exists in another skill — reference it instead
- Do NOT add the skill to any registry or index file — `.claude/skills/` is auto-discovered
- Do NOT use reserved words "anthropic" or "claude" in skill names
- Do NOT include time-sensitive information (dates, versions that will expire)

## Verification

- The file exists at `.claude/skills/<skill-name>/SKILL.md`
- The frontmatter has valid `name` (max 64 chars, lowercase/numbers/hyphens only) and `description` (max 1024 chars)
- The description is written in third person and includes when-to-use trigger phrases
- The body has at minimum: a title, Input section, Process section, and Verification section
- `$ARGUMENTS` is documented with a fallback
- SKILL.md is under 500 lines; detailed content is in supporting files
- The skill is focused on a single well-defined task
