---
name: commit
description: Stages and commits changes with a well-crafted commit message matching the project's style. Use when the user says "commit", "save changes", "create a commit", or wants to commit their work.
disable-model-invocation: true
context: fork
allowed-tools: Bash, Read, Grep, Glob
---

# Commit Changes

Stage and commit changes with a descriptive, well-crafted commit message that matches
this repo's existing style.

## Input

`args` is an optional commit message hint or description of the changes. If not provided,
infer the message from the diff.

## Process

### 1. Gather context (run in parallel)

- `git status` — see what files are modified, staged, and untracked (never use `-uall`)
- `git diff` and `git diff --cached` — see both unstaged and staged changes
- `git log --oneline -15` — see recent commit message style
- `git branch --show-current` — **required**; see the branch guard below

If there are no changes to commit, inform the user and stop.

### 2. Branch guard (CRITICAL)

**Derive the branch names — never assume them.** These repos use two different naming schemes,
so a hardcoded list silently fails to protect the repos using the other one:

```bash
BRANCHES=$(git branch -a --format='%(refname:short)' | sed 's|^origin/||' | sort -u)
if grep -qx 'redux/development' <<<"$BRANCHES"; then
  DEV=redux/development; RELEASED=redux/master; ARCHIVE=master
else
  DEV=development;       RELEASED=main;         ARCHIVE=pre-redux
fi
echo "released=$RELEASED  development=$DEV  archive=$ARCHIVE"
```

`$RELEASED`, `$DEV` and `$ARCHIVE` are **protected**. Changes reach them **only by merging a
feature/bugfix branch** — never by a direct commit. The archive holds the frozen pre-Redux
(SpaceWarp-1.x) codebase and takes no new work at all.

- If the current branch is `$RELEASED`, `$DEV`, or `$ARCHIVE`, **do NOT commit there.**
  Automatically create a new feature/bugfix branch cut from the current branch and commit on
  it — **do not ask the user first.** Derive a short, descriptive branch name from the change
  (`feature/<short-name>` for new work, `bugfix/<short-name>` for a fix), e.g.
  `git checkout -b feature/<short-name>`. Tell the user which branch you created.
- If already on a feature/bugfix branch, just commit there.

### 3. Draft the commit message

Write a **concise, single-line** commit message that captures the intent of the change.
Match the imperative mood of the recent `git log` output.

- Imperative mood, focused on the *why*/intent — not a list of changed lines.
- 1 line, aim for ≤ 72 characters.
- Add a short body only when the change genuinely needs explanation the subject can't carry;
  most commits are subject-only.
- Real examples from this repo:
  - `Replace SASManager.SetRotation's 300-line switch with a direction registry`
  - `Guard three post-wiring dereferences the WireSection refactor left unguarded`
  - `Fix GetParentStar returning null -> uncaught per-tick NRE in star pointing modes`
  - `Remove unused property`

### 4. Stage files

- Stage relevant changed files by name (`git add <file1> <file2> ...`)
- Do NOT use `git add -A` or `git add .`
- Do NOT stage generated/build artifacts (`Library/`, `Temp/`, `obj/`, `*.csproj`, `*.sln`,
  `ThunderKit/`, `pm_cache/`, `Redux/`) or files that may contain secrets (`.env`,
  `credentials.json`, etc.)
- If unsure which files to include, ask the user

### 5. Create the commit

Use a HEREDOC (Bash tool) to pass the message. End with the Co-Authored-By trailer for the
Claude model in use — recent history uses `Claude Sonnet 5`; use `Claude Opus 4.8` when
running as Opus, etc.

```bash
git commit -m "$(cat <<'EOF'
<concise single-line message>

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>
EOF
)"
```

### 6. Verify

Run `git status` and `git log --oneline -1` after the commit to confirm it succeeded.

## What NOT to do

- Do NOT commit directly on the released, development, or archive branch — auto-create a
  feature/bugfix branch first (see the branch guard). Derive their names; do not assume
  `main`/`development` — most of these repos use `redux/master`/`redux/development`.
- Do NOT push to remote unless the user explicitly asks (opening a PR against the development
  branch is a separate, user-initiated step; the user merges manually)
- Do NOT amend existing commits unless the user explicitly asks
- Do NOT use `--no-verify` or skip hooks
- Do NOT stage secrets, credentials, or generated/build artifacts
- Do NOT create empty commits
- Do NOT use interactive git flags (`-i`)

## Verification

- Current branch is a feature/bugfix branch, not the derived released/development/archive branch
- `git status` shows a clean working tree (or only intentionally unstaged files remain)
- `git log --oneline -1` shows the new commit with the expected message
