// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Geometric distribution samplers: disk, sphere, hemisphere, cone.
// For: particle systems, physics impulses, projectile spread, ambient occlusion.
//
// References:
// - Marsaglia (1972): sphere surface sampling via rejection
// - Marc B. Reynolds, "Uniform points in sphere and capped cone" (2018)
// - PBR Book, "2D Sampling with Multidimensional Transformations"

using System;
using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Distributions
{
    public static class GeometricSamplers
    {
        /// <summary>
        /// Uniform point on the unit disk (rejection method).
        /// ~78.5% acceptance rate (π/4).
        /// </summary>
        public static void OnDisk(ref Pcg32 rng, out float x, out float y)
        {
            float r2;
            do
            {
                x = rng.NextFloatSigned();
                y = rng.NextFloatSigned();
                r2 = x * x + y * y;
            }
            while (r2 >= 1f || r2 == 0f);
        }

        /// <summary>
        /// Uniform point inside the unit disk (including interior).
        /// Same rejection method as OnDisk — all accepted points are uniformly distributed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InDisk(ref Pcg32 rng, out float x, out float y)
        {
            OnDisk(ref rng, out x, out y);
        }

        /// <summary>
        /// Uniform point on the unit sphere surface (Marsaglia method).
        /// Uses rejection sampling in unit disk then projects to sphere.
        /// </summary>
        public static void OnSphere(ref Pcg32 rng, out float x, out float y, out float z)
        {
            float u, v, s;
            do
            {
                u = rng.NextFloatSigned();
                v = rng.NextFloatSigned();
                s = u * u + v * v;
            }
            while (s >= 1f || s == 0f);

            float factor = 2f * MathF.Sqrt(1f - s);
            x = u * factor;
            y = v * factor;
            z = 1f - 2f * s;
        }

        /// <summary>
        /// Uniform point inside the unit sphere (rejection method).
        /// ~52.4% acceptance rate (4π/3 / 8 = π/6).
        /// </summary>
        public static void InSphere(ref Pcg32 rng, out float x, out float y, out float z)
        {
            float r2;
            do
            {
                x = rng.NextFloatSigned();
                y = rng.NextFloatSigned();
                z = rng.NextFloatSigned();
                r2 = x * x + y * y + z * z;
            }
            while (r2 >= 1f || r2 == 0f);
        }

        /// <summary>
        /// Cosine-weighted point on the upper hemisphere (Malley's method).
        /// Projects uniform disk sample to hemisphere. PDF ∝ cos(θ).
        /// Ideal for diffuse lighting, ambient occlusion.
        /// </summary>
        public static void OnHemisphereCosine(ref Pcg32 rng,
            out float x, out float y, out float z)
        {
            OnDisk(ref rng, out x, out y);
            z = MathF.Sqrt(MathF.Max(0f, 1f - x * x - y * y));
        }

        /// <summary>
        /// Uniform point on a spherical cap (cone) with the given half-angle in radians.
        /// Oriented along +Z. Transform the result to your desired direction.
        /// Uses sin/cos — NOT cross-platform deterministic without Burst Deterministic.
        /// </summary>
        public static void InCone(ref Pcg32 rng, float halfAngleRadians,
            out float x, out float y, out float z)
        {
            float cosMax = MathF.Cos(halfAngleRadians);
            float u = rng.NextFloat();
            float cosTheta = 1f - u * (1f - cosMax);
            float sinTheta = MathF.Sqrt(MathF.Max(0f, 1f - cosTheta * cosTheta));
            float phi = 2f * MathF.PI * rng.NextFloat();

            x = sinTheta * MathF.Cos(phi);
            y = sinTheta * MathF.Sin(phi);
            z = cosTheta;
        }
    }
}
