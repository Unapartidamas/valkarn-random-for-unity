// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Selection;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class WeightedShuffleTests
    {
        [Test]
        public void Shuffle_PreservesAllElements()
        {
            var rng = ValkarnRandom.Create(42UL);
            var items = new[] { "A", "B", "C", "D", "E" };
            var weights = new[] { 5f, 1f, 1f, 1f, 1f };

            WeightedShuffle.Shuffle(items, weights, ref rng);

            CollectionAssert.AreEquivalent(
                new[] { "A", "B", "C", "D", "E" }, items);
        }

        [Test]
        public void Shuffle_HighWeightItemAppearsEarlier()
        {
            const int trials = 10000;
            int heavyFirst = 0;

            for (int t = 0; t < trials; t++)
            {
                var rng = ValkarnRandom.Create((ulong)t);
                var items = new[] { "heavy", "light1", "light2", "light3" };
                var weights = new[] { 100f, 1f, 1f, 1f };

                WeightedShuffle.Shuffle(items, weights, ref rng);

                if (items[0] == "heavy")
                    heavyFirst++;
            }

            float ratio = heavyFirst / (float)trials;
            Assert.Greater(ratio, 0.8f,
                $"Heavy item should appear first most of the time, got {ratio * 100:F1}%");
        }

        [Test]
        public void Shuffle_EqualWeights_AllPositionsReachable()
        {
            var firstPositionCounts = new int[3];

            for (int t = 0; t < 10000; t++)
            {
                var rng = ValkarnRandom.Create((ulong)t);
                var items = new[] { "A", "B", "C" };
                var weights = new[] { 1f, 1f, 1f };

                WeightedShuffle.Shuffle(items, weights, ref rng);
                firstPositionCounts[Array.IndexOf(new[] { "A", "B", "C" }, items[0])]++;
            }

            for (int i = 0; i < 3; i++)
                Assert.Greater(firstPositionCounts[i], 2000,
                    $"Item {i} was first only {firstPositionCounts[i]} times");
        }

        [Test]
        public void Shuffle_Deterministic()
        {
            var a = ValkarnRandom.Create(42UL);
            var b = ValkarnRandom.Create(42UL);

            var itemsA = new[] { 1, 2, 3, 4, 5 };
            var itemsB = new[] { 1, 2, 3, 4, 5 };
            var weights = new[] { 5f, 4f, 3f, 2f, 1f };

            WeightedShuffle.Shuffle(itemsA, weights, ref a);
            WeightedShuffle.Shuffle(itemsB, weights, ref b);

            CollectionAssert.AreEqual(itemsA, itemsB);
        }

        [Test]
        public void Shuffle_MismatchedLengths_Throws()
        {
            var rng = ValkarnRandom.Create(99UL);
            Assert.Throws<ArgumentException>(() =>
                WeightedShuffle.Shuffle(new[] { 1, 2 }, new[] { 1f }, ref rng));
        }

        [Test]
        public void SelectTopK_ReturnsCorrectCount()
        {
            var rng = ValkarnRandom.Create(123UL);
            var weights = new[] { 3f, 5f, 1f, 2f, 4f };

            var result = WeightedShuffle.SelectTopK(weights, 3, ref rng);

            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(result.All(i => i >= 0 && i < 5));
            Assert.AreEqual(result.Distinct().Count(), result.Length, "Indices must be unique");
        }

        [Test]
        public void SelectTopK_HighWeightsPreferred()
        {
            const int trials = 5000;
            int heaviestSelected = 0;

            for (int t = 0; t < trials; t++)
            {
                var rng = ValkarnRandom.Create((ulong)t);
                var weights = new[] { 100f, 1f, 1f, 1f, 1f };
                var result = WeightedShuffle.SelectTopK(weights, 2, ref rng);

                if (result.Contains(0))
                    heaviestSelected++;
            }

            Assert.Greater(heaviestSelected, trials * 0.9f,
                "Heaviest item should almost always be selected");
        }
    }
}
