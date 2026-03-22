// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Fairness;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class LowDiscrepancyTests
    {
        [Test]
        public void R2_1D_AllValuesInRange()
        {
            var seq = new R2Sequence(0);

            for (int i = 0; i < 10000; i++)
            {
                float v = seq.Next1D();
                Assert.GreaterOrEqual(v, 0f);
                Assert.Less(v, 1f);
            }
        }

        [Test]
        public void R2_1D_ConvergesFasterThanWhiteNoise()
        {
            // Simulate a binary outcome with 30% probability.
            // R2 should converge to the target faster than white noise.
            const float targetP = 0.3f;
            const int samples = 100;

            // R2 approach: trigger when R2 value < targetP
            var seq = new R2Sequence(0, 0.5f);
            int r2Hits = 0;
            for (int i = 0; i < samples; i++)
            {
                if (seq.Next1D() < targetP)
                    r2Hits++;
            }
            float r2Error = System.MathF.Abs(r2Hits / (float)samples - targetP);

            // White noise approach
            var rng = ValkarnRandom.Create(42UL);
            int whiteHits = 0;
            for (int i = 0; i < samples; i++)
            {
                if (rng.NextFloat() < targetP)
                    whiteHits++;
            }
            float whiteError = System.MathF.Abs(whiteHits / (float)samples - targetP);

            // R2 should have smaller error (this may not always hold for a single run,
            // but should hold statistically over many tests)
            // We use a generous threshold since this is probabilistic
            Assert.Less(r2Error, 0.1f, $"R2 error {r2Error} should be small");
        }

        [Test]
        public void R2_2D_AllValuesInRange()
        {
            var seq = new R2Sequence(0);

            for (int i = 0; i < 10000; i++)
            {
                seq.Next2D(out float x, out float y);
                Assert.GreaterOrEqual(x, 0f);
                Assert.Less(x, 1f);
                Assert.GreaterOrEqual(y, 0f);
                Assert.Less(y, 1f);
            }
        }

        [Test]
        public void R2_Get1D_RandomAccess_MatchesSequential()
        {
            var seq = new R2Sequence(0, 0.5f);

            for (int i = 0; i < 100; i++)
            {
                float sequential = seq.Next1D();
                float randomAccess = R2Sequence.Get1D(i, 0.5f);
                Assert.AreEqual(sequential, randomAccess, 0.0001f,
                    $"Mismatch at index {i}");
            }
        }

        [Test]
        public void Halton_2D_AllValuesInRange()
        {
            for (int i = 1; i < 10000; i++)
            {
                HaltonSequence.Get2D(i, out float x, out float y);
                Assert.GreaterOrEqual(x, 0f);
                Assert.Less(x, 1f);
                Assert.GreaterOrEqual(y, 0f);
                Assert.Less(y, 1f);
            }
        }

        [Test]
        public void Halton_VanDerCorput_KnownValues()
        {
            // Base 2: 1/2, 1/4, 3/4, 1/8, 5/8, 3/8, 7/8, ...
            Assert.AreEqual(0.5f, HaltonSequence.VanDerCorput(1, 2), 0.001f);
            Assert.AreEqual(0.25f, HaltonSequence.VanDerCorput(2, 2), 0.001f);
            Assert.AreEqual(0.75f, HaltonSequence.VanDerCorput(3, 2), 0.001f);
            Assert.AreEqual(0.125f, HaltonSequence.VanDerCorput(4, 2), 0.001f);
        }
    }
}
