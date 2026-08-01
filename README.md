# Firmament — A Game Engine From Scratch (C# + OpenTK)

A hobby engine built one lesson at a time, ending in a 2D vector Asteroids and a 3D Asteroids.
No Unity. No Godot. No engine framework. Just C#, OpenTK, and OpenGL calls I wrote myself.

**Status:** see [Progress](#progress) at the bottom.

---

## How to use this repo

This README is the curriculum *and* the prompt. Open Claude Code in this repo (or paste this
file into the web version) and say:

> Lesson 1.1

That's it. Claude reads this file, finds the lesson, and teaches it. When I'm done:

> Lesson 1.2

Other things I can say:

| Say this | What happens |
|---|---|
| `Lesson 4.2` | Teach the next lesson |
| `Create the next lesson` (or `/create-next-lesson`) | Mark the current latest lesson Done, author the next uncreated lesson from this curriculum, and update Progress |
| `Review lesson 4.2` | Look at what I actually wrote, critique it, suggest improvements |
| `I'm stuck on 4.2` | Debug help, no new material |
| `Explain the math in 3.3 again, slower` | Re-explain a concept a different way |
| `Give me extra exercises for 5.1` | More practice, no new lesson |
| `Skip ahead to 8.1` | Fine — but tell me what I'm missing first |
| `Refactor check` | Look at the whole codebase, flag drift and cruft |

---

## Rules for Claude (read this before teaching any lesson)

I'm a Sr. Software Developer / CTO with 20+ years of C#. I do **not** need C# explained to me.
I *do* need graphics pipeline concepts and vector/matrix math explained, because I've never
written raw OpenGL and my linear algebra is rusty.

**Teach like this:**

1. **Stay in scope.** One lesson = one concept. Do not write ahead. Do not "helpfully" add the
   next three lessons' worth of abstraction. If the lesson is a colored triangle, I do not want
   a `MaterialSystem`.
2. **Concept first, ~150–400 words.** What is this thing, why does OpenGL need it, what happens
   on the GPU. Plain language. Then the code.
3. **I type the code.** Give me the code to write, but structured as *what to write and where*,
   not a finished repo dump. Small files, shown in full when they're new, as diffs when they're
   edits.
4. **Explain math geometrically before algebraically.** "The dot product tells you how much two
   directions agree — 1 is same way, 0 is perpendicular, -1 is opposite" comes *before* any
   formula. Always tell me what the math is *for* in this specific lesson. Never assume I
   remember what a matrix column means.
5. **End every lesson with:**
   - ✅ **Checkpoint** — exactly what I should see on screen / in the console if it worked
   - 🐛 **Common failure modes** — black screen? here's the 4 usual causes, in order of likelihood
   - 🔧 **Exercises** — 2–4 small tweaks to try, ordered easy → interesting
   - 📎 **What this unlocks** — one line on why the next lesson needs this
6. **Assume I tweaked things.** I learn by messing with the code. When I come back for the next
   lesson, read the actual files in the repo first — don't assume my code matches what you gave me.
7. **Idiomatic modern C#.** File-scoped namespaces, nullable reference types **off**, `readonly struct`
   where it matters, spans where they earn their keep. Treat me as a peer on language choices;
   argue with me if I pick something dumb.
8. **No mystery numbers.** Every magic constant, GL enum, and buffer offset gets a one-line reason.
9. **Performance honesty.** When we do something the slow-but-clear way, say so and say which
   lesson fixes it.
10. **Don't be a cheerleader.** If my design is going to hurt in lesson 20, say it now.

**Tech constraints:**
- .NET 8+ (or newer LTS), C# latest
- OpenTK 4.x, OpenGL 4.6 core profile — we target 4.6 and don't care about older versions
- Cross-platform-friendly: no Windows-only APIs
- Zero paid assets. CC0 / public domain only (Kenney.nl, OpenGameArt, freesound, Poly Haven). Note the license in `assets/CREDITS.md` as we go.
- No engine dependency creep. Allowed third-party: OpenTK, StbImageSharp (or ImageSharp), ImGui.NET (Part 11 only). Anything else needs a justification.

---

## Suggested repo layout

Introduced gradually — don't create empty folders on day one.

```
/src
  /Firmament.Core          # math, timing, input, logging, resources
  /Firmament.Rendering     # GL wrappers, shaders, buffers, renderers
  /Firmament.Engine        # game loop, scene, entities, components
  /Firmament.Asteroids2D   # game 1
  /Firmament.Asteroids3D   # game 2
/assets
  /shaders /textures /models /audio /fonts
  CREDITS.md
/docs
  /lessons            # my notes per lesson
README.md
```

---

# The Curriculum

13 parts, ~110 lessons. Parts 1–7 build the 2D engine and ship Asteroids 2D.
Parts 8–10 take it to 3D and ship Asteroids 3D. Part 11 is tooling and profiling.
Part 12 is character animation — optional for Asteroids, but it's where an engine stops being a
tech demo. Part 13 is whatever still sounds fun by then.

There is no schedule. This is a hobby. A lesson is 20–90 minutes.

---

## Part 1 — A Window and a Heartbeat
*Goal: a window that opens, closes cleanly, and honestly reports its own frame rate.*

- [x] **1.1** Solution setup, OpenTK via NuGet, `GameWindow`, a window that opens and closes
- [x] **1.2** The game loop: `OnUpdateFrame` vs `OnRenderFrame`, what delta time actually is
- [x] **1.3** FPS: frame counter, rolling average, frame-time in ms (ms is the honest number, FPS is the vanity number) — display in the title bar
- [x] **1.4** `GL.ClearColor` / `GL.Clear`, the color buffer, why you clear at all *(last OpenGL lesson — superseded by the 1.5 pivot)*
- [ ] **1.5** 🔀 **Pivot to DirectX 11 (Silk.NET):** swap the OpenTK/OpenGL foundation for a D3D11 device + swap chain, keep the same game loop (animating the clear color proves it's alive)
- [ ] **1.6** Keyboard input basics: ESC to quit, `KeyboardState` snapshot vs event
- [ ] **1.7** Window resize, viewport, aspect ratio, and what happens if you ignore it
- [ ] **1.8** VSync, tearing, and why your FPS counter says 60 no matter what you do

## Part 2 — The Pipeline and the First Triangle
*Goal: understand every stage between a float array and a lit pixel.*

- [ ] **2.1** The graphics pipeline, end to end, no code — a mental model to hang everything else on
- [ ] **2.2** Vertex Buffer Objects: getting floats onto the GPU
- [ ] **2.3** Vertex Array Objects: telling the GPU what those floats *mean* (`VertexAttribPointer`)
- [ ] **2.4** GLSL: your first vertex + fragment shader, in-line strings
- [ ] **2.5** Compiling, linking, and actually reading the error log (do this now, thank yourself forever)
- [ ] **2.6** `GL.DrawArrays` — the triangle appears
- [ ] **2.7** A `Shader` class: load from file, dispose properly, cache uniform locations
- [ ] **2.8** Vertex colors and how the rasterizer interpolates between vertices
- [ ] **2.9** Element Buffer Objects: drawing a quad with 4 vertices instead of 6
- [ ] **2.10** `GL.GetError` wrappers, `KHR_debug` output, and an assert helper for GL calls
- [ ] **2.11** Uniforms: pushing a color from C# into the shader, per-frame

## Part 3 — The Math You Actually Need
*Goal: stop being scared of vectors. Only the parts games use.*

- [ ] **3.1** Vectors as arrows: add, subtract, scale, magnitude, normalize — and what each *means* on screen
- [ ] **3.2** Dot product: angles, facing checks, projection. Three concrete game uses
- [ ] **3.3** Cross product: perpendiculars, winding order, and why it matters in 3D
- [ ] **3.4** Matrices as coordinate-space transforms (not as grids of numbers)
- [ ] **3.5** Translate / rotate / scale, and why multiplication order changes everything
- [ ] **3.6** Orthographic projection: defining a 2D world in world units, not pixels
- [ ] **3.7** The Model-View-Projection chain, uploaded as a `mat4` uniform
- [ ] **3.8** OpenTK's `Vector2/3/4`, `Matrix4`, row-major gotchas, and when to hand-roll instead
- [ ] **3.9** A 2D camera: pan, zoom, screen↔world coordinate conversion
- [ ] **3.10** Angles, radians, `Atan2`, and rotating a ship toward a point
- [ ] **3.11** Framerate-independent movement: why `position += velocity` without `dt` is a bug

## Part 4 — Engine Bones
*Goal: stop writing everything in `Program.cs`.*

- [ ] **4.1** Splitting `Engine` from `Game`: what the engine owns vs what the game owns
- [ ] **4.2** Fixed timestep with an accumulator, and render interpolation (the "Fix Your Timestep" lesson)
- [ ] **4.3** GL object lifetimes: `IDisposable`, finalizer traps, and why GL handles can't be freed on a random thread
- [ ] **4.4** A resource manager: load once, reference count, hot-reload shaders on file change
- [ ] **4.5** Entities and components — a pragmatic middle ground, not a full ECS (and an honest note on when a real ECS would be worth it)
- [ ] **4.6** The scene: update order, spawn/destroy queues, and why you never mutate a list you're iterating
- [ ] **4.7** Input manager: `IsDown` vs `WasPressed` vs `WasReleased`, action mapping, mouse state
- [ ] **4.8** Logging + a debug overlay: FPS, frame time, draw calls, entity count
- [ ] **4.9** Config: window size, vsync, key bindings from a JSON file

## Part 5 — 2D Renderer
*Goal: draw lots of things, fast, without touching raw GL again in game code.*

- [ ] **5.1** A line renderer — the Asteroids look. Dynamic VBO, `GL.LineWidth` and its lies
- [ ] **5.2** Why one draw call per object kills you: the case for batching
- [ ] **5.3** A sprite batcher: build a vertex buffer in C#, flush once per frame
- [ ] **5.4** Textures: loading a PNG with StbImageSharp, filtering, wrapping, mipmaps
- [ ] **5.5** Texture units and sampling multiple textures in one shader
- [ ] **5.6** Blending and alpha: `SrcAlpha`/`OneMinusSrcAlpha`, additive glow, and render-state management
- [ ] **5.7** Texture atlases and UV math (goodbye texture-swap draw calls)
- [ ] **5.8** Bitmap font text rendering — FPS on the screen, not the title bar
- [ ] **5.9** A `Renderer2D` façade: `DrawLine`, `DrawPolygon`, `DrawSprite`, `DrawText`
- [ ] **5.10** Sorting and layers, and the draw-call counter that proves the batcher works

## Part 6 — Asteroids 2D
*Goal: a finished, playable game. Not a demo.*

- [ ] **6.1** The ship: transform, thrust vector, angular velocity, drag
- [ ] **6.2** Screen wrapping (the world is a torus) and drawing objects that straddle the edge
- [ ] **6.3** Bullets: spawning, lifetime, and an object pool that doesn't allocate
- [ ] **6.4** Procedural asteroids: jagged polygons from a seeded RNG
- [ ] **6.5** Collision part 1: circle-circle broadphase
- [ ] **6.6** Collision part 2: point-in-polygon and SAT for the ship hull
- [ ] **6.7** Asteroid splitting, momentum inheritance, and the scoring rules
- [ ] **6.8** Game states: attract mode → playing → dying → game over, as a state machine
- [ ] **6.9** Lives, respawn invulnerability, wave progression, and the UFO
- [ ] **6.10** Particles: explosions, engine exhaust, a pooled particle system
- [ ] **6.11** Audio: OpenAL through OpenTK, one-shot SFX, and the classic two-tone heartbeat
- [ ] **6.12** Juice: screen shake, hit pause, line flicker. The 5% that's 50% of the feel
- [ ] **6.13** High score persistence, pause menu, and calling it done

## Part 7 — Framebuffers and Post-Processing
*Goal: render to a texture, then abuse it.*

- [ ] **7.1** FBOs: rendering to a texture instead of the screen
- [ ] **7.2** The full-screen quad pass and a pass-through post shader
- [ ] **7.3** Gaussian blur, two-pass, and the bright-pass filter
- [ ] **7.4** Bloom — vector lines that actually glow
- [ ] **7.5** A CRT shader: scanlines, barrel distortion, phosphor decay (optional and very fun)
- [ ] **7.6** A post-processing stack you can reorder at runtime

**🏁 Checkpoint: Asteroids 2D is shipped. Take a break or keep going.**

---

## Part 8 — Into the Third Dimension
*Goal: everything you know, plus a Z axis and a depth buffer.*

- [ ] **8.1** Perspective projection: FOV, near/far planes, and why the far plane ruins your day
- [ ] **8.2** The depth buffer, depth testing, z-fighting, and precision
- [ ] **8.3** A rotating cube — 3D hello world
- [ ] **8.4** The view matrix, `LookAt`, and thinking in camera space
- [ ] **8.5** A free-fly camera: WASD + mouse look, yaw/pitch, and the gimbal lock you're about to hit
- [ ] **8.6** Quaternions, gently. What they solve, how to use them, why you don't need the math behind them yet
- [ ] **8.7** Face culling, winding order, and the "half my model is invisible" bug
- [ ] **8.8** Normals: what they are, how to compute them, flat vs smooth shading
- [ ] **8.9** Blinn-Phong lighting: ambient + diffuse + specular, built up one term at a time
- [ ] **8.10** Directional, point, and spot lights; attenuation; multiple lights in one shader
- [ ] **8.11** Materials and a uniform buffer object (UBO) for shared per-frame data
- [ ] **8.12** Loading models: OBJ by hand first (it's simple and educational), then glTF via a library
- [ ] **8.13** Textured 3D: diffuse maps, and why sRGB vs linear color space matters
- [ ] **8.14** Normal mapping and tangent space (optional but high impact)

## Part 9 — 3D Engine Systems
*Goal: handle a scene with a thousand rocks in it.*

- [ ] **9.1** Scene graph: parent/child transforms, dirty flags, world matrix caching
- [ ] **9.2** Bounding volumes: AABB, spheres, and computing them from a mesh
- [ ] **9.3** Frustum culling — the biggest perf win you'll ever get for the least code
- [ ] **9.4** Instanced rendering: 5,000 asteroids in one draw call
- [ ] **9.5** A skybox / procedural starfield with correct depth handling
- [ ] **9.6** Shadow mapping: depth-from-light, peter-panning, acne, PCF (optional, hardest lesson in the book)
- [ ] **9.7** Transparency: sorting, and why it's genuinely unsolved
- [ ] **9.8** Level of detail: swapping meshes by distance

## Part 10 — Asteroids 3D
*Goal: ship game two.*

- [ ] **10.1** Ship control in 3D: 6DOF vs constrained-plane, and picking the one that's actually fun
- [ ] **10.2** Third-person chase camera with spring damping
- [ ] **10.3** Procedural asteroid meshes: icosphere subdivision + noise displacement
- [ ] **10.4** A bounded play space: wrapping, invisible walls, or a fog boundary — pick and justify
- [ ] **10.5** 3D collision: spheres → OBB → mesh-approximate, in that order
- [ ] **10.6** Shooting in 3D: raycast vs projectile, aim assist, and a crosshair that helps
- [ ] **10.7** Splitting asteroids in 3D with inherited spin
- [ ] **10.8** Billboarded particles and GPU particle updates
- [ ] **10.9** Positional audio with OpenAL listeners
- [ ] **10.10** A 3D HUD: screen-space overlay, off-screen enemy indicators, radar
- [ ] **10.11** Waves, difficulty curve, and shipping it

## Part 11 — Tooling and Performance
*Goal: know why it's slow before you guess.*

- [ ] **11.1** CPU profiling: `Stopwatch` scopes, allocation tracking, and killing GC spikes in the loop
- [ ] **11.2** GPU timer queries and a real frame budget breakdown
- [ ] **11.3** RenderDoc: capture a frame, inspect every draw call, find the bug you couldn't see
- [ ] **11.4** ImGui.NET: a live debug panel for tweaking values without recompiling
- [ ] **11.5** An in-engine scene inspector and entity editor
- [ ] **11.6** Asset hot-reload for shaders, textures, and models
- [ ] **11.7** `dotnet publish`, single-file, AOT, and shipping something a friend can run

## Part 12 — Rigging, Skinning, and Animation
*Goal: make a mesh move like a creature instead of a crate.*

Nothing in Asteroids needs this. It's here because it's one of the most interesting systems in a
real engine, and because "I want to load a rigged character and have it run" is a reasonable thing
to want from an engine you built yourself. **Prerequisites: 8.6 (quaternions), 8.12 (glTF loading),
9.1 (scene graph).** Grab a CC0 rigged model first — Quaternius, Kenney, or Mixamo — and note it in
`CREDITS.md`.

- [ ] **12.1** Why transforms aren't enough: the four ways to animate a mesh, and when each one wins
- [ ] **12.2** Morph targets / blend shapes: interpolating between whole vertex sets. The simple case first
- [ ] **12.3** Skeletons: joint hierarchy, local vs model space, the bind pose, and the inverse bind matrix (the concept people get stuck on — we'll do it slowly)
- [ ] **12.4** Skinning weights: joint indices and weights as vertex attributes, the 4-influence convention, weight normalization
- [ ] **12.5** Linear blend skinning in the vertex shader, and the bone matrix palette as a UBO
- [ ] **12.6** The candy-wrapper artifact: where LBS breaks down, and dual quaternion skinning as the fix
- [ ] **12.7** glTF animation data: samplers, channels, keyframes, and the three interpolation modes
- [ ] **12.8** Sampling a pose at time *t*: quaternion SLERP, and why you never LERP a rotation
- [ ] **12.9** Animation clips: play, loop, speed, events on keyframes
- [ ] **12.10** Crossfading between clips, and a blend tree for locomotion (idle → walk → run)
- [ ] **12.11** Additive animation and bone masking — aim the upper body while the legs keep running
- [ ] **12.12** An animation state machine, and how it talks to gameplay code
- [ ] **12.13** Root motion vs in-place, and the collision problems root motion causes
- [ ] **12.14** Sockets and attachments: putting a weapon in a hand that's moving
- [ ] **12.15** Two-bone IK: foot planting on uneven ground, and look-at aiming
- [ ] **12.16** Performance: GPU skinning vs compute skinning, animation LOD, and instancing skinned meshes (the hard one)
- [ ] **12.17** A skeleton debug renderer — bones, joint axes, and bind-pose overlay. Build this early, honestly

## Part 13 — Stretch Goals
*Pick what sounds fun. No order.*

- [ ] **13.1** Compute shaders: particle simulation on the GPU
- [ ] **13.2** Deferred rendering vs forward — the tradeoff, and a G-buffer
- [ ] **13.3** Screen-space ambient occlusion
- [ ] **13.4** PBR: metallic/roughness workflow and IBL
- [ ] **13.5** A simple scripting layer (Roslyn or Lua) for gameplay
- [ ] **13.6** Networking: two-player Asteroids with client-side prediction
- [ ] **13.7** Gamepad support and rebindable input
- [ ] **13.8** A Vulkan backend behind the same `IGraphicsDevice` interface (the boss fight)

---

## Progress

- [ ] Part 1 — Window and Heartbeat
- [ ] Part 2 — Pipeline and First Triangle
- [ ] Part 3 — The Math
- [ ] Part 4 — Engine Bones
- [ ] Part 5 — 2D Renderer
- [ ] Part 6 — **Asteroids 2D shipped**
- [ ] Part 7 — Post-Processing
- [ ] Part 8 — Into 3D
- [ ] Part 9 — 3D Systems
- [ ] Part 10 — **Asteroids 3D shipped**
- [ ] Part 11 — Tooling
- [ ] Part 12 — Rigging, Skinning, Animation
- [ ] Part 13 — Stretch

### Lesson log

Each lesson links to its authored file. Check it off when you've worked through it; jot notes inline after the link if you want.

> **🔀 Foundation pivot (as of 1.5):** the engine moves off **OpenTK / OpenGL** onto **DirectX 11 via Silk.NET** — Windows-only, by choice. Lessons 1.1–1.4 stand as the OpenGL foundation; from 1.5 on, the rendering stack is D3D11. Parts 2–13 are still OpenGL-worded and will be re-scoped to D3D11 as a separate pass.

- [x] [1.1 — A Window and a GL Context](lessons/1.1-a-window-and-a-gl-context.md)
- [x] [1.2 — The Game Loop and Delta Time](lessons/1.2-the-game-loop-and-delta-time.md)
- [x] [1.3 — FPS, Frame Time, and the Title Bar](lessons/1.3-fps-frame-time-and-the-title-bar.md)
- [x] [1.4 — `GL.ClearColor` / `GL.Clear` and the Color Buffer](lessons/1.4-clear-color-and-the-color-buffer.md)
- [ ] [1.5 — 🔀 Pivot to DirectX 11 (Silk.NET)](lessons/1.5-pivot-to-directx-11-silk-net.md)

---

## Reference

- [OpenTK docs](https://opentk.net/learn/index.html)
- [LearnOpenGL](https://learnopengl.com/) — C++, but the concepts map 1:1 and it's the best there is
- [Fix Your Timestep](https://gafferongames.com/post/fix_your_timestep/) — required reading for 4.2
- [The Book of Shaders](https://thebookofshaders.com/) — fragment shader intuition
- [Kenney assets](https://kenney.nl/assets) (CC0), [OpenGameArt](https://opengameart.org/), [Poly Haven](https://polyhaven.com/) (CC0), [freesound](https://freesound.org/)
- Rigged/animated models for Part 12: [Quaternius](https://quaternius.com/) (CC0), [Mixamo](https://www.mixamo.com/) (free, Adobe account required), and the [glTF sample models](https://github.com/KhronosGroup/glTF-Sample-Models) repo — `RiggedSimple` and `CesiumMan` are the standard debugging models

## License

Personal hobby project. Third-party assets retain their own licenses — see `assets/CREDITS.md`.
