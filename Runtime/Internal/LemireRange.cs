// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Debiased integer range reduction.
// Reference: Daniel Lemire, "Fast Random Integer Generation in an Interval",
//            ACM TOMS 45(1), 2019.

using System.Runtime.CompilerServices;
using UnaPartidaMas.Valkarn.Random;

namespace UnaPartidaMas.Valkarn.Random.Internal
{
    internal static class LemireRange
    {
        /// <summary>
        /// Returns a uniformly distributed uint in [0, range) without modulo bias.
        /// <paramref name="nextUInt"/> is called to obtain raw random bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint Sample(uint range, ref Pcg32 rng)
        {
            ulong m = (ulong)rng.NextUInt() * range;
            uint l = (uint)m;

            if (l < range)
            {
                uint t = (uint)(-(int)range) % range;
                while (l < t)
                {
                    m = (ulong)rng.NextUInt() * range;
                    l = (uint)m;
                }
            }

            return (uint)(m >> 32);
        }

        /// <summary>
        /// Returns a uniformly distributed uint in [0, range) using a raw uint value.
        /// May need additional raw values via <paramref name="rng"/> for rejection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint SampleWithFirst(uint raw, uint range, ref Pcg32 rng)
        {
            ulong m = (ulong)raw * range;
            uint l = (uint)m;

            if (l < range)
            {
                uint t = (uint)(-(int)range) % range;
                while (l < t)
                {
                    m = (ulong)rng.NextUInt() * range;
                    l = (uint)m;
                }
            }

            return (uint)(m >> 32);
        }
    }
}
