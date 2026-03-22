// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Vose's Alias Method — O(n) build, O(1) weighted sampling.
// Reference: Michael D. Vose, "A Linear Algorithm for Generating Random Numbers
//            with a Given Distribution", IEEE TSE 17(9), 1991.
//
// Build once from weights, sample repeatedly in constant time.
// Ideal for loot tables, spawn weights, AI decision weights.
// Two random numbers per sample: one for bucket index, one for alias choice.

using System;
using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Selection
{
    public sealed class AliasTable
    {
        readonly float[] probabilities;
        readonly int[] aliases;

        /// <summary>
        /// Builds an alias table from weights. Weights do not need to sum to 1.
        /// O(n) time and space.
        /// </summary>
        public AliasTable(ReadOnlySpan<float> weights)
        {
            int n = weights.Length;
            if (n == 0)
                throw new ArgumentException("Weights must not be empty.", nameof(weights));

            probabilities = new float[n];
            aliases = new int[n];

            float sum = 0f;
            for (int i = 0; i < n; i++)
            {
                if (weights[i] < 0f)
                    throw new ArgumentException($"Weight at index {i} is negative ({weights[i]}).", nameof(weights));
                sum += weights[i];
            }

            if (sum <= 0f)
                throw new ArgumentException("At least one weight must be positive.", nameof(weights));

            float scale = n / sum;
            Span<float> scaled = n <= 256 ? stackalloc float[n] : new float[n];
            for (int i = 0; i < n; i++)
                scaled[i] = weights[i] * scale;

            // Small and large stacks (Vose's algorithm)
            Span<int> small = n <= 256 ? stackalloc int[n] : new int[n];
            Span<int> large = n <= 256 ? stackalloc int[n] : new int[n];
            int smallCount = 0, largeCount = 0;

            for (int i = 0; i < n; i++)
            {
                if (scaled[i] < 1f)
                    small[smallCount++] = i;
                else
                    large[largeCount++] = i;
            }

            while (smallCount > 0 && largeCount > 0)
            {
                int s = small[--smallCount];
                int l = large[--largeCount];

                probabilities[s] = scaled[s];
                aliases[s] = l;

                scaled[l] = (scaled[l] + scaled[s]) - 1f;

                if (scaled[l] < 1f)
                    small[smallCount++] = l;
                else
                    large[largeCount++] = l;
            }

            while (largeCount > 0)
            {
                int l = large[--largeCount];
                probabilities[l] = 1f;
                aliases[l] = l;
            }

            while (smallCount > 0)
            {
                int s = small[--smallCount];
                probabilities[s] = 1f;
                aliases[s] = s;
            }
        }

        /// <summary>
        /// Builds from an array of weights.
        /// </summary>
        public AliasTable(float[] weights) : this(weights.AsSpan()) { }

        /// <summary>Number of items in the table.</summary>
        public int Count => probabilities.Length;

        /// <summary>
        /// Samples a random index in O(1) time according to the original weights.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sample(ref ValkarnRandom rng)
        {
            int i = rng.Range(probabilities.Length);
            float u = rng.NextFloat();
            return u < probabilities[i] ? i : aliases[i];
        }

        /// <summary>
        /// Samples using a raw Pcg32 reference (for internal/Burst-adjacent use).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sample(ref Pcg32 rng)
        {
            int i = rng.NextInt(probabilities.Length);
            float u = rng.NextFloat();
            return u < probabilities[i] ? i : aliases[i];
        }
    }
}
