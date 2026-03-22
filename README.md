# Valkarn Random for Unity

> High-performance deterministic PRNG library for Unity. PCG32, xoshiro256**, Philox counter-based, distributions, weighted selection, fairness systems. Zero-allocation, Burst-compatible, IL2CPP-optimized.

[![Unity](https://img.shields.io/badge/Unity-2023.1%2B-black)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/license-MIT-green)](LICENSE.md)

---

## Features

- **Zero allocation** — struct-based PRNGs, no heap pressure on hot paths
- **Deterministic** — same seed = same sequence, always, on every platform
- **Per-instance state** — no global mutable state, no cross-system corruption
- **Burst & Jobs ready** — all core PRNG types are unmanaged structs
- **4 PRNG engines + 1 noise function** — PCG32, xoshiro256**, SplitMix64, Philox4x32, SquirrelNoise5
- **7 distribution samplers** — Gaussian, exponential, triangular, sphere, disk, hemisphere, cone
- **Weighted selection O(1)** — Vose Alias Method for loot tables
- **Anti-streak fairness** — PRD (Dota 2 style), shuffle bags, low-discrepancy sequences
- **Unbiased shuffle** — Fisher-Yates with Lemire debiased range reduction
- **Position-based noise** — SquirrelNoise5 for deterministic procedural generation
- **Fork & skip-ahead** — deterministic sub-streams, O(log n) advance for save/load

---

## Quick Start

### Install via Unity Package Manager

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.unapartidamas.valkarn.random": "https://github.com/unapartidamas/valkarn-random.git"
  }
}
```

### Basic usage

```csharp
using UnaPartidaMas.Valkarn.Random;

// Auto-seeded from OS entropy (non-deterministic)
var rng = ValkarnRandom.Create();
int roll = rng.Range(1, 7);       // [1, 6] — a fair die
float f  = rng.NextFloat();        // [0, 1)
```

```csharp
// Deterministic — same seed, same sequence, every time
var rng = ValkarnRandom.Create(42);
int value = rng.Range(0, 100);     // always the same result
```

### Per-system determinism

```csharp
// Each system gets its own RNG — they never interfere with each other
var masterRng = ValkarnRandom.Create(worldSeed);
var aiRng     = masterRng.Fork(id: 0);   // deterministic child for AI
var lootRng   = masterRng.Fork(id: 1);   // deterministic child for loot
var spawnRng  = masterRng.Fork(id: 2);   // deterministic child for spawning
```

### Distributions

```csharp
// Gaussian — damage variance, AI reaction times
float damage = rng.NextGaussian(mean: 50f, stddev: 10f);

// Exponential — Poisson spawn timing
float nextSpawn = rng.NextExponential(rate: 3f); // avg 0.33s between spawns

// Triangular — "roughly centered" random
float npcSpeed = rng.NextTriangular(min: 2f, max: 8f, mode: 5f);

// Geometric — particles, projectile spread
rng.NextOnSphere(out float x, out float y, out float z);
```

### Loot tables (O(1) weighted selection)

```csharp
using UnaPartidaMas.Valkarn.Random.Selection;

// Build once
var loot = new AliasTable(new float[] { 70f, 20f, 7f, 3f }); // common, rare, epic, legendary

// Sample in O(1)
int drop = loot.Sample(ref rng); // 0=common, 1=rare, 2=epic, 3=legendary
```

### Anti-streak (PRD)

```csharp
using UnaPartidaMas.Valkarn.Random.Fairness;

// 25% crit chance — but no 12+ miss streaks (guaranteed proc within 12 attempts)
var crit = new PseudoRandomDistribution(0.25f);

if (crit.Roll(ref rng))
    ApplyCriticalHit();
```

### Shuffle bag (Tetris-style)

```csharp
var bag = ShuffleBag<string>.CreateWeighted(
    ("common", 7), ("rare", 2), ("epic", 1));

string item = bag.Draw(ref rng); // exact 70/20/10 distribution per cycle
```

### Position-based noise (stateless)

```csharp
// No state, no sequence dependency — pure function of (position, seed)
uint tileType = ValkarnNoise.Hash(tileX, tileY, worldSeed);
float height  = ValkarnNoise.Float(tileX, tileY, worldSeed);
```

### Save / Load

```csharp
// Save
ulong savedState = rng.State;
ulong savedInc   = rng.Increment;

