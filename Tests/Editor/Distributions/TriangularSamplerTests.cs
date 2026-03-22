// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Distributions;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class TriangularSamplerTests
    {
        [Test]
        public void Sample_AllValuesInRange()
        {
            var rng = new Pcg32(42UL);
            const float min = 2f, max = 8f, mode = 5f;

            for (int i = 0; i < 100000; i++)
            {
                float v = TriangularSampler.Sample(ref rng, min, max, mode);
                Assert.GreaterOrEqual(v, min, $"Value {v} < min at iteration {i}");
                Assert.LessOrEqual(v, max, $"Value {v} > max at iteration {i}");
            }
        }

        [Test]
        public void Sample_ModeIsApproximateMean_Symmetric()
        {
            var rng = new Pcg32(99UL);
            const float min = 0f, max = 10f, mode = 5f;
            const int samples = 100000;
            // For symmetric triangular, mean = (min + max + mode) / 3
            const float expectedMean = (min + max + mode) / 3f;

            double sum = 0;
            for (int i = 0; i < samples; i++)
                sum += TriangularSampler.Sample(ref rng, min, max, mode);

            float actualMean = (float)(sum / samples);
            Assert.AreEqual(expectedMean, actualMean, 0.05f,
                $"Mean should be ~{expectedMean:F2}, got {actualMean:F2}");
        }

        [Test]
        public void Sample_ModeAtMin_SkewsRight()
        {
            var rng = new Pcg32(123UL);
            const int samples = 10000;
            int belowMid = 0;

            for (int i = 0; i < samples; i++)
            {
                float v = TriangularSampler.Sample(ref rng, 0f, 10f, 0f);
                if (v < 5f) belowMid++;
            }

            // With mode=min, most values should be below midpoint
            Assert.Greater(belowMid, samples * 0.6f, "Should skew toward min");
        }

        [Test]
        public void Sample_Deterministic()
        {
            var a = new Pcg32(456UL);
            var b = new Pcg32(456UL);

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(
                    TriangularSampler.Sample(ref a, 1f, 5f, 3f),
                    TriangularSampler.Sample(ref b, 1f, 5f, 3f));
        }

        [Test]
        public void Sample_MinEqualsMax_Throws()
        {
            var rng = new Pcg32(789UL);
            Assert.Throws<ArgumentException>(() =>
                TriangularSampler.Sample(ref rng, 5f, 5f, 5f));
        }

        [Test]
        public void Sample_ModeOutsideRange_Throws()
        {
            var rng = new Pcg32(111UL);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TriangularSampler.Sample(ref rng, 0f, 10f, 15f));
        }
    }
}
