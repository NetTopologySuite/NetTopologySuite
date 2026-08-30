# Agent Instructions

These are baseline rules for any AI agent (Copilot, ChatGPT, etc.) working in this repository, whether run interactively by a contributor or autonomously (e.g. as a coding agent on a PR).

## Disclosure

- **Any AI-assisted contribution must be disclosed.** Say so explicitly in the PR description and/or commit message (e.g. "This PR was drafted with AI assistance"). See [AI_POLICY.md](AI_POLICY.md).
- Disclosure does not replace review: the human submitting the PR remains fully responsible for correctness, licensing, and quality.

## Project rules

- Read [CONTRIBUTING.md](CONTRIBUTING.md) before making changes; it defines the project's scope and conventions.
- `NetTopologySuite` (the core project) tracks JTS Topology Suite behavior 1:1 — do not change core algorithmic behavior to "fix" something unless it's an actual bug; non-breaking extra functionality is fine.
- Make the smallest change that accomplishes the task. No unrelated refactors, renames, or reformatting ("no resharping").
- Do not break the public API. Prefer additive changes; flag any breaking change explicitly instead of making it silently.
- Follow standard .NET capitalization/naming conventions.
- Document all public/protected classes, methods, and properties with XML doc comments.
- Add or update unit tests for any behavior change or bug fix — don't submit changes without test coverage.
- Keep changes scoped to the stated task; consider downstream consumers, since NTS is used in many different projects with different needs.

## Before finishing

- Build and run the relevant test project(s) for anything you touched; don't leave the build or tests broken.
- Summarize what changed and why, and call out any AI involvement per the disclosure rule above.
