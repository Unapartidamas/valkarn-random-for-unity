// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// SplitMix64 — seed mixer and hash finalizer.
// Reference: Guy L. Steele Jr., Doug Lea, Christine H. Flood,
//            "Fast Splittable Pseudorandom Number Generators", OOPSLA 2014.
//
// State: 8 bytes (single ulong). Period: 2^64.
// Passes TestU01 BigCrush and Dieharder.
// Primary use: expand a user seed into quality state for other PRNGs.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnaPartidaMas.Valkarn.Random
{
    [StructLayout(LayoutKind.Auto)]
    public struct SplitMix64
    {
        ulong state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMix64(ulong seed)
        {
            state = seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Next()
        {
            ulong z = state += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// One-shot hash: mixes a single value without needing an instance.
        /// Useful for deriving seeds from entity IDs, positions, etc.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Mix(ulong value)
        {
            ulong z = value + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Derives a seed by combining a base seed with an index/id.
        /// Deterministic: same (seed, id) always produces the same result.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Derive(ulong seed, ulong id)
        {
            return Mix(seed ^ (id * 0x517CC1B727220A95UL));
        }
    }
}
