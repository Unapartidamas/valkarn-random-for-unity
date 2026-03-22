// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Fairness;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class ShuffleBagTests
    {
        [Test]
        public void Draw_ExactDistributionPerCycle()
        {
            var bag = new ShuffleBag<string>(new[] { "A", "A", "A", "B", "B", "C" });
            var rng = ValkarnRandom.Create(42UL);

            // Draw one full cycle
            int aCount = 0, bCount = 0, cCount = 0;
            for (int i = 0; i < 6; i++)
            {
                string item = bag.Draw(ref rng);
                switch (item)
                {
                    case "A": aCount++; break;
                    case "B": bCount++; break;
                    case "C": cCount++; break;
                }
            }

            Assert.AreEqual(3, aCount, "Exactly 3 A's per cycle");
            Assert.AreEqual(2, bCount, "Exactly 2 B's per cycle");
            Assert.AreEqual(1, cCount, "Exactly 1 C per cycle");
        }

        [Test]
        public void Draw_AutoReshuffles()
        {
            var bag = new ShuffleBag<int>(new[] { 1, 2, 3 });
            var rng = ValkarnRandom.Create(99UL);

            // Draw 2 full cycles (6 items)
            for (int i = 0; i < 6; i++)
                bag.Draw(ref rng);

            // Should still work (auto-reshuffled)
            int v = bag.Draw(ref rng);
            Assert.IsTrue(v >= 1 && v <= 3);
        }

        [Test]
        public void CreateWeighted_CorrectDistribution()
        {
            var bag = ShuffleBag<string>.CreateWeighted(
                ("common", 7),
                ("rare", 2),
                ("epic", 1)
            );

            Assert.AreEqual(10, bag.Size);

            var rng = ValkarnRandom.Create(123UL);
            int common = 0, rare = 0, epic = 0;

            for (int i = 0; i < 10; i++)
            {
                switch (bag.Draw(ref rng))
                {
                    case "common": common++; break;
                    case "rare": rare++; break;
                    case "epic": epic++; break;
                }
            }

            Assert.AreEqual(7, common);
            Assert.AreEqual(2, rare);
            Assert.AreEqual(1, epic);
        }

        [Test]
        public void Remaining_DecrementsCorrectly()
        {
            var bag = new ShuffleBag<int>(new[] { 1, 2, 3, 4, 5 });
            var rng = ValkarnRandom.Create(456UL);

            Assert.AreEqual(5, bag.Remaining);
            bag.Draw(ref rng);
            Assert.AreEqual(4, bag.Remaining);
            bag.Draw(ref rng);
            Assert.AreEqual(3, bag.Remaining);
        }

        [Test]
        public void Draw_Deterministic()
        {
            var bagA = new ShuffleBag<int>(new[] { 1, 2, 3, 4, 5 });
            var bagB = new ShuffleBag<int>(new[] { 1, 2, 3, 4, 5 });
            var rngA = ValkarnRandom.Create(42UL);
            var rngB = ValkarnRandom.Create(42UL);

            for (int i = 0; i < 20; i++)
                Assert.AreEqual(bagA.Draw(ref rngA), bagB.Draw(ref rngB));
        }
    }
}
