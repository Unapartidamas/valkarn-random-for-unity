// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Exponential distribution sampler via inverse CDF.
// Reference: Jeff Preshing, "How to Generate Random Timings for a Poisson Process", 2011.
//
// Use for: enemy spawn timing, item drop intervals, damage-over-time tick timing.
// The inter-arrival times of a Poisson process are exponentially distributed.
// Mean = 1/rate. Memoryless property: P(X > s+t | X > s) = P(X > t).

using System;
using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Distributions
{
    public static class ExponentialSampler
    {
        /// <summary>
        /// Samples from Exponential(rate). Returns a value in (0, +inf).
        /// Mean = 1/rate, Variance = 1/rate^2.
        /// Uses (0,1] float to avoid log(0) = -inf.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sample(ref Pcg32 rng, float rate)
        {
            if (rate <= 0f)
                throw new ArgumentOutOfRangeException(nameof(rate), rate, "Rate must be positive.");
            return -MathF.Log(rng.NextFloatNonZero()) / rate;
        }
    }
}
