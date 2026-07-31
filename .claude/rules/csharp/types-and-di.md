# Types & Dependency Injection

## var
- Use `var` as the default for local variable declarations.
- Small methods and good naming make the type obvious from context.
- Use explicit types only when the type is not clear from the right-hand side.

## Nullable Reference Types
- Use nullable annotations (`?`) only where a value is genuinely optional or nullable — for example, generic return types (`T?`), reflection-heavy code, or extension methods that accept null input by design.
- Do not annotate defensively. If a value cannot be null in normal usage, do not mark it nullable.
- Do not write `string?`, `ILogger?`, or other nullable annotations on injected dependencies or values that are always populated.

## Collection Initialization
- Use collection expressions (`[]`) to initialize collections. Do not use `new List<T>()`, `new T[0]`, or `new Dictionary<K, V>()` when an empty or inline-populated collection is needed.
- The target type drives the actual collection — `List<float> Vertices { get; set; } = [];` produces an empty list, just like `new List<float>()`, but is shorter and consistent.

```csharp
// Correct — collection expressions
public List<Mesh> Meshes { get; set; } = [];
public float[] Vertices { get; set; } = [];
var attributes = [position, color, texCoord];

// Wrong — explicit constructor calls
public List<Mesh> Meshes { get; set; } = new List<Mesh>();
public float[] Vertices { get; set; } = new float[0];
var attributes = new List<VertexAttribute> { position, color, texCoord };
```

This applies to property initializers, field initializers, local variables, and method arguments. The exception is when you need a specific concrete type that the target cannot infer (e.g. assigning to `IEnumerable<T>` and needing a `HashSet<T>` specifically) — in that case, name the type explicitly.

## Thread-Safe Collections
- Use `ConcurrentDictionary<TKey, TValue>` for shared mutable state that is accessed across threads.
- Never use a plain `Dictionary` with manual locking for this purpose.

## Lazy Initialization
- The default lazy pattern uses the C# `field` keyword with null-coalescing assignment in a property getter. This is preferred over `Lazy<T>` for ordinary deferred initialization:

```csharp
public ShaderProgram DefaultShader
{
    get { return field ??= LoadDefaultShader(); }
}
```

- Use `Lazy<T>` only when you genuinely need its thread-safety guarantees (a value that may be initialized concurrently and must run the factory exactly once). When you do, use the factory constructor overload: `new Lazy<T>(() => ...)`.

## Records — BANNED
- Records are banned. Use classes only. (Prefer `struct` for small value types such as vectors/matrices when a value type is genuinely appropriate.)

## Access Modifiers
- Public is the default. Do not add access modifiers to restrict visibility unless there is a specific reason.
- `dotnet format` (via `.editorconfig`) enforces readonly and auto-properties — follow its guidance.

## Constructor Injection Only
- When a class has genuine dependencies, inject them via the constructor. No property injection. No setter injection.
- Use primary constructors (C# 12+) as the standard form for all new code. Do not write explicit constructor bodies with `this.field = param` assignments when a primary constructor will do.
- Never use `new` inside a class to instantiate a *dependency* you would want to swap or test — ask for it via the constructor. (Constructing plain value objects, meshes, matrices, etc. inline is fine — those are data, not dependencies.)

```csharp
// Correct — primary constructor
public class Renderer(IWindow window, ShaderProgram shader)
{
    // window and shader are available directly
}

// Wrong — traditional explicit constructor
public class Renderer
{
    private readonly IWindow window;

    public Renderer(IWindow window)
    {
        this.window = window;
    }
}
```

## LINQ
- Use LINQ for querying and transforming collections. Prefer it over imperative loops.
- Always use method chaining syntax. Never use query syntax (`from x in y where...`).
- CSharpier handles formatting — write readable code and let it format.
- Note: in per-frame hot paths (the render/update loop), a plain `for` loop that avoids allocations is preferable to LINQ. Use LINQ freely in setup and non-hot-path code.

## GlobalUsings
Each project may have a single `GlobalUsings.cs` at the project root that declares `global using` directives for namespaces used throughout the project (e.g. `global using OpenTK.Mathematics;`). Keep these short and project-wide — they are part of the public surface of the project, not a dumping ground.

## C# 14 Features
- The `field` keyword is accepted in property getters for backing-field initialization (`field ??= ...`) and in computed properties that need to cache.
- Extension blocks (`extension(TargetType target) { ... }`) are accepted for grouping multiple extension methods on the same type.
