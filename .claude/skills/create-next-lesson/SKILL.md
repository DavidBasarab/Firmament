---
name: create-next-lesson
description: Author the next Firmament lesson from the README curriculum and update the README progress. Use when the user says "create the next lesson", "next lesson", "make the next lesson", or invokes /create-next-lesson. Marks the previously-latest lesson Done, authors the next uncreated lesson's markdown file (built on the real committed code, not prior lesson docs), links it in the lesson log, and checks a Part's Progress box once all its lessons are Done.
---

# Create the Next Firmament Lesson

This repo is a lesson-by-lesson game-engine curriculum. `README.md` is both the lesson plan and the teaching contract. This skill advances it by one lesson.

Asking for "the next lesson" is the signal that the current latest lesson is **finished** — so this skill marks that one Done, then authors the next one.

## What this skill does NOT do

- It does **not** write lesson code into the source projects. Per the README teaching rules ("I type the code"), you author the lesson *document* only — the user types the code themselves. If the user explicitly asks you to also apply the code, that's a separate follow-up.
- It does **not** write ahead. One lesson = one concept. No abstractions the current lesson doesn't need.

## Procedure

### 1. Read the curriculum and rules

Read `README.md` in full — the curriculum list, the "Rules for Claude" teaching section, and the Progress + Lesson log at the bottom. The lesson you author must obey those rules (concept-first ~150–400 words, explain graphics/math not C#, stay in scope, end with Checkpoint / Common failure modes / Exercises / What this unlocks).

### 2. Figure out the state

- List `lessons/*.md`. Their filenames start with the lesson number (e.g. `1.2-...md`). The **latest authored lesson** is the highest-numbered one present.
- Parse the curriculum's ordered lesson list (`**x.y** Title` under each `## Part N`). The **next lesson** is the first curriculum entry that has no file in `lessons/`.
- If every curriculum lesson already has a file, stop and tell the user the curriculum is fully authored — there is nothing to create.

### 3. Read the ACTUAL code before writing (do not skip)

README teaching rule 6: the user tweaks things, and earlier lesson docs may not match what's committed. Before writing, read the real source files the new lesson builds on (the `Firmament.*` projects, `.csproj` files, `Directory.Build.props`, `GlobalUsings.cs`). Note any divergence between what a prior lesson doc handed out and what's actually in the repo, and build the new lesson on **the committed reality** (namespaces, project names, GL version, target framework, package versions). Flag notable divergences inline in the lesson so they aren't a mystery to the reader.

### 4. Verify every API and toolchain fact

No mystery numbers, no guessed signatures. Confirm any API surface you teach against what's actually installed — reflect over the OpenTK DLLs in `bin/`, check package versions in the `.csproj`, check the target framework. Follow the project's PowerShell rule when running commands: `pwsh -Command '. $PROFILE; ...'`. Every magic constant, GL enum, and buffer offset gets a one-line reason.

### 5. Author the lesson file

Create `lessons/{x.y}-{kebab-title}.md`, where the slug is a short kebab-case description of the lesson topic (match the style of existing files, e.g. `1.2-the-game-loop-and-delta-time.md`).

Match the structure and voice of the already-authored lesson files:

- Title line `# Lesson x.y — Title`, then a blockquote with the Part name and the curriculum's one-line description.
- A **"Where you already are"** orienting paragraph grounded in the real committed code.
- **Concept** first (~150–400 words), plain language, graphics/math explained geometrically before algebraically. Do not explain C#.
- Steps as **what to write and where** — small files shown in full when new, diffs when edits — with a per-value justification table for any non-obvious constants or settings.
- Keep it strictly **in scope** for this one lesson. Name which later lesson handles anything you deliberately defer.
- End with the four required sections: **✅ Checkpoint**, **🐛 Common failure modes** (ordered by likelihood), **🔧 Exercises** (2–4, easy → interesting), **📎 What this unlocks** (one line).
- If the README contradicts the committed code, add a short **⚠️ Stale README** note (code is authoritative). Don't re-flag the same staleness a prior lesson already flagged unless it changed.
- Finish with a `## Notes / what I changed` section containing `<!-- your notes here -->`.

### 6. Update the README progress

Done-status is tracked in **two** places — keep them in sync:

- **Curriculum list** (under each `## Part N`): every lesson is a checkbox, `- [ ] **x.y** ...`. Mark the *previously latest* lesson (the highest-numbered one that had a file **before** this run) Done by flipping its `- [ ]` to `- [x]`. The lesson you just authored stays `- [ ]` — creating it is not completing it.
- **Lesson log** (bottom of README): a checkbox list, one item per lesson, `- [ ] [x.y — Title](lessons/x.y-slug.md)`. Flip the same previously-latest lesson to `- [x]`, and add a new **unchecked** item for the lesson you just created, linked to its file with its title.
- **Part Progress checkboxes:** if marking that lesson Done means *every* lesson in its Part is now `- [x]`, check that Part's box in the Progress list (`- [x] Part N — ...`). Otherwise leave it unchecked.

Edge case — first run after only `1.1` exists: the latest authored lesson is `1.1`, so mark `1.1` Done and author `1.2`.

### 7. Report

Tell the user, concisely:
- Which lesson you marked Done and which you created (with the file path).
- Any code divergence or stale-README issue you found and how you handled it.
- The reminder that you authored the doc only — they type the code — and that they can ask you to apply the code if they want.
