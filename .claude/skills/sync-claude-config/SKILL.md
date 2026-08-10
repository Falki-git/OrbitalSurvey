---
name: sync-claude-config
description: Copies the shared Claude config templates (settings.json and skills) from E:\GitHub\KSP2\.claude to every KSP2 mod repo and verifies them by hash. Use when the user says "sync claude config", "push the templates", "sync the skills", or asks whether the mod repos have drifted.
disable-model-invocation: true
argument-hint: [check]
allowed-tools: Bash, Read
---

# Sync shared Claude config to all KSP2 mod repos

Two things cannot be shared from a parent directory and must be duplicated into every mod repo:

| Template (canonical) | Copy in each repo |
|---|---|
| `E:\GitHub\KSP2\.claude\settings.template.json` | `<repo>\.claude\settings.json` |
| `E:\GitHub\KSP2\.claude\skills-template\` | `<repo>\.claude\skills\` |

Everything else shared — `CLAUDE.md` and the reference docs in `E:\GitHub\KSP2\.claude\` — exists
in exactly **one** place and is read from there. It is never copied and must never be synced.

## Input

`args` is optional. If it is `check` (or the user only asked whether things have drifted), run
**step 2 only** and report — do not copy anything.

## Process

### 1. Guard

```bash
cd "E:/GitHub/KSP2"
ls .claude/settings.template.json && ls -d .claude/skills-template
```

If either is missing, stop and tell the user — do not improvise a source. The repo list is:

```
CustomizableUIRedux KerbalkindRedux MicroEngineerRedux OrbitalSurveyRedux
SASExtendedRedux ShowKSP2EventsRedux WASDForVABRedux SkipSplashScreenRedux
```

Also list directories holding a `.claude/` folder and flag any that are **not** in that list, so a
newly added mod repo is noticed. Ignore snapshot folders whose names contain `- Copy` — those are
frozen pre-upgrade backups and must never be synced.

### 2. Check for drift (always safe, changes nothing)

```bash
cd "E:/GitHub/KSP2"
md5sum */.claude/settings.json | grep -v ' - Copy' | awk '{print $1}' | sort -u
md5sum */.claude/skills/*/SKILL.md | grep -v ' - Copy' | awk '{print $1}' | sort | uniq -c
```

Expect **one** hash for settings, and **8 of each** skill hash. Compare against the templates'
own hashes. Report precisely which repos differ; do not just say "drift detected".

If `args` was `check`, stop here.

### 3. Copy

```bash
cd "E:/GitHub/KSP2"
for d in CustomizableUIRedux KerbalkindRedux MicroEngineerRedux OrbitalSurveyRedux \
         SASExtendedRedux ShowKSP2EventsRedux WASDForVABRedux SkipSplashScreenRedux; do
  cp .claude/settings.template.json "$d/.claude/settings.json"
  rm -rf "$d/.claude/skills" 2>/dev/null
  mkdir -p "$d/.claude/skills"
  cp -r .claude/skills-template/. "$d/.claude/skills/"
done
```

**`rm -rf` on a repo's `skills/` can fail with `Device or resource busy`** when a Claude session is
running in that repo. That is expected and harmless — `cp -r` still overwrites every file. Do not
retry it, do not treat it as an error, and do not tell the user the sync failed. The only thing
that decides success is the hash check in step 4.

The one case `rm` failing does matter: a skill **deleted** from the template would survive in that
repo. Step 4 catches it as an extra file.

### 4. Verify (mandatory — never skip)

Re-run the step 2 commands and confirm one settings hash and 8 of each skill hash. Also confirm no
repo has a skill directory the template lacks:

```bash
cd "E:/GitHub/KSP2"
for d in CustomizableUIRedux KerbalkindRedux MicroEngineerRedux OrbitalSurveyRedux \
         SASExtendedRedux ShowKSP2EventsRedux WASDForVABRedux SkipSplashScreenRedux; do
  diff <(ls .claude/skills-template) <(ls "$d/.claude/skills") >/dev/null || echo "EXTRA/MISSING: $d"
done
```

Report the result plainly. If anything still differs, say so and stop — do not paper over it.

### 5. Report what needs committing

The copies are git-tracked in the six repos that are git repositories (`ShowKSP2EventsRedux` and
`WASDForVABRedux` are not). Run `git -C <repo> status --porcelain -- .claude` for each and list
which repos have changes.

**Do not commit or push.** Report the list and let the user decide, per this project's git rules.

If the templates themselves changed, note that they live in the `_claudeDocsRepo` worktree and may
also want committing there.

## What NOT to do

- Do NOT copy `CLAUDE.md` or anything else from `E:\GitHub\KSP2\.claude\` into a mod repo — only
  `settings.template.json` and `skills-template/` are duplicated. Everything else is read from the
  shared location and must stay single-copy.
- Do NOT edit a repo's `.claude/settings.json` or `.claude/skills/` directly. They are generated.
  Edit the template and re-run this skill.
- Do NOT create hard links or symlinks to "avoid the duplication". This was tried and failed:
  git rewrites files rather than editing them in place, so a branch switch silently severed the
  links while leaving the filenames and content looking correct.
- Do NOT sync into `- Copy…` snapshot folders.
- Do NOT commit or push.

## Verification

- One distinct hash across all 8 `settings.json`, matching `settings.template.json`
- 8 copies of each skill, each matching `skills-template/`
- No repo has an extra or missing skill directory
- The user has been told exactly which repos now have uncommitted changes
