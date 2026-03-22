// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// ValkarnRandom — main API facade for deterministic random number generation.
// Wraps Pcg32 (XSH-RR) with convenience methods for common game operations.
//
// Zero Config: var rng = ValkarnRandom.Create(42); float f = rng.NextFloat();
// Full Control: Fork(), Advance(), state serialization, distribution samplers.
//
// WARNING: NOT cryptographically secure. Do not use where prediction resistance
// is required (gambling, anti-cheat, server-authoritative loot). PCG32 can be
// reversed from ~64 observed outputs via lattice reduction (Bouillaguet et al. 2020).
//
// THREAD SAFETY: Not thread-safe. Each thread must use its own instance.
// Use Fork() to create per-thread RNGs, or Philox4x32 for stateless parallel use.
//
// MUTABLE STRUCT: This is a mutable value type. You must store it in a field or
// pass by ref. Passing by value copies the state — the original is not advanced.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnaPartidaMas.Valkarn.Random.Internal;

namespace UnaPartidaMas.Valkarn.Random
{
    /// <summary>
    /// High-performance deterministic PRNG facade wrapping PCG32.
    /// <para><b>Not cryptographically secure.</b> Do not use for gambling, anti-cheat, or secrets.</para>
    /// <para><b>Not thread-safe.</b> Each thread must own its instance. Use Fork() for parallelism.</para>
    /// <para><b>Mutable struct.</b> Always store in a field or pass by <c>ref</c>.</para>
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ValkarnRandom
    {
        Pcg32 rng;

#if UNITY_ASSERTIONS
        int _ownerThreadId;

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        void CheckThreadSafety()
        {
            int current = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (_ownerThreadId == 0)
                _ownerThreadId = current;
            else if (_ownerThreadId != current)
                throw new InvalidOperationException(
                    $"ValkarnRandom accessed from thread {current} but owned by thread {_ownerThreadId}. " +
                    "Each thread must use its own instance. Use Fork() to create per-thread RNGs.");
        }
#endif

        /// <summary>Creates a ValkarnRandom from a seed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValkarnRandom(ulong seed)
        {
            rng = new Pcg32(seed);
#if UNITY_ASSERTIONS
            _ownerThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        }

        /// <summary>
        /// Creates a ValkarnRandom with a seed from OS entropy.
        /// Non-deterministic — use for gameplay where reproducibility is not needed.
        /// </summary>
        public static ValkarnRandom Create()
        {
            Span<byte> bytes = stackalloc byte[8];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            ulong seed = System.BitConverter.ToUInt64(bytes);
            return new ValkarnRandom(seed); // constructor sets _ownerThreadId
        }

        /// <summary>Creates a ValkarnRandom from a seed. Convenience factory.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValkarnRandom Create(ulong seed) => new ValkarnRandom(seed);

        /// <summary>Creates a ValkarnRandom from an int seed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValkarnRandom Create(int seed) => new ValkarnRandom((ulong)seed);

        /// <summary>Restores from serialized state. For save/load only.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValkarnRandom FromState(ulong state, ulong increment)
        {
            ValkarnRandom v;
            v.rng = Pcg32.FromState(state, increment);
#if UNITY_ASSERTIONS
            v._ownerThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
            return v;
        }

        // ── Integers ──────────────────────────────────────────────────

        /// <summary>Returns a random uint in [0, uint.MaxValue].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
#if UNITY_ASSERTIONS
            CheckThreadSafety();
#endif
            return rng.NextUInt();
        }

        /// <summary>Returns a random int in [0, int.MaxValue].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt()
        {
#if UNITY_ASSERTIONS
            CheckThreadSafety();
#endif
            return rng.NextInt();
        }

        /// <summary>Returns a random int in [min, maxExclusive). Debiased (Lemire).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Range(int min, int maxExclusive) => rng.NextInt(min, maxExclusive);

        /// <summary>Returns a random int in [0, maxExclusive). Debiased (Lemire).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Range(int maxExclusive) => rng.NextInt(maxExclusive);

        // ── Floats ────────────────────────────────────────────────────

