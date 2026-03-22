// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// xoshiro256** — high-quality 64-bit PRNG with provably non-overlapping jump functions.
// Reference: David Blackman & Sebastiano Vigna, "Scrambled Linear Pseudorandom
//            Number Generators", ACM TOMS, 2021. https://prng.di.unimi.it/
//
// State: 32 bytes (4 x ulong). Period: 2^256 - 1.
// Jump: advance 2^128 steps in O(1). LongJump: advance 2^192 steps.
// Passes TestU01 BigCrush and PractRand.
//
// Use Jump() to create provably non-overlapping parallel streams.
// Each stream has 2^128 values — enough for any game simulation.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnaPartidaMas.Valkarn.Random.Internal;

namespace UnaPartidaMas.Valkarn.Random
{
    [StructLayout(LayoutKind.Auto)]
    public struct Xoshiro256StarStar
    {
        ulong s0, s1, s2, s3;

        /// <summary>
        /// Creates from a seed. Uses SplitMix64 to expand into 4 state words.
        /// Guarantees non-zero state (all-zero is the absorbing state).
        /// </summary>
        public Xoshiro256StarStar(ulong seed)
        {
            var sm = new SplitMix64(seed);
            s0 = sm.Next();
            s1 = sm.Next();
            s2 = sm.Next();
            s3 = sm.Next();
        }

        /// <summary>
        /// Creates from explicit state. For deserialization only.
        /// Throws if all state words are zero (absorbing state — would produce only zeros).
        /// </summary>
        public static Xoshiro256StarStar FromState(ulong s0, ulong s1, ulong s2, ulong s3)
        {
            if ((s0 | s1 | s2 | s3) == 0)
                throw new System.ArgumentException(
                    "State must not be all-zero (absorbing state for xoshiro256**).");

            Xoshiro256StarStar rng;
            rng.s0 = s0;
            rng.s1 = s1;
            rng.s2 = s2;
            rng.s3 = s3;
            return rng;
        }

        /// <summary>Returns the next 64-bit random value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextULong()
        {
            ulong result = BitOps.RotateLeft(s1 * 5, 7) * 9;
            ulong t = s1 << 17;

            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;
            s3 = BitOps.RotateLeft(s3, 45);

            return result;
        }

        /// <summary>Returns the next 32-bit random value (upper 32 bits of 64-bit output).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            return (uint)(NextULong() >> 32);
        }

        /// <summary>Returns a float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat()
        {
            return FloatConversion.ToFloat01(NextUInt());
        }

        /// <summary>Returns a double in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble()
        {
            return FloatConversion.ToDouble01(NextULong());
        }

        /// <summary>
        /// Advances the state by 2^128 steps. Equivalent to calling NextULong() 2^128 times.
        /// Use to create provably non-overlapping parallel streams:
        /// stream_i = base.Jump() called i times.
        /// Each stream has 2^128 non-overlapping values.
        /// </summary>
        public void Jump()
        {
            ulong js0 = 0x180EC6D33CFD0ABAUL;
            ulong js1 = 0xD5A61266F0C9392CUL;
            ulong js2 = 0xA9582618E03FC9AAUL;
            ulong js3 = 0x39ABDC4529B1661CUL;

            ulong t0 = 0, t1 = 0, t2 = 0, t3 = 0;

            JumpImpl(js0, js1, js2, js3, ref t0, ref t1, ref t2, ref t3);

            s0 = t0;
            s1 = t1;
            s2 = t2;
            s3 = t3;
        }

        /// <summary>
        /// Advances the state by 2^192 steps. Use to create 2^64 non-overlapping
        /// groups of 2^64 streams each (via Jump() within each group).
        /// </summary>
        public void LongJump()
        {
            ulong js0 = 0x76E15D3EFEFDCBBFUL;
            ulong js1 = 0xC5004E441C522FB3UL;
            ulong js2 = 0x77710069854EE241UL;
            ulong js3 = 0x39109BB02ACBE635UL;

            ulong t0 = 0, t1 = 0, t2 = 0, t3 = 0;

            JumpImpl(js0, js1, js2, js3, ref t0, ref t1, ref t2, ref t3);

            s0 = t0;
            s1 = t1;
            s2 = t2;
            s3 = t3;
        }

        void JumpImpl(
            ulong js0, ulong js1, ulong js2, ulong js3,
            ref ulong t0, ref ulong t1, ref ulong t2, ref ulong t3)
        {
            // Iterate over each polynomial word without heap allocation.
            JumpWord(js0, ref t0, ref t1, ref t2, ref t3);
            JumpWord(js1, ref t0, ref t1, ref t2, ref t3);
            JumpWord(js2, ref t0, ref t1, ref t2, ref t3);
            JumpWord(js3, ref t0, ref t1, ref t2, ref t3);
        }

        void JumpWord(ulong word,
            ref ulong t0, ref ulong t1, ref ulong t2, ref ulong t3)
        {
            for (int b = 0; b < 64; b++)
            {
                if ((word & (1UL << b)) != 0)
                {
                    t0 ^= s0;
                    t1 ^= s1;
                    t2 ^= s2;
                    t3 ^= s3;
                }
                NextULong();
            }
        }

        /// <summary>
        /// Creates N non-overlapping parallel streams from a single seed.
        /// Each stream has 2^128 values guaranteed disjoint.
        /// </summary>
        public static Xoshiro256StarStar[] CreateParallelStreams(ulong seed, int count)
        {
            var streams = new Xoshiro256StarStar[count];
            var baseRng = new Xoshiro256StarStar(seed);

            for (int i = 0; i < count; i++)
            {
                streams[i] = baseRng;
                baseRng.Jump();
            }

            return streams;
        }

        public ulong S0 => s0;
        public ulong S1 => s1;
        public ulong S2 => s2;
        public ulong S3 => s3;
    }
}
