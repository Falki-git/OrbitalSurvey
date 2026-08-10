---
name: open-pr
description: Opens a GitHub pull request for the current branch with a concise, release-notes-ready description. Use when the user says "open a PR", "create a pull request", "raise a PR", or wants to submit the current branch for review/merge.
disable-model-invocation: true
argument-hint: [target-branch] [title hint]
allowed-tools: Bash, Read, Grep, Glob
---

# Open a Pull Request

Open a GitHub PR for the current branch using the `gh` CLI, with a concise description built
around a release-notes-ready bullet list.

## Input

`$ARGUMENTS` is optional:
- An explicit **target/base branch** to open the PR against (overrides the default below).
- A short **title hint** describing the change.

If not provided, infer everything from the branch and its commits.

## Process

### 1. Determine head and base branches

- `git branch --show-current` — the head branch (the PR source).
- **Derive the integration branch names — never assume them.** These repos use two schemes:
  ```bash
  BRANCHES=$(git branch -a --format='%(refname:short)' | sed 's|^origin/||' | sort -u)
  if grep -qx 'redux/development' <<<"$BRANCHES"; then
    DEV=redux/development; RELEASED=redux/master; ARCHIVE=master
  else
    DEV=development;       RELEASED=main;         ARCHIVE=pre-redux
  fi
  ```
- Pick the base branch (unless the user named one in `$ARGUMENTS`):
  - On a **feature/bugfix branch** → base is **`$DEV`**.
  - On **`$DEV`** → base is **`$RELEASED`**.
  - On `$RELEASED` or `$ARCHIVE` → there is no default target; stop and ask the user.
- Always state which names you derived, so a wrong guess is visible to the user.

### 2. Check there's something to merge

Run `git log <base>..HEAD --oneline` (and `git rev-list --count <base>..HEAD`).

- **If there are no commits to pull** (HEAD is not ahead of base), **stop.** Inform the user
  that `<head>` has nothing to merge into `<base>`, and ask what they'd like to do next.
  Do not open an empty PR.

### 3. Make sure the branch is pushed

- `git push -u origin <head>` if the branch has no upstream or is ahead of its remote.
  (A PR can't be opened until the branch exists on the remote.)

### 4. Draft the PR title and body

Gather the change set with `git log <base>..HEAD` and `git diff <base>...HEAD --stat` to see
every commit and file touched.

**Title:** concise, imperative-mood summary of the branch's overall change. Prefer the user's
title hint if given.

**Body:** two parts — concise throughout.

1. A **bullet list of everything added or changed**, one bullet per notable change. This list
   is copy-pasted into release notes, so:
   - Start each bullet with a **past-tense verb**: `Added`, `Fixed`, `Changed`, `Removed`,
     `Renamed`, `Moved`, `Extracted`, `Guarded`, etc. — **not** `Add`/`Fix`/`Change`.
   - Keep each bullet to a single concise line.
   - Group by commit intent, not by file; collapse related commits into one bullet where it
     reads better for release notes.
2. **Below the list**, a short prose description **only where it adds context** the bullets
   can't carry (rationale, migration notes, caveats). Omit it entirely for self-explanatory
   changes — don't pad.

Body template:

```markdown
- Added <thing>
- Fixed <thing>
- Changed <thing>

<Optional: 1–3 sentences of extra context where a bullet needs explaining.>
```

### 5. Create the PR

Use a HEREDOC (Bash tool) for the body so formatting is preserved:

```bash
gh pr create --base <base> --head <head> --title "<title>" --body "$(cat <<'EOF'
- Added ...
- Fixed ...

<optional extra context>
EOF
)"
```

### 6. Report back

Print the PR URL that `gh pr create` returns so the user can open it.

## After the merge (user-initiated)

The user merges PRs manually and will tell you when a merge has happened — don't merge
yourself. When the user reports that a **development -> released** PR was merged, fast-forward
the development branch up to the released branch so it starts fresh for the next round:

```bash
git checkout "$DEV"
git fetch origin
git merge --ff-only "origin/$RELEASED"
git push origin "$DEV"
```

- Only do this for a **development -> released** merge — not for feature/bugfix -> development.
- This user-authorized fast-forward is the sanctioned exception to the "never touch the
  development branch directly" rule: it only moves the pointer, it is not a direct commit.
- If it can't be done as a clean `--ff-only`, **stop and tell the user** rather than forcing it.

## What NOT to do

- Do NOT open a PR when HEAD is not ahead of base (see step 2) — stop and ask instead.
- Do NOT merge the PR — the user merges manually.
- Do NOT push to or open PRs from the released or archive branch as the head branch.
- Do NOT use imperative verbs (`Add`/`Fix`) in the bullet list — always past tense
  (`Added`/`Fixed`) so it drops straight into release notes.
- Do NOT pad the prose description with restatements of the bullets.
- Do NOT invent changes not present in the `<base>..HEAD` commit range.

## Verification

- The current branch is ahead of the base branch (commits exist in `<base>..HEAD`).
- The branch is pushed to `origin` and tracks it.
- Every bullet starts with a past-tense verb and corresponds to a real change in the range.
- `gh pr create` returned a PR URL, reported back to the user.
