// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Fisher-Yates (Knuth) shuffle — O(n) in-place, unbiased.
// Uses Lemire's "nearly divisionless" range reduction to avoid modulo bias.
// Reference: R.A. Fisher & F. Yates (1938), Richard Durftenfeld (1964).
//
// CRITICAL: The random integer must be in [0, i] inclusive, NOT [0, n-1].
// Using [0, n-1] produces biased results.

using System;
using System.Runtime.CompilerServices;
using UnaPartidaMas.Valkarn.Random.Internal;

namespace UnaPartidaMas.Valkarn.Random.Selection
{
    public static class FisherYates
    {
        /// <summary>
        /// Shuffles the array in-place. Every permutation is equally likely.
        /// </summary>
        public static void Shuffle<T>(T[] array, ref ValkarnRandom rng)
        {
            Shuffle(array.AsSpan(), ref rng);
        }

        /// <summary>
        /// Shuffles the span in-place. Every permutation is equally likely.
        /// </summary>
        public static void Shuffle<T>(Span<T> span, ref ValkarnRandom rng)
        {
            for (int i = span.Length - 1; i > 0; i--)
            {
                int j = rng.Range(i + 1);
                (span[i], span[j]) = (span[j], span[i]);
            }
        }

        /// <summary>
        /// Partial shuffle: randomly selects k elements from the array.
        /// After this call, the last k elements of the array are the selection.
        /// O(k) time, O(1) extra space. Modifies the array in-place.
        /// </summary>
        public static void PartialShuffle<T>(T[] array, int k, ref ValkarnRandom rng)
        {
            PartialShuffle(array.AsSpan(), k, ref rng);
        }

        /// <summary>
        /// Partial shuffle: randomly selects k elements from the span.
        /// After this call, the last k elements are the random selection.
        /// </summary>
        public static void PartialShuffle<T>(Span<T> span, int k, ref ValkarnRandom rng)
        {
            int n = span.Length;
            if (k > n) k = n;

            for (int i = n - 1; i >= n - k; i--)
            {
                int j = rng.Range(i + 1);
                (span[i], span[j]) = (span[j], span[i]);
            }
        }
    }
}
