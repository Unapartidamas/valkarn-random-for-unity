// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class ValkarnNoiseTests
    {
        [Test]
        public void Hash_1D_Deterministic()
        {
            uint a = ValkarnNoise.Hash(42, 0);
            uint b = ValkarnNoise.Hash(42, 0);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Hash_2D_Deterministic()
        {
            uint a = ValkarnNoise.Hash(10, 20, 0);
            uint b = ValkarnNoise.Hash(10, 20, 0);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Hash_3D_Deterministic()
        {
            uint a = ValkarnNoise.Hash(1, 2, 3, 42);
            uint b = ValkarnNoise.Hash(1, 2, 3, 42);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Float_1D_InRange()
        {
            for (int i = 0; i < 10000; i++)
            {
                float f = ValkarnNoise.Float(i, 0);
                Assert.GreaterOrEqual(f, 0f);
                Assert.Less(f, 1f);
            }
        }

        [Test]
        public void Float_2D_InRange()
        {
            for (int y = 0; y < 100; y++)
                for (int x = 0; x < 100; x++)
                {
                    float f = ValkarnNoise.Float(x, y, 42);
                    Assert.GreaterOrEqual(f, 0f);
                    Assert.Less(f, 1f);
                }
        }

        [Test]
        public void DeriveSeed_Deterministic()
        {
            ulong a = ValkarnNoise.DeriveSeed(100UL, 5UL);
            ulong b = ValkarnNoise.DeriveSeed(100UL, 5UL);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void DeriveSeed_DifferentIds_DifferentResults()
        {
            ulong a = ValkarnNoise.DeriveSeed(100UL, 0UL);
            ulong b = ValkarnNoise.DeriveSeed(100UL, 1UL);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Hash_DifferentPositions_DifferentResults()
        {
            uint a = ValkarnNoise.Hash(0, 42);
            uint b = ValkarnNoise.Hash(1, 42);
            Assert.AreNotEqual(a, b);
        }
    }
}