        /// <summary>Returns a float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat()
        {
#if UNITY_ASSERTIONS
            CheckThreadSafety();
#endif
            return rng.NextFloat();
        }

        /// <summary>Returns a float in (0, 1]. Safe for log-based transforms.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloatNonZero() => rng.NextFloatNonZero();

        /// <summary>Returns a float in [-1, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloatSigned() => rng.NextFloatSigned();

        /// <summary>Returns a float in [min, max).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Range(float min, float max) => rng.NextFloat(min, max);

        /// <summary>Returns a double in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() => rng.NextDouble();

        // ── Boolean ───────────────────────────────────────────────────

        /// <summary>Returns true with 50% probability.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool() => rng.NextBool();

        /// <summary>Returns true with the given probability in [0, 1].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool(float probability) => rng.NextBool(probability);

        // ── Distributions ─────────────────────────────────────────────

        /// <summary>
        /// Returns a Gaussian (normal) distributed value.
        /// Uses the Polar (Marsaglia) method — cross-platform deterministic (no sin/cos).
        /// </summary>
        public float NextGaussian(float mean = 0f, float stddev = 1f)
        {
            return Distributions.GaussianSampler.SamplePolar(ref rng, mean, stddev);
        }

        /// <summary>
        /// Returns an exponentially distributed value with the given rate (λ).
        /// Mean = 1/rate. Useful for Poisson process timing (spawn intervals, etc).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextExponential(float rate = 1f)
        {
            return Distributions.ExponentialSampler.Sample(ref rng, rate);
        }

        /// <summary>
        /// Returns a triangular distributed value in [min, max] with the given mode.
        /// Useful for "roughly centered" random (AI decisions, damage variance).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextTriangular(float min, float max, float mode)
        {
            return Distributions.TriangularSampler.Sample(ref rng, min, max, mode);
        }

        // ── Geometric Sampling ────────────────────────────────────────

        /// <summary>Returns a uniformly distributed point on the unit circle.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextOnDisk(out float x, out float y)
        {
            Distributions.GeometricSamplers.OnDisk(ref rng, out x, out y);
        }

        /// <summary>Returns a uniformly distributed point on the unit sphere surface.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextOnSphere(out float x, out float y, out float z)
        {
            Distributions.GeometricSamplers.OnSphere(ref rng, out x, out y, out z);
        }

        // ── Forking ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a new ValkarnRandom with a deterministically derived seed.
        /// Same parent state always produces the same child. Uses SplitMix64
        /// for seed derivation (NOT PCG stream selection — streams are correlated).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValkarnRandom Fork()
        {
#if UNITY_ASSERTIONS
            CheckThreadSafety();
#endif
            ulong derived = SplitMix64.Mix(rng.NextULong());
            return new ValkarnRandom(derived);
        }

        /// <summary>
        /// Creates a deterministic child RNG keyed by an ID (entity, subsystem, etc).
        /// Same (parent state, id) always produces the same child.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValkarnRandom Fork(ulong id)
        {
#if UNITY_ASSERTIONS
            CheckThreadSafety();
#endif
            ulong derived = SplitMix64.Derive(rng.NextULong(), id);
            return new ValkarnRandom(derived);
        }

        // ── State / Serialization ─────────────────────────────────────

        /// <summary>Advances the internal state by delta steps. O(log delta).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(ulong delta) => rng.Advance(delta);

        /// <summary>Internal PCG32 state. For serialization only. Exposes full PRNG state.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ulong State => rng.State;

        /// <summary>Internal PCG32 increment. For serialization only. Exposes full PRNG state.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ulong Increment => rng.Increment;

        // ── Array Selection ───────────────────────────────────────────

        /// <summary>Returns a random element from the array.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Choose<T>(T[] array)
        {
            Internal.ThrowHelper.ThrowIfEmpty(array, nameof(array));
            return array[Range(array.Length)];
        }

        /// <summary>Returns a random element from a Span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Choose<T>(ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                throw new ArgumentException("Span must not be empty.", nameof(span));
            return span[Range(span.Length)];
        }
    }
}
