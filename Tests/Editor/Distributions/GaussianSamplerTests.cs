// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Distributions;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class GaussianSamplerTests
    {
        [Test]
        public void SamplePolar_MeanConverges()
        {
            var rng = new Pcg32(42UL);
            const int samples = 100000;
            const float expectedMean = 5f;
            const float stddev = 2f;

            double sum = 0;
            for (int i = 0; i < samples; i++)
                sum += GaussianSampler.SamplePolar(ref rng, expectedMean, stddev);

            float actualMean = (float)(sum / samples);
            Assert.AreEqual(expectedMean, actualMean, 0.05f,
                $"Mean should converge to {expectedMean}, got {actualMean}");
        }

        [Test]
        public void SamplePolar_StdDevConverges()
        {
            var rng = new Pcg32(99UL);
            const int samples = 100000;
            const float mean = 0f;
            const float expectedStddev = 3f;

            double sum = 0, sumSq = 0;
            for (int i = 0; i < samples; i++)
            {
                double v = GaussianSampler.SamplePolar(ref rng, mean, expectedStddev);
                sum += v;
                sumSq += v * v;
            }

            double actualMean = sum / samples;
            double variance = sumSq / samples - actualMean * actualMean;
            float actualStddev = (float)Math.Sqrt(variance);

            Assert.AreEqual(expectedStddev, actualStddev, 0.1f,
                $"StdDev should converge to {expectedStddev}, got {actualStddev}");
        }

        [Test]
        public void SamplePolar_Deterministic()
        {
            var a = new Pcg32(123UL);
            var b = new Pcg32(123UL);

            for (int i = 0; i < 1000; i++)
            {
                float va = GaussianSampler.SamplePolar(ref a, 0f, 1f);
                float vb = GaussianSampler.SamplePolar(ref b, 0f, 1f);
                Assert.AreEqual(va, vb, $"Diverged at iteration {i}");
            }
        }

        [Test]
        public void SamplePolar2_ProducesTwoValues()
        {
            var rng = new Pcg32(456UL);

            GaussianSampler.SamplePolar2(ref rng, 0f, 1f, out float z0, out float z1);

            // Both should be finite, typically in [-4, 4] range
            Assert.IsFalse(float.IsNaN(z0));
            Assert.IsFalse(float.IsNaN(z1));
            Assert.IsFalse(float.IsInfinity(z0));
            Assert.IsFalse(float.IsInfinity(z1));
        }

        [Test]
        public void SampleBoxMuller_NoInfinity_NoNaN()
        {
            var rng = new Pcg32(789UL);

            for (int i = 0; i < 100000; i++)
            {
                float v = GaussianSampler.SampleBoxMuller(ref rng, 0f, 1f);
                Assert.IsFalse(float.IsNaN(v), $"NaN at iteration {i}");
                Assert.IsFalse(float.IsInfinity(v), $"Infinity at iteration {i}");
            }
        }
    }
}
