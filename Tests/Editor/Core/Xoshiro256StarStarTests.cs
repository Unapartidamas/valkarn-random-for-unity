// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class Xoshiro256StarStarTests
    {
        [Test]
        public void NextULong_SameSeed_SameSequence()
        {
            var a = new Xoshiro256StarStar(42UL);
            var b = new Xoshiro256StarStar(42UL);

            for (int i = 0; i < 10000; i++)
                Assert.AreEqual(a.NextULong(), b.NextULong(), $"Diverged at iteration {i}");
        }

        [Test]
        public void Constructor_SeedZero_NonZeroState()
        {
            // Xoshiro256** has absorbing zero state.
            // SplitMix64 seeding must guarantee non-zero.
            var rng = new Xoshiro256StarStar(0UL);
            Assert.IsTrue(rng.S0 != 0 || rng.S1 != 0 || rng.S2 != 0 || rng.S3 != 0,
                "State must not be all-zero after seeding with 0");
        }

        [Test]
        public void Jump_ProducesNonOverlappingStreams()
        {
            var streams = Xoshiro256StarStar.CreateParallelStreams(99UL, 4);

            // Each stream should produce different values
            ulong[] firstValues = new ulong[4];
            for (int i = 0; i < 4; i++)
                firstValues[i] = streams[i].NextULong();

            for (int i = 0; i < 4; i++)
                for (int j = i + 1; j < 4; j++)
                    Assert.AreNotEqual(firstValues[i], firstValues[j],
                        $"Streams {i} and {j} produced identical first values");
        }

        [Test]
        public void Jump_Deterministic()
        {
            var a = new Xoshiro256StarStar(55UL);
            var b = new Xoshiro256StarStar(55UL);

            a.Jump();
            b.Jump();

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(a.NextULong(), b.NextULong());
        }

        [Test]
        public void LongJump_Deterministic()
        {
            var a = new Xoshiro256StarStar(77UL);
            var b = new Xoshiro256StarStar(77UL);

            a.LongJump();
            b.LongJump();

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(a.NextULong(), b.NextULong());
        }

        [Test]
        public void NextFloat_InRange()
        {
            var rng = new Xoshiro256StarStar(100UL);

            for (int i = 0; i < 100000; i++)
            {
                float f = rng.NextFloat();
                Assert.GreaterOrEqual(f, 0f);
                Assert.Less(f, 1f);
            }
        }

        [Test]
        public void FromState_RestoresExactly()
        {
            var rng = new Xoshiro256StarStar(200UL);
            for (int i = 0; i < 50; i++)
                rng.NextULong();

            var restored = Xoshiro256StarStar.FromState(rng.S0, rng.S1, rng.S2, rng.S3);

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(rng.NextULong(), restored.NextULong());
        }
    }
}
