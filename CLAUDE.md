# Firmament Coding Standards

This file defines the coding standards for the Firmament codebase.
All C# code you generate — in any context — must follow the rules below.
The goal is that AI-generated code is indistinguishable from code written by a senior engineer on this project.

## ⚠️ READ THIS FIRST — Before Any Work

**Before doing anything else in this repository — reading, planning, or writing code — read [`README.md`](README.md) in full.**
It is both the product description and the teaching curriculum for this repo. Do not skip it, even for a "small" change.

The README tells you what you must know before you touch a line of code:
- **Project:** Firmament — a 2D/3D game engine built from scratch in C# with OpenTK and raw OpenGL, one lesson at a time. No Unity, no Godot, no engine framework.
- **How this repo is driven:** the README is the lesson plan *and* the prompt. The owner works lesson-by-lesson (e.g. "Lesson 1.1"). **Stay in scope** — one lesson teaches one concept. Do not write ahead, do not add abstractions the current lesson does not need, and do not "helpfully" scaffold future lessons.
- **Audience:** the owner is a Sr. Software Developer / CTO with 20+ years of C#. Do not explain C#. Do explain graphics-pipeline concepts and vector/matrix math, per the teaching rules in the README.

Where the README and the actual code disagree, **the code is authoritative** — trust the code and flag the stale README.

## Projects

- `Firmament.Core` — the engine library (`Firmament.Core.*` namespaces). OpenTK / OpenGL lives here.
- `Firmament.Game` — the console entry point that runs the engine (`Firmament.Game.*` namespaces).
- `Firmament.slnx` — the solution.

All production namespaces start with `Firmament.*`.

---

## C# Rules

Apply these rules to all C# code.

@.claude/rules/csharp/naming-and-structure.md
@.claude/rules/csharp/types-and-di.md
@.claude/rules/csharp/toolchain.md
@.claude/rules/csharp/async.md
@.claude/rules/csharp/errors-and-logging.md
@.claude/rules/csharp/not-allowed.md
