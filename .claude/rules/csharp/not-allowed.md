# What NOT to Do

These are hard stops. Do not do any of the following under any circumstances.

## Type System
- Do NOT use nullable annotations (`?`) defensively — only use `?` where a value is genuinely nullable (generic return types, reflection code, extension methods designed to accept null). Never annotate injected dependencies or always-populated values.
- Do NOT use records — use classes (or `struct` for small value types) only.

## Async
- Do NOT use `async void` — always return `Task` or `Task<T>`.
- Do NOT use `ConfigureAwait(false)` — we do not use it.
- Do NOT block on tasks with `.Result` or `.Wait()`.
- Do NOT make the per-frame `Update`/`Render` loop `async`, and do not issue OpenGL calls off the GL context thread.

## Code Style
- Do NOT use expression-bodied members (`=>` syntax for methods or properties) — this applies to ALL access levels (public, private, protected, internal).
- Do NOT use query syntax LINQ (`from x in y where...`) — method chaining only.
- Do NOT use string concatenation with `+` — use string interpolation. Write `$"Loaded {count} meshes"`, never `"Loaded " + count + " meshes"`.
- Do NOT abbreviate names — write them out fully (widely recognized graphics acronyms like `GPU`, `VAO`, `VBO`, `UV` are fine).
- Do NOT write comments explaining what code does — rename until obvious. (Comments explaining *why*, or explaining non-obvious graphics/math, are welcome.)
- Do NOT use `new List<T>()`, `new T[0]`, or `new Dictionary<K, V>()` for empty or inline-populated collections — use collection expressions (`[]`).

## Architecture
- Do NOT use property injection or setter injection — constructor only.
- Do NOT use `new` inside a class to instantiate a dependency you would want to swap or test — inject it. (Constructing plain data — vectors, matrices, meshes — inline is fine.)
- Do NOT name a file after an interface — always name after the class.
- Do NOT add abstractions or patterns that do not exist in the surrounding codebase.
- Do NOT introduce over-engineering or write ahead of the current lesson — match the abstraction level of the existing code and stay in scope.

## Errors & Logging
- Do NOT throw exceptions for predictable, known failure states — return an enum.
- Do NOT swallow exceptions silently.
- Do NOT log per-frame at `Information`, and do NOT leave `Console.WriteLine` / temporary traces in committed code.

## Formatting
- Do NOT manually fight CSharpier formatting — it is the final authority.
- Do NOT suppress `dotnet format` / analyzer warnings without a comment explaining why.
