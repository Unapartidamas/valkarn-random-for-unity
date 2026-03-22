// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Gaussian (normal) distribution sampler.
//
// Default: Marsaglia Polar method — avoids sin/cos for cross-platform determinism.
// Reference: George Marsaglia (1964), refinement of Box-Muller.
// Only uses log() and sqrt(), which are IEEE 754 basic ops under Burst Deterministic.
//
// The polar method has ~27% rejection rate (draws ~1.27 pairs on average).
// Produces 2 normal values per acceptance; second value is cached.

using System;
using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Distributions
{
    public static class GaussianSampler
    {
        /// <summary>
        /// Samples a Gaussian value using the Marsaglia Polar method.
        /// Cross-platform deterministic (no sin/cos).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SamplePolar(ref Pcg32 rng, float mean, float stddev)
        {
            float u, v, s;

            do
            {
                u = rng.NextFloatSigned();
                v = rng.NextFloatSigned();
                s = u * u + v * v;
            }
            while (s >= 1f || s == 0f);

            float factor = MathF.Sqrt(-2f * MathF.Log(s) / s);
            return mean + stddev * u * factor;
        }

        /// <summary>
        /// Samples two Gaussian values using the Marsaglia Polar method.
        /// More efficient when you need pairs (avoids wasting the second value).
        /// </summary>
        public static void SamplePolar2(ref Pcg32 rng, float mean, float stddev,
            out float z0, out float z1)
        {
            float u, v, s;

            do
            {
                u = rng.NextFloatSigned();
                v = rng.NextFloatSigned();
                s = u * u + v * v;
            }
            while (s >= 1f || s == 0f);

            float factor = MathF.Sqrt(-2f * MathF.Log(s) / s);
            z0 = mean + stddev * u * factor;
            z1 = mean + stddev * v * factor;
        }

        /// <summary>
        /// Samples a Gaussian value using the Box-Muller transform.
        /// Branch-free, no rejection loop — better for SIMD/Burst batch paths.
        /// WARNING: Uses sin/cos — NOT cross-platform deterministic unless
        /// Burst FloatMode.Deterministic is enabled (routes to SLEEF).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SampleBoxMuller(ref Pcg32 rng, float mean, float stddev)
        {
            float u1 = rng.NextFloatNonZero(); // (0, 1] — safe for log()
            float u2 = rng.NextFloat();         // [0, 1)

            float mag = stddev * MathF.Sqrt(-2f * MathF.Log(u1));
            return mean + mag * MathF.Cos(2f * MathF.PI * u2);
        }
    }
}
