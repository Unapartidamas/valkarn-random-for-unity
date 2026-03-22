// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class Pcg32Tests
    {
        [Test]
        public void NextUInt_SameSeed_ProducesSameSequence()
        {
            var a = new Pcg32(42UL);
            var b = new Pcg32(42UL);

            for (int i = 0; i < 10000; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), $"Diverged at iteration {i}");
        }

        [Test]
        public void NextUInt_DifferentSeeds_ProduceDifferentSequences()
        {
            var a = new Pcg32(42UL);
            var b = new Pcg32(99UL);

            bool anyDifferent = false;
            for (int i = 0; i < 100; i++)
            {
                if (a.NextUInt() != b.NextUInt())
                {
                    anyDifferent = true;
                    break;
                }
            }

            Assert.IsTrue(anyDifferent);
        }

        [Test]
        public void NextFloat_AlwaysInRange_ZeroToOne()
        {
            var rng = new Pcg32(123UL);

            for (int i = 0; i < 100000; i++)
            {
                float f = rng.NextFloat();
                Assert.GreaterOrEqual(f, 0f, $"Iteration {i}: value {f} < 0");
                Assert.Less(f, 1f, $"Iteration {i}: value {f} >= 1");
            }
        }

        [Test]
        public void NextFloatNonZero_NeverZero_AlwaysPositive()
        {
            var rng = new Pcg32(456UL);

            for (int i = 0; i < 100000; i++)
            {
                float f = rng.NextFloatNonZero();
                Assert.Greater(f, 0f, $"Iteration {i}: value was 0");
                Assert.LessOrEqual(f, 1f, $"Iteration {i}: value {f} > 1");
            }
        }

        [Test]
        public void NextFloatSigned_InRange_NegOneToOne()
        {
            var rng = new Pcg32(789UL);

            for (int i = 0; i < 100000; i++)
            {
                float f = rng.NextFloatSigned();
                Assert.GreaterOrEqual(f, -1f, $"Iteration {i}: value {f} < -1");
                Assert.Less(f, 1f, $"Iteration {i}: value {f} >= 1");
            }
        }

        [Test]
        public void NextInt_Range_AlwaysInBounds()
        {
            var rng = new Pcg32(111UL);

            for (int i = 0; i < 100000; i++)
            {
                int v = rng.NextInt(10, 20);
                Assert.GreaterOrEqual(v, 10);
                Assert.Less(v, 20);
            }
        }

        [Test]
        public void NextInt_Range_UniformDistribution_ChiSquared()
        {
            var rng = new Pcg32(222UL);
            const int buckets = 10;
            const int samples = 100000;
            var counts = new int[buckets];

            for (int i = 0; i < samples; i++)
                counts[rng.NextInt(buckets)]++;

            float expected = samples / (float)buckets;
            float chiSquared = 0f;

            for (int i = 0; i < buckets; i++)
            {
                float diff = counts[i] - expected;
                chiSquared += diff * diff / expected;
            }

            // Chi-squared critical value for 9 df at p=0.001 is ~27.88
            Assert.Less(chiSquared, 27.88f, $"Distribution is not uniform (χ² = {chiSquared})");
        }

        [Test]
        public void Advance_SkipAhead_MatchesSequentialCalls()
        {
            var sequential = new Pcg32(333UL);
            var skipped = new Pcg32(333UL);

            const ulong skipCount = 1000;

            for (ulong i = 0; i < skipCount; i++)
                sequential.NextUInt();

            skipped.Advance(skipCount);

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(sequential.NextUInt(), skipped.NextUInt(), $"Diverged at {i} after advance");
        }

        [Test]
        public void FromState_RestoresExactly()
        {
            var rng = new Pcg32(444UL);

            for (int i = 0; i < 50; i++)
                rng.NextUInt();

            ulong savedState = rng.State;
            ulong savedInc = rng.Increment;

            uint expected1 = rng.NextUInt();
            uint expected2 = rng.NextUInt();

            var restored = Pcg32.FromState(savedState, savedInc);
            Assert.AreEqual(expected1, restored.NextUInt());
            Assert.AreEqual(expected2, restored.NextUInt());
        }

        [Test]
        public void NextBool_Probability_ApproximatelyCorrect()
        {
            var rng = new Pcg32(555UL);
            const int samples = 100000;
            int trueCount = 0;

            for (int i = 0; i < samples; i++)
            {
                if (rng.NextBool(0.3f))
                    trueCount++;
            }

            float ratio = trueCount / (float)samples;
            Assert.AreEqual(0.3f, ratio, 0.02f, $"Expected ~30%, got {ratio * 100f:F1}%");
        }
    }
}
