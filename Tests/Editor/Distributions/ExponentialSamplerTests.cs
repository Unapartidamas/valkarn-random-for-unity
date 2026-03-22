// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Distributions;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class ExponentialSamplerTests
    {
        [Test]
        public void Sample_AllValuesPositive()
        {
            var rng = new Pcg32(42UL);

            for (int i = 0; i < 100000; i++)
            {
                float v = ExponentialSampler.Sample(ref rng, 1f);
                Assert.Greater(v, 0f, $"Value <= 0 at iteration {i}");
                Assert.IsFalse(float.IsNaN(v), $"NaN at iteration {i}");
                Assert.IsFalse(float.IsInfinity(v), $"Infinity at iteration {i}");
            }
        }

        [Test]
        public void Sample_MeanConverges()
        {
            var rng = new Pcg32(99UL);
            const float rate = 3f;
            const float expectedMean = 1f / rate;
            const int samples = 100000;

            double sum = 0;
            for (int i = 0; i < samples; i++)
                sum += ExponentialSampler.Sample(ref rng, rate);

            float actualMean = (float)(sum / samples);
            Assert.AreEqual(expectedMean, actualMean, 0.01f,
                $"Mean should be ~{expectedMean:F3}, got {actualMean:F3}");
        }

        [Test]
        public void Sample_Deterministic()
        {
            var a = new Pcg32(123UL);
            var b = new Pcg32(123UL);

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(
                    ExponentialSampler.Sample(ref a, 2f),
                    ExponentialSampler.Sample(ref b, 2f));
        }

        [Test]
        public void Sample_ZeroRate_Throws()
        {
            var rng = new Pcg32(456UL);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ExponentialSampler.Sample(ref rng, 0f));
        }

        [Test]
        public void Sample_NegativeRate_Throws()
        {
            var rng = new Pcg32(789UL);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ExponentialSampler.Sample(ref rng, -1f));
        }
    }
}
