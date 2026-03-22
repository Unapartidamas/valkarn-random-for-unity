// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class Philox4x32Tests
    {
        [Test]
        public void NextUInt_SameSeed_SameSequence()
        {
            var a = new Philox4x32(42UL);
            var b = new Philox4x32(42UL);

            for (int i = 0; i < 10000; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), $"Diverged at iteration {i}");
        }

        [Test]
        public void NextUInt_DifferentSeeds_DifferentSequences()
        {
            var a = new Philox4x32(42UL);
            var b = new Philox4x32(99UL);

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
        public void ComputeStateless_Deterministic()
        {
            Philox4x32.ComputeStateless(0, 0, 0xDEADBEEF, 0xCAFEBABE,
                out uint a0, out uint a1, out uint a2, out uint a3);
            Philox4x32.ComputeStateless(0, 0, 0xDEADBEEF, 0xCAFEBABE,
                out uint b0, out uint b1, out uint b2, out uint b3);

            Assert.AreEqual(a0, b0);
            Assert.AreEqual(a1, b1);
            Assert.AreEqual(a2, b2);
            Assert.AreEqual(a3, b3);
        }

        [Test]
        public void ComputeStateless_DifferentCounters_DifferentOutput()
        {
            Philox4x32.ComputeStateless(0, 0, 123, 456,
                out uint a0, out _, out _, out _);
            Philox4x32.ComputeStateless(1, 0, 123, 456,
                out uint b0, out _, out _, out _);

            Assert.AreNotEqual(a0, b0);
        }

        [Test]
        public void ComputeStateless_DifferentKeys_DifferentOutput()
        {
            Philox4x32.ComputeStateless(0, 0, 100, 200,
                out uint a0, out _, out _, out _);
            Philox4x32.ComputeStateless(0, 0, 101, 200,
                out uint b0, out _, out _, out _);

            Assert.AreNotEqual(a0, b0);
        }

        [Test]
        public void NextFloat_InRange()
        {
            var rng = new Philox4x32(777UL);

            for (int i = 0; i < 100000; i++)
            {
                float f = rng.NextFloat();
                Assert.GreaterOrEqual(f, 0f);
                Assert.Less(f, 1f);
            }
        }

        [Test]
        public void FourOutputsPerCounter_AreDistinct()
        {
            // Philox produces 4 values per counter tick; they should generally differ
            var rng = new Philox4x32(888UL);
            uint a = rng.NextUInt();
            uint b = rng.NextUInt();
            uint c = rng.NextUInt();
            uint d = rng.NextUInt();

            // At least 2 of 4 should be different (extremely unlikely all equal)
            bool allSame = (a == b) && (b == c) && (c == d);
            Assert.IsFalse(allSame, "All four outputs from one counter are identical");
        }
    }
}