// Load — exact continuation
var restored = ValkarnRandom.FromState(savedState, savedInc);
```

---

## Important: Mutable Struct

`ValkarnRandom` is a **mutable value type**. Always store it in a field or pass by `ref`. Passing by value copies the state — the original is not advanced:

```csharp
// WRONG — rng is copied, original never advances
void Roll(ValkarnRandom rng) { rng.NextFloat(); }

// RIGHT — rng is passed by reference
void Roll(ref ValkarnRandom rng) { rng.NextFloat(); }
```

The same applies to `PseudoRandomDistribution` and `R2Sequence`.

---

## Security

**ValkarnRandom is NOT cryptographically secure.** PCG32 can be reversed from ~64 observed outputs via lattice reduction. Do not use for:
- Gambling / gacha with real money
- Anti-cheat systems
- Server-authoritative loot visible to clients
- Secret generation (tokens, keys)

For these use cases, use `System.Security.Cryptography.RandomNumberGenerator` or a dedicated CSPRNG.

---

## Thread Safety

**Not thread-safe.** Each thread must own its own `ValkarnRandom` instance. Use `Fork()` to create per-thread RNGs deterministically, or `Philox4x32.ComputeStateless()` for lock-free parallel use in Burst Jobs.

---

## Performance

Benchmarked on .NET 9, Release mode, 10M iterations (Stopwatch):

| Operation | ValkarnRandom | System.Random (.NET 9) | Speedup |
|-----------|---------------|------------------------|---------|
| Raw integer | **3.2 ns** | 7.4 ns | **2.3x** |
| Float [0,1) | **1.4 ns** | 7.6 ns | **5.4x** |
| Range [0,100) | **2.1 ns** (Lemire) | 17.1 ns | **8.1x** |
| Gaussian (Polar) | **32.4 ns** | N/A (no built-in) | - |
| GC allocs per call | **0 bytes** | 0 bytes | tie |

Note: `System.Random` in .NET 6+ uses xoshiro128** internally and rejection-based range reduction — significantly faster than older .NET Framework versions.

---

## Comparison

| Feature | UnityEngine.Random | System.Random | **Valkarn Random** |
|---------|-------------------|---------------|-------------------|
| State | Global (static) | Per-instance (class) | **Per-instance (struct, 16B)** |
| Thread-safe | No (main thread only) | No (`Shared` is, instances not) | **No (isolated by value)** |
| Deterministic | Fragile (global state) | Yes | **Yes** |
| Burst / Jobs | No | No | **Yes (core structs)** |
| Fork / sub-streams | No | No | **Fork()** |
| Save / Load state | Yes (`Random.State`) | No | **Full state** |
| Skip-ahead | No | No | **O(log n)** |
| Distributions | No | No | **7 built-in** |
| Weighted selection | No | No | **O(1) Alias** |
| Anti-streak | No | No | **PRD + ShuffleBag** |
| Blue noise | No | No | **R2 + Halton** |
| Position noise | No | No | **SquirrelNoise5** |
| Statistical quality | Undocumented | xoshiro128** | **PCG32 (BigCrush + PractRand 1TB)** |
| Debiased range | Unknown | Rejection | **Lemire** |
| Cryptographic | No | No | **No** |

Note: `AliasTable` and `ShuffleBag<T>` are managed classes (not Burst-compatible). Core PRNG structs (`ValkarnRandom`, `Pcg32`, `Xoshiro256StarStar`, `Philox4x32`, `SplitMix64`) are all unmanaged and Burst-safe.

---

## Requirements

- Unity 2023.1 or later
- .NET Standard 2.1

Optional (detected automatically via asmdef `versionDefines`):
- Unity Burst 1.8+ — enables `[BurstCompile]` compatibility for core structs
- Unity Collections 2.1+ — future: NativeArray batch fill
- Unity Mathematics 1.2+ — future: float3/float4 convenience methods

---

## License

MIT — free for everyone. See [LICENSE.md](LICENSE.md).

---

## Credits

Made by **Una Partida Mas** — [unapartidamas.com](https://unapartidamas.com)
