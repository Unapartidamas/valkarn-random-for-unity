// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Weighted shuffle via Efraimidis-Spirakis key method.
// Reference: Pavlos S. Efraimidis & Paul G. Spirakis, "Weighted Random Sampling
//            with a Reservoir", Information Processing Letters, 2006.
//
// Each item gets key = u^(1/w) where u ~ Uniform(0,1) and w = weight.
// Sort by key descending → weighted shuffle.
// Higher-weight items tend to appear earlier but randomization is preserved.

using System;
using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Selection
{
    public static class WeightedShuffle
    {
        /// <summary>
        /// Shuffles items in-place weighted by the corresponding weights.
        /// Higher-weight items are more likely to appear earlier.
        /// O(n log n) due to sort.
        /// </summary>
        public static void Shuffle<T>(T[] items, float[] weights, ref ValkarnRandom rng)
        {
            if (items.Length != weights.Length)
                throw new ArgumentException("Items and weights must have the same length.");

            int n = items.Length;
            if (n <= 1) return;

            var keys = new float[n];
            for (int i = 0; i < n; i++)
            {
                float u = rng.NextFloatNonZero(); // (0, 1] to avoid log(0)
                // key = u^(1/w) = exp(log(u) / w)
                // Higher weight → larger key → earlier position
                keys[i] = MathF.Exp(MathF.Log(u) / MathF.Max(weights[i], float.Epsilon));
            }

            // Sort items by key descending (largest key first)
            Array.Sort(keys, items);
            Array.Reverse(items);
        }

        /// <summary>
        /// Returns the indices of the top k items selected by weighted sampling
        /// without replacement. O(n log n) via sort.
        /// </summary>
        public static int[] SelectTopK(float[] weights, int k, ref ValkarnRandom rng)
        {
            int n = weights.Length;
            if (k > n) k = n;

            var keys = new float[n];
            var indices = new int[n];

            for (int i = 0; i < n; i++)
            {
                float u = rng.NextFloatNonZero();
                keys[i] = MathF.Exp(MathF.Log(u) / MathF.Max(weights[i], float.Epsilon));
                indices[i] = i;
            }

            Array.Sort(keys, indices);

            // Take last k (highest keys)
            var result = new int[k];
            for (int i = 0; i < k; i++)
                result[i] = indices[n - 1 - i];

            return result;
        }
    }
}
