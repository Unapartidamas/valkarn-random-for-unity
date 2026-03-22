// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Internal;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class LemireRangeTests
    {
        [Test]
        public void Sample_AlwaysInRange()
        {
            var rng = new Pcg32(42UL);
            const uint range = 7;

            for (int i = 0; i < 100000; i++)
            {
                uint v = LemireRange.Sample(range, ref rng);
                Assert.Less(v, range, $"Value {v} >= range {range} at iteration {i}");
            }
        }

        [Test]
        public void Sample_UniformDistribution_ChiSquared()
        {
            var rng = new Pcg32(99UL);
            const uint range = 13;
            const int samples = 100000;
            var counts = new int[range];

            for (int i = 0; i < samples; i++)
                counts[LemireRange.Sample(range, ref rng)]++;

            float expected = samples / (float)range;
            float chiSquared = 0f;

            for (int i = 0; i < (int)range; i++)
            {
                float diff = counts[i] - expected;
                chiSquared += diff * diff / expected;
            }

            // Chi-squared critical value for 12 df at p=0.001 is ~32.91
            Assert.Less(chiSquared, 32.91f, $"Not uniform (χ² = {chiSquared:F2})");
        }

        [Test]
        public void Sample_RangeOne_AlwaysReturnsZero()
        {
            var rng = new Pcg32(123UL);

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(0u, LemireRange.Sample(1, ref rng));
        }

        [Test]
        public void Sample_LargeRange_InBounds()
        {
            var rng = new Pcg32(456UL);
            const uint range = 0x80000000u; // 2^31

            for (int i = 0; i < 10000; i++)
            {
                uint v = LemireRange.Sample(range, ref rng);
                Assert.Less(v, range);
            }
        }

        [Test]
        public void Sample_Deterministic()
        {
            var a = new Pcg32(789UL);
            var b = new Pcg32(789UL);

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(
                    LemireRange.Sample(100, ref a),
                    LemireRange.Sample(100, ref b));
        }
    }
}
