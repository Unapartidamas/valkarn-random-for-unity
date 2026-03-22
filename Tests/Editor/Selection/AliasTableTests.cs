// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Selection;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class AliasTableTests
    {
        [Test]
        public void Sample_UniformWeights_ApproximatelyEqual()
        {
            var weights = new float[] { 1f, 1f, 1f, 1f };
            var table = new AliasTable(weights);
            var rng = ValkarnRandom.Create(42UL);

            const int samples = 100000;
            var counts = new int[4];

            for (int i = 0; i < samples; i++)
                counts[table.Sample(ref rng)]++;

            float expected = samples / 4f;
            for (int i = 0; i < 4; i++)
            {
                float ratio = counts[i] / expected;
                Assert.AreEqual(1f, ratio, 0.05f,
                    $"Bucket {i}: expected ~{expected}, got {counts[i]}");
            }
        }

        [Test]
        public void Sample_SkewedWeights_MatchesDistribution()
        {
            var weights = new float[] { 7f, 2f, 1f };
            var table = new AliasTable(weights);
            var rng = ValkarnRandom.Create(99UL);

            const int samples = 100000;
            var counts = new int[3];

            for (int i = 0; i < samples; i++)
                counts[table.Sample(ref rng)]++;

            float total = 10f;
            Assert.AreEqual(0.7f, counts[0] / (float)samples, 0.02f, "70% bucket");
            Assert.AreEqual(0.2f, counts[1] / (float)samples, 0.02f, "20% bucket");
            Assert.AreEqual(0.1f, counts[2] / (float)samples, 0.02f, "10% bucket");
        }

        [Test]
        public void Sample_SingleItem_AlwaysReturnZero()
        {
            var table = new AliasTable(new float[] { 1f });
            var rng = ValkarnRandom.Create(123UL);

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(0, table.Sample(ref rng));
        }

        [Test]
        public void Constructor_EmptyWeights_Throws()
        {
            Assert.Throws<ArgumentException>(() => new AliasTable(Array.Empty<float>()));
        }

        [Test]
        public void Count_MatchesInputLength()
        {
            var table = new AliasTable(new float[] { 1f, 2f, 3f, 4f, 5f });
            Assert.AreEqual(5, table.Count);
        }

        [Test]
        public void Sample_Deterministic()
        {
            var weights = new float[] { 3f, 5f, 2f };
            var table = new AliasTable(weights);

            var a = ValkarnRandom.Create(42UL);
            var b = ValkarnRandom.Create(42UL);

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(table.Sample(ref a), table.Sample(ref b));
        }
    }
}
