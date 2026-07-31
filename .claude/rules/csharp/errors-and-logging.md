# Error Handling & Logging

## Error Handling
- Exceptions are for unplanned, unexpected failures only (missing files, corrupted assets, driver/GL errors).
- Never throw an exception for a predictable outcome (validation failure, value out of range, known bad state).
- For known failure modes, return a value — an enum is preferred.
- Let exceptions bubble to the boundary where they can be meaningfully handled.
- Do not catch and swallow exceptions silently. The one exception: if a failure is genuinely non-actionable, an empty catch with a `// ignored` comment is acceptable. This must be rare and deliberate — never use it to hide logic errors.
- "Log and rethrow at the boundary" is allowed at the top-level entry point. Do not log-and-rethrow at every layer — pick one boundary.

```csharp
// Preferred for known failures:
public enum ShaderCompileResult { Success, VertexStageFailed, FragmentStageFailed, LinkFailed }

public ShaderCompileResult TryCompile(ShaderSource source)
{
    if (!CompileStage(source.Vertex))     return ShaderCompileResult.VertexStageFailed;
    if (!CompileStage(source.Fragment))   return ShaderCompileResult.FragmentStageFailed;
    if (!LinkProgram())                   return ShaderCompileResult.LinkFailed;
    return ShaderCompileResult.Success;
}
```

## OpenGL errors
- GL is a C state machine that reports failures out-of-band, not via exceptions. In `Debug` builds, check `GL.GetError()` (or install a debug message callback) after risky GL calls and surface the failure — a silent GL error is the usual cause of a black screen.
- Do not leave per-call `GL.GetError()` polling in `Release`/hot paths; gate it behind a debug flag.

## Logging
- Log at the action site, not at the boundary.
- Log thoughtfully — do not add log entries without a clear reason. A 60 FPS loop will drown you in noise; never log per-frame at `Information`.
- Do not use logging (or `Console.WriteLine`) as a scratch debugger. If you add a temporary trace while diagnosing, remove it before committing.
