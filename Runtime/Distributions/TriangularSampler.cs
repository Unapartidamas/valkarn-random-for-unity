// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Triangular distribution sampler via inverse CDF.
// Bounded range [min, max] with a mode (most likely value).
// Useful for "roughly centered" random: AI decisions, damage variance, NPC reaction times.
// Simpler than normal distribution — no outliers, intuitive parameterization.

using System;
using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Distributions
{
    public static class TriangularSampler
    {
        /// <summary>
        /// Samples from Triangular(min, max, mode).
        /// Returns a value in [min, max] with peak probability at mode.
        /// </summary>
        public static float Sample(ref Pcg32 rng, float min, float max, float mode)
        {
            if (min >= max)
                throw new ArgumentException($"min ({min}) must be < max ({max}).");
            if (mode < min || mode > max)
                throw new ArgumentOutOfRangeException(nameof(mode), mode,
                    $"mode must be in [{min}, {max}].");

            float u = rng.NextFloat();
            float range = max - min;
            float f = (mode - min) / range;

            if (u < f)
                return min + MathF.Sqrt(u * range * (mode - min));
            else
                return max - MathF.Sqrt((1f - u) * range * (max - mode));
        }
    }
}
