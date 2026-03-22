// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// PCG32 (XSH-RR) — primary scalar PRNG.
// Reference: Melissa E. O'Neill, "PCG: A Family of Simple Fast Space-Efficient
//            Statistically Good Algorithms for Random Number Generation",
//            HMC-CS-2014-0905, Harvey Mudd College, 2014.
//
// State: 16 bytes (ulong state + ulong increment). Period: 2^64.
// Output: 32-bit via XOR-shift + random rotation.
// Passes TestU01 BigCrush and PractRand to 1TB+.
//
// NOTE: PCG streams (different increments) are NOT statistically independent
// (Durst 1989, Vigna 2020). Do NOT use stream selection for parallel RNG.
// Use Xoshiro256StarStar.Jump() or Philox4x32 for parallel independence.
//
// WARNING: NOT cryptographically secure. State recoverable from ~64 outputs
// via LLL lattice reduction (Bouillaguet et al. 2020).
//
// THREAD SAFETY: Not thread-safe. Each thread must use its own instance.
// MUTABLE STRUCT: Always store in a field or pass by ref.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnaPartidaMas.Valkarn.Random.Internal;

namespace UnaPartidaMas.Valkarn.Random
{
    /// <summary>
    /// PCG32 (XSH-RR) — primary scalar PRNG. 16 bytes, period 2^64.
    /// <para><b>Not cryptographically secure.</b></para>
    /// <para><b>Mutable struct.</b> Store in a field or pass by <c>ref</c>.</para>
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct Pcg32
    {
        const ulong Multiplier = 6364136223846793005UL;
        const ulong DefaultIncrement = 1442695040888963407UL;

        ulong state;
        ulong increment;

        /// <summary>
        /// Creates a PCG32 PRNG from a seed. Uses SplitMix64 for state initialization.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pcg32(ulong seed)
        {
            increment = DefaultIncrement;
            state = 0UL;
            state = state * Multiplier + increment;
            state += SplitMix64.Mix(seed);
            state = state * Multiplier + increment;
        }

        /// <summary>
        /// Creates a PCG32 PRNG from a seed and a stream ID.
        /// Different stream IDs produce different sequences.
        /// WARNING: Streams are not statistically independent (Vigna 2020).
        /// Prefer hash-based seeding via SplitMix64.Derive() for parallel RNG.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pcg32(ulong seed, ulong streamId)
        {
            increment = (streamId << 1) | 1UL;
            state = 0UL;
            state = state * Multiplier + increment;
            state += SplitMix64.Mix(seed);
            state = state * Multiplier + increment;
        }

        /// <summary>
        /// Creates a PCG32 from explicit state. For deserialization/save-load only.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pcg32 FromState(ulong state, ulong increment)
        {
            Pcg32 rng;
            rng.state = state;
            rng.increment = increment | 1UL;
            return rng;
        }

        /// <summary>Returns the next 32-bit random value and advances state.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            ulong old = state;
            state = old * Multiplier + increment;

            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return BitOps.RotateRight(xorShifted, rot);
        }

        /// <summary>Returns a random int in [0, int.MaxValue].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt()
        {
            return (int)(NextUInt() >> 1);
        }

        /// <summary>Returns a random int in [0, maxExclusive). maxExclusive must be > 0.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive,
                    "maxExclusive must be > 0.");
            return (int)LemireRange.Sample((uint)maxExclusive, ref this);
        }

        /// <summary>Returns a random int in [min, maxExclusive). min must be &lt; maxExclusive.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int min, int maxExclusive)
        {
            if (min >= maxExclusive)
                throw new System.ArgumentException(
                    $"min ({min}) must be < maxExclusive ({maxExclusive}).");
            uint range = (uint)(maxExclusive - min);
            return min + (int)LemireRange.Sample(range, ref this);
        }

        /// <summary>Returns a random ulong by combining two 32-bit outputs.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextULong()
        {
            uint hi = NextUInt();
            uint lo = NextUInt();
            return ((ulong)hi << 32) | lo;
        }

        /// <summary>Returns a float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat()
        {
            return FloatConversion.ToFloat01(NextUInt());
        }

        /// <summary>Returns a float in (0, 1]. Safe for log-based transforms.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloatNonZero()
        {
            return FloatConversion.ToFloatNonZero(NextUInt());
        }

        /// <summary>Returns a float in [-1, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloatSigned()
        {
            return FloatConversion.ToFloatSigned(NextUInt());
        }

        /// <summary>Returns a float in [min, max).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max)
        {
            return min + NextFloat() * (max - min);
        }

        /// <summary>Returns a double in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble()
        {
            return FloatConversion.ToDouble01(NextULong());
        }

        /// <summary>Returns true with 50% probability.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool()
        {
            return (NextUInt() & 1u) == 1u;
        }

        /// <summary>Returns true with the given probability in [0, 1].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool(float probability)
        {
            return NextFloat() < probability;
        }

        /// <summary>
        /// Advances the state by <paramref name="delta"/> steps in O(log delta) time.
        /// Useful for save/load and replay seeking.
        /// </summary>
        public void Advance(ulong delta)
        {
            ulong curMult = Multiplier;
            ulong curPlus = increment;
            ulong accMult = 1UL;
            ulong accPlus = 0UL;

            while (delta > 0)
            {
                if ((delta & 1) != 0)
                {
                    accMult *= curMult;
                    accPlus = accPlus * curMult + curPlus;
                }
                curPlus = (curMult + 1UL) * curPlus;
                curMult *= curMult;
                delta >>= 1;
            }

            state = accMult * state + accPlus;
        }

        /// <summary>Current internal state. For serialization only. Exposes full PRNG state.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ulong State => state;

        /// <summary>Current increment. For serialization only. Exposes full PRNG state.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ulong Increment => increment;
    }
}
