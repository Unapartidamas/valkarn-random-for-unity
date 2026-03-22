// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Selection;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class FisherYatesTests
    {
        [Test]
        public void Shuffle_PreservesAllElements()
        {
            var rng = ValkarnRandom.Create(42UL);
            var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            FisherYates.Shuffle(array, ref rng);

            CollectionAssert.AreEquivalent(
                new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                array,
                "Shuffle must preserve all elements");
        }

        [Test]
        public void Shuffle_Deterministic()
        {
            var a = ValkarnRandom.Create(42UL);
            var b = ValkarnRandom.Create(42UL);

            var arrayA = new[] { 1, 2, 3, 4, 5 };
            var arrayB = new[] { 1, 2, 3, 4, 5 };

            FisherYates.Shuffle(arrayA, ref a);
            FisherYates.Shuffle(arrayB, ref b);

            CollectionAssert.AreEqual(arrayA, arrayB);
        }

        [Test]
        public void Shuffle_SmallArray_AllPermutationsReachable()
        {
            // For n=3, there are 6 permutations. Run many shuffles and check all appear.
            var permutations = new HashSet<string>();
            var rng = ValkarnRandom.Create(99UL);

            for (int i = 0; i < 10000; i++)
            {
                var array = new[] { 1, 2, 3 };
                FisherYates.Shuffle(array, ref rng);
                permutations.Add(string.Join(",", array));
            }

            Assert.AreEqual(6, permutations.Count, "Not all 6 permutations of [1,2,3] were reached");
        }

        [Test]
        public void PartialShuffle_SelectsKElements()
        {
            var rng = ValkarnRandom.Create(123UL);
            var array = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            int k = 3;

            FisherYates.PartialShuffle(array, k, ref rng);

            // The last k elements are the selection
            var selected = array.Skip(array.Length - k).ToArray();
            Assert.AreEqual(k, selected.Length);

            // All selected elements must be from the original array
            var original = new HashSet<int> { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            foreach (int v in selected)
                Assert.IsTrue(original.Contains(v));
        }
    }
}
