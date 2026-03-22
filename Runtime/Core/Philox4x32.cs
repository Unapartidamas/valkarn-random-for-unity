// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Philox-4x32-7 — counter-based PRNG with strongest independence guarantees.
// Reference: John K. Salmon, Mark A. Moraes, Ron O. Dror, David E. Shaw,
//            "Parallel Random Numbers: As Easy as 1, 2, 3", SC11, 2011.
//
// State: 24 bytes (4x32 counter + 2x32 key). No mutable shared state.
// Each call to Next() produces 4 independent uint32 values.
// 7 rounds (reduced from 10 — sufficient for game applications, passes BigCrush).
//
// Ideal for Burst Jobs: each job gets a unique key, counter = work item index.
// Zero synchronization needed between threads.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnaPartidaMas.Valkarn.Random
{
    [StructLayout(LayoutKind.Auto)]
    public struct Philox4x32
    {
        const uint MulConstA = 0xD2511F53U;
        const uint MulConstB = 0xCD9E8D57U;
        const uint WeylConstA = 0x9E3779B9U;
        const uint WeylConstB = 0xBB67AE85U;

        uint c0, c1, c2, c3;
        uint k0, k1;

        // Buffered output from last round
        uint r0, r1, r2, r3;
        int bufferIndex;

        /// <summary>
        /// Creates a Philox4x32 PRNG from a seed.
        /// Counter starts at 0, key derived from seed via SplitMix64.
        /// </summary>
        public Philox4x32(ulong seed)
        {
            ulong mixed = SplitMix64.Mix(seed);
            k0 = (uint)mixed;
            k1 = (uint)(mixed >> 32);
            c0 = c1 = c2 = c3 = 0;
            r0 = r1 = r2 = r3 = 0;
            bufferIndex = 4; // force generation on first call
        }

        /// <summary>
        /// Creates a Philox4x32 with explicit key and starting counter.
        /// For Burst Jobs: key = job seed, counter = work item index.
        /// </summary>
        public Philox4x32(uint key0, uint key1, uint counter0)
        {
            k0 = key0;
            k1 = key1;
            c0 = counter0;
            c1 = c2 = c3 = 0;
            r0 = r1 = r2 = r3 = 0;
            bufferIndex = 4;
        }

        /// <summary>Returns the next 32-bit random value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            if (bufferIndex >= 4)
            {
                Generate();
                bufferIndex = 0;
            }

            switch (bufferIndex++)
            {
                case 0: return r0;
                case 1: return r1;
                case 2: return r2;
                default: return r3;
            }
        }

        /// <summary>Returns a float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat()
        {
            return Internal.FloatConversion.ToFloat01(NextUInt());
        }

        /// <summary>
        /// Generates 4 random values from the current counter, then increments counter.
        /// Pure function of (counter, key) — deterministic.
        /// </summary>
        void Generate()
        {
            r0 = c0; r1 = c1; r2 = c2; r3 = c3;
            uint rk0 = k0, rk1 = k1;

            for (int i = 0; i < 7; i++)
            {
                PhiloxRound(ref r0, ref r1, ref r2, ref r3, rk0, rk1);
                rk0 += WeylConstA;
                rk1 += WeylConstB;
            }

            IncrementCounter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void PhiloxRound(ref uint v0, ref uint v1, ref uint v2, ref uint v3, uint k0, uint k1)
        {
            ulong prodA = (ulong)MulConstA * v0;
            ulong prodB = (ulong)MulConstB * v2;

            uint hiA = (uint)(prodA >> 32);
            uint loA = (uint)prodA;
            uint hiB = (uint)(prodB >> 32);
            uint loB = (uint)prodB;

            v0 = hiB ^ v1 ^ k0;
            v1 = loB;
            v2 = hiA ^ v3 ^ k1;
            v3 = loA;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IncrementCounter()
        {
            if (++c0 == 0)
                if (++c1 == 0)
                    if (++c2 == 0)
                        ++c3;
        }

        /// <summary>
        /// Stateless: compute 4 random values from (counter, key) without advancing state.
        /// Ideal for random-access patterns in Burst Jobs.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComputeStateless(
            uint counter0, uint counter1, uint key0, uint key1,
            out uint out0, out uint out1, out uint out2, out uint out3)
        {
            out0 = counter0; out1 = counter1; out2 = 0; out3 = 0;
            uint rk0 = key0, rk1 = key1;

            for (int i = 0; i < 7; i++)
            {
                PhiloxRound(ref out0, ref out1, ref out2, ref out3, rk0, rk1);
                rk0 += WeylConstA;
                rk1 += WeylConstB;
            }
        }
    }
}
