// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Distributions;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class GeometricSamplersTests
    {
        [Test]
        public void OnDisk_AllPointsInsideUnitCircle()
        {
            var rng = new Pcg32(42UL);

            for (int i = 0; i < 10000; i++)
            {
                GeometricSamplers.OnDisk(ref rng, out float x, out float y);
                float r2 = x * x + y * y;
                Assert.Less(r2, 1f, $"Point ({x}, {y}) is outside unit circle");
            }
        }

        [Test]
        public void OnSphere_AllPointsOnUnitSphere()
        {
            var rng = new Pcg32(99UL);

            for (int i = 0; i < 10000; i++)
            {
                GeometricSamplers.OnSphere(ref rng, out float x, out float y, out float z);
                float r = MathF.Sqrt(x * x + y * y + z * z);
                Assert.AreEqual(1f, r, 0.01f,
                    $"Point ({x}, {y}, {z}) has radius {r}, expected 1.0");
            }
        }

        [Test]
        public void InSphere_AllPointsInsideUnitSphere()
        {
            var rng = new Pcg32(123UL);

            for (int i = 0; i < 10000; i++)
            {
                GeometricSamplers.InSphere(ref rng, out float x, out float y, out float z);
                float r2 = x * x + y * y + z * z;
                Assert.Less(r2, 1f, $"Point ({x}, {y}, {z}) is outside unit sphere");
            }
        }

        [Test]
        public void OnHemisphereCosine_AllPointsPositiveZ()
        {
            var rng = new Pcg32(456UL);

            for (int i = 0; i < 10000; i++)
            {
                GeometricSamplers.OnHemisphereCosine(ref rng, out float x, out float y, out float z);
                Assert.GreaterOrEqual(z, 0f, $"z = {z} is negative");
                float r = MathF.Sqrt(x * x + y * y + z * z);
                Assert.AreEqual(1f, r, 0.01f, $"Not on unit hemisphere: r = {r}");
            }
        }

        [Test]
        public void InCone_AllPointsWithinConeAngle()
        {
            var rng = new Pcg32(789UL);
            float halfAngle = MathF.PI / 6f; // 30 degrees
            float cosMin = MathF.Cos(halfAngle);

            for (int i = 0; i < 10000; i++)
            {
                GeometricSamplers.InCone(ref rng, halfAngle, out float x, out float y, out float z);
                float r = MathF.Sqrt(x * x + y * y + z * z);

                // Normalize and check angle with +Z
                float cosAngle = z / r;
                Assert.GreaterOrEqual(cosAngle, cosMin - 0.01f,
                    $"Point outside cone: cos(angle) = {cosAngle}, min = {cosMin}");
            }
        }
    }
}
