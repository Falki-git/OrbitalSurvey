<!--
  MASTER COPY of the per-repo CLAUDE.md stub.

  Hard-linked into every KSP2 mod repo as <repo>/CLAUDE.md, so an edit here rewrites all of them.
  Everything this file used to say in prose now lives once in ../.claude/CLAUDE.md (imported
  below): the branch-model table, how to identify the repo from the working directory, the
  "- CopyYYYYMMDD" snapshot rule, reading ProjectVersion.txt, and reading mod_specifics.md before
  feature work. Repeating any of it here would just load the same instruction into context twice.

  Deliberately generic: it names no repo, mod or branch. That is what lets one file serve every
  repo, and what keeps the dated snapshot folders correct where a hardcoded name would be wrong.
  Do not add repo-specific content here — put it in that repo's .claude/mod_specifics.md.

  This comment block is stripped before the file is injected into context, so it costs nothing.

  If a repo's copy ever diverges (a git checkout across differing content silently breaks the
  hard link), re-link it rather than editing in place:
    New-Item -ItemType HardLink -Path <repo>\CLAUDE.md `
             -Target E:\GitHub\KSP2\.claude\repo-CLAUDE-stub.md
-->

@../.claude/CLAUDE.md
