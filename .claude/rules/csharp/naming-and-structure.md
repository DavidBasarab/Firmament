# Naming & Structure

## Core Philosophy
- Follow Clean Code principles (Robert C. Martin) and SOLID.
- Methods do one thing. Classes have one responsibility.
- Code reads like prose. Names make intent obvious without reading the implementation.
- Prefer interfaces and polymorphism over if/switch chains.
- Do NOT over-engineer. Do NOT introduce abstractions that do not already exist in this codebase.
- Match the abstraction level and style of the surrounding code.
- **Stay in scope.** This repo is built lesson-by-lesson (see `README.md`). One lesson = one concept. Do not write ahead or add machinery the current lesson does not need.

## Naming Rules
- Avoid abbreviations. Prefer full words so readers never have to guess meaning.
- Acceptable abbreviations: widely recognized acronyms (e.g. `HTTP`, `URL`, `ID`, `GPU`, `VAO`, `VBO`) and any abbreviation that appears among the top 3 Google results for that term. When in doubt, use the full word.
- Names reveal intent. A method name makes it unnecessary to read the body.
- No comments explaining what code does — rename until it is obvious.
- PascalCase: classes, interfaces, methods, properties, constants
- camelCase: local variables, parameters, private fields — no leading underscore
- Private fields prefer `readonly` for dependencies where applicable
- Boolean names read as questions or states: `isReady`, `hasOutputs`, `canRestore`
- String interpolation required — never string concatenation with `+`
- Do NOT suffix method names with `Async` just because they return a `Task`. Name the method after what it does: `Load`, not `LoadAsync`. Only use the `Async` suffix when a non-async overload with the same name already exists and both must coexist.

## Discards
- Use `_` to discard outputs you intentionally do not need — `out _` for ignored out parameters, `using var _ = ...` for disposables acquired only for their side effect.

## Method Size
- Methods should be as short as possible.
- ~10 lines is a signal to evaluate refactoring — not an automatic rule.
- No method should require a comment to explain what it does. Refactor or rename instead.

## Spacing
- Leave a blank line between method definitions.
- Leave a blank line after variable declarations in a method before logic begins.
- Leave a blank line before return statements.

## Control Flow
- Avoid deep if/else nesting. Prefer guard clauses and early returns to keep the main flow readable.
- Avoid complex nested ternary expressions — prefer clear `if` statements or extract into a well-named method.
- If you need to explain what code does with a comment, first ask whether a better name makes the comment unnecessary.
- Use switch expressions (not if/else chains) when branching on an enum or type. Always include a discard arm `_` that throws `ArgumentOutOfRangeException` for unhandled cases:

```csharp
// Correct — switch expression
var stride = attribute switch
{
    VertexAttribute.Position => 3,
    VertexAttribute.Color => 4,
    VertexAttribute.TexCoord => 2,
    _ => throw new ArgumentOutOfRangeException(nameof(attribute)),
};

// Wrong — if/else chain
if (attribute == VertexAttribute.Position) stride = 3;
else if (attribute == VertexAttribute.Color) stride = 4;
```

## Files & Namespaces
- One class per file. File named after the class, never the interface.
- When a class directly implements a single interface, the interface and class live in the same file — named after the class. Do not create a separate file for the interface.
- Only create a standalone interface file when the interface has multiple implementations or is consumed without a single obvious implementation.
- Namespace must exactly match the folder path within the project. No exceptions.
- All production namespaces start with `Firmament.*` — engine code under `Firmament.Core.*` (e.g. `Firmament.Core.Rendering`, `Firmament.Core.Math`), the runnable app under `Firmament.Asteroids2D.*`.
- Always use file-scoped namespaces (C# 10+). Never use block-style `namespace X { }`.

```csharp
// Correct — file-scoped
namespace Firmament.Core.Rendering;

public class ShaderProgram { }

// Wrong — block-scoped
namespace Firmament.Core.Rendering
{
    public class ShaderProgram { }
}
```

## Interfaces
- All interfaces use the `I` prefix.
- Interface names describe a capability or action: `IRenderable`, `ILoadShader`, `IHandleInput`.
- NOT: `IShaderService` when what it does is load a shader — describe the capability, not what something "is".
- Default to narrow, single-purpose interfaces. One interface = one capability.
- Exception: a highly cohesive group of related operations may be grouped on one interface.
- Abstract a cross-boundary dependency (file system, time, windowing, input) behind an interface when you genuinely need to swap or test it — but per the Core Philosophy, do not add an interface a lesson does not need.
