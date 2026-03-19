# MonoGame Particle System Engine

> Real-time interactive particle system engine built from scratch in **MonoGame / C#**.
> No game engine abstractions — raw MonoGame APIs only.

![C#](https://img.shields.io/badge/C%23-MonoGame-blue)
![License](https://img.shields.io/badge/license-MIT-green)

---

## What It Is

A standalone, visually impressive **particle simulation sandbox** demonstrating deep MonoGame framework knowledge. Not a game — a technical showcase of rendering pipeline mastery.

Three interactive demos are included:

| Key | Demo | Description |
|-----|------|-------------|
| `1` | **Fire** | Dual-emitter campfire with flame core and ember drift. Mouse moves the fire source. |
| `2` | **Galaxy** | Rotating spiral galaxy using orbital velocity math on ring emitters. |
| `3` | **Explosion** | One-shot burst explosions triggered by left-click. Up to 8 simultaneous blasts. |

---

## Technical Highlights

### Core Engine (`Core/`)

| File | Purpose |
|------|---------|
| `Particle.cs` | Value-type `struct` for zero GC allocation. Stores position, velocity, colour, scale, lifetime. |
| `ParticlePool.cs` | Fixed-size pre-allocated pool using a `Queue<int>` free list. `O(1)` spawn and kill. |
| `ParticleEmitter.cs` | Manages emission rate, shape (Point / Cone / Ring), physics integration, and `SpriteBatch` rendering. |

### Rendering

- **`BlendState.Additive`** — overlapping particles add brightness instead of covering each other, creating natural glow/fire/bloom effects
- **`Effects/additive_blend.fx`** — custom HLSL shader with radial soft-edge fade and pre-multiplied alpha for per-particle opacity control
- **`TextureHelper.cs`** — procedurally generates soft-circle textures at runtime (no Content Pipeline dependency)

### Performance

- `Particle` is a `struct` — cache-friendly, zero heap allocation per particle
- `ParticlePool` pre-allocates all memory at startup — zero GC pressure during simulation
- Supports 10,000+ simultaneous particles at 60 fps

---

## Architecture

```
MonoGame-ParticleEngine/
├── Core/
│   ├── Particle.cs           # Particle struct (position, velocity, color, life)
│   ├── ParticlePool.cs       # Fixed-size object pool, O(1) spawn/kill
│   └── ParticleEmitter.cs    # Emission shapes, physics integration, SpriteBatch draw
├── Demos/
│   ├── IDemo.cs              # Common interface: Load / Update / Draw / Unload
│   ├── FireDemo.cs           # Dual-emitter campfire (flame + embers)
│   ├── GalaxyDemo.cs         # Spiral galaxy with Keplerian orbital velocity
│   └── ExplosionDemo.cs      # One-shot burst: flash + shrapnel + smoke
├── Effects/
│   └── additive_blend.fx     # HLSL shader: radial soft edge + additive blend
├── ParticleGame.cs           # MonoGame Game class: main loop, input, demo switching
└── TextureHelper.cs          # Procedural soft-circle texture generator
```

---

## Getting Started

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download) or later
- [MonoGame 3.8+](https://www.monogame.net/downloads/)

### Run

```bash
git clone https://github.com/AE707/MonoGame-ParticleEngine.git
cd MonoGame-ParticleEngine
dotnet run
```

### Controls

| Input | Action |
|-------|--------|
| `1` | Switch to Fire demo |
| `2` | Switch to Galaxy demo |
| `3` | Switch to Explosion demo |
| Mouse move | Move emitter (Fire demo) |
| Left click | Detonate explosion (Explosion demo) |
| `Esc` | Quit |

---

## Key Concepts Demonstrated

1. **Object Pooling** — Pre-allocated fixed array + free-index queue eliminates GC entirely
2. **HLSL Shader** — Custom `.fx` file with radial soft-edge, `smoothstep` alpha, and pre-multiplied alpha
3. **Additive Blending** — `BlendState.Additive` for fire/galaxy glow without transparency artifacts
4. **Emitter Shapes** — Point, Cone, and Ring geometries with configurable spread angles
5. **Force Fields** — Per-emitter `GlobalForce` for gravity, convection, wind
6. **One-Shot Bursts** — `ExplosionDemo` shows burst vs continuous emitter pattern
7. **Orbital Physics** — `GalaxyDemo` applies tangential velocity for Keplerian rotation
8. **Procedural Textures** — No content pipeline; textures generated at runtime via `SetData()`

---

## License

MIT — see [LICENSE](LICENSE)
