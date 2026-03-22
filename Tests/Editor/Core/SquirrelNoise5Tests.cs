// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class SquirrelNoise5Tests
    {
        [Test]
        public void Get1D_Deterministic()
        {
            uint a = SquirrelNoise5.Get1D(100, 42);
            uint b = SquirrelNoise5.Get1D(100, 42);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Get1D_DifferentPositions_DifferentOutput()
        {
            uint a = SquirrelNoise5.Get1D(0, 42);
            uint b = SquirrelNoise5.Get1D(1, 42);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Get1D_DifferentSeeds_DifferentOutput()
        {
            uint a = SquirrelNoise5.Get1D(0, 0);
            uint b = SquirrelNoise5.Get1D(0, 1);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Get2D_Deterministic()
        {
            uint a = SquirrelNoise5.Get2D(10, 20, 42);
            uint b = SquirrelNoise5.Get2D(10, 20, 42);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Get3D_Deterministic()
        {
            uint a = SquirrelNoise5.Get3D(1, 2, 3, 42);
            uint b = SquirrelNoise5.Get3D(1, 2, 3, 42);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Get1DFloat_InRange()
        {
            for (int i = 0; i < 100000; i++)
            {
                float f = SquirrelNoise5.Get1DFloat(i, 0);
                Assert.GreaterOrEqual(f, 0f);
                Assert.Less(f, 1f);
            }
        }

        [Test]
        public void Get1D_Avalanche_SingleBitChange_AffectsOutput()
        {
            // Rough avalanche test: flipping one bit in position should change ~50% of output bits
            int totalBitDiffs = 0;
            const int samples = 10000;

            for (int i = 0; i < samples; i++)
            {
                uint a = SquirrelNoise5.Get1D(i, 0);
                uint b = SquirrelNoise5.Get1D(i ^ 1, 0); // flip lowest bit
                uint diff = a ^ b;

                // Count bits that differ
                int bits = 0;
                while (diff != 0)
                {
                    bits += (int)(diff & 1);
                    diff >>= 1;
                }
                totalBitDiffs += bits;
            }

            float avgBitDiff = totalBitDiffs / (float)samples;
            // Ideal avalanche: 16 bits differ on average (50% of 32)
            // Accept 12-20 range for a non-cryptographic hash
            Assert.Greater(avgBitDiff, 12f, $"Poor avalanche: avg {avgBitDiff:F1} bits differ");
            Assert.Less(avgBitDiff, 20f, $"Suspicious avalanche: avg {avgBitDiff:F1} bits differ");
        }
    }
}
