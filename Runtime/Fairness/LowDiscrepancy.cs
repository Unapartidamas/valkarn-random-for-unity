// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Low-discrepancy (quasi-random) sequences for "better-than-random" distribution.
//
// R2 sequence: Martin Roberts, "The Unreasonable Effectiveness of Quasirandom Sequences".
// Halton sequence: J.H. Halton, "On the efficiency of certain quasi-random sequences", 1960.
//
// Key finding (Alan Wolfe / demofox, 2020): white noise needs 1,380-50,513 rolls to
// converge to correct drop probabilities. Low-discrepancy achieves this in 9-353 rolls.
// Orders of magnitude improvement in perceived fairness for loot systems.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnaPartidaMas.Valkarn.Random.Fairness
{
    /// <summary>
    /// R2 sequence — generalized golden ratio in N dimensions.
    /// Incrementally extensible, parameter-free, minimum packing distance ~1/sqrt(n).
    /// Superior to Halton in higher dimensions.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct R2Sequence
    {
        // Plastic constant φ₂ = 1.32471795724474602596...
        // α₁ = 1/φ₂ ≈ 0.7548776662466927
        // α₂ = 1/φ₂² ≈ 0.5698402909980532
        const float Alpha1 = 0.7548776662466927f;
        const float Alpha2 = 0.5698402909980532f;
        const float Alpha3 = 0.4560260619084403f; // 1/φ₃³ for 3D variant

        int index;
        readonly float offset;

        /// <summary>Creates an R2 sequence starting at the given index with a random offset.</summary>
        public R2Sequence(int startIndex, float offset = 0.5f)
        {
            index = startIndex;
            this.offset = offset;
        }

        /// <summary>Returns the next 1D value in [0, 1) and advances.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Next1D()
        {
            float value = Frac(offset + Alpha1 * index);
            index++;
            return value;
        }

        /// <summary>Returns the next 2D point in [0,1)² and advances.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next2D(out float x, out float y)
        {
            x = Frac(offset + Alpha1 * index);
            y = Frac(offset + Alpha2 * index);
            index++;
        }

        /// <summary>Returns the next 3D point in [0,1)³ and advances.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next3D(out float x, out float y, out float z)
        {
            x = Frac(offset + Alpha1 * index);
            y = Frac(offset + Alpha2 * index);
            z = Frac(offset + Alpha3 * index);
            index++;
        }

        /// <summary>
        /// Gets the Nth 1D value without advancing. Random access.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get1D(int n, float offset = 0.5f)
        {
            return Frac(offset + Alpha1 * n);
        }

        /// <summary>Current index in the sequence.</summary>
        public int Index => index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Frac(float x)
        {
            float result = x - MathF.Floor(x);
            // MathF.Floor-based frac is always in [0, 1) for finite x.
            return result;
        }
    }

    /// <summary>
    /// Halton sequence — van der Corput in coprime bases.
    /// Simple, well-understood, good for 2D. Degrades in high dimensions.
    /// </summary>
    public static class HaltonSequence
    {
        /// <summary>Van der Corput sequence in the given base at the given index.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float VanDerCorput(int index, int @base)
        {
            float result = 0f;
            float bk = 1f / @base;
            int n = index;

            while (n > 0)
            {
                result += (n % @base) * bk;
                n /= @base;
                bk /= @base;
            }

            return result;
        }

        /// <summary>2D Halton point (bases 2, 3) at the given index.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Get2D(int index, out float x, out float y)
        {
            x = VanDerCorput(index, 2);
            y = VanDerCorput(index, 3);
        }

        /// <summary>3D Halton point (bases 2, 3, 5) at the given index.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Get3D(int index, out float x, out float y, out float z)
        {
            x = VanDerCorput(index, 2);
            y = VanDerCorput(index, 3);
            z = VanDerCorput(index, 5);
        }
    }
}
