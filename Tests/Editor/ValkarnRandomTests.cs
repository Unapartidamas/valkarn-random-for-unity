// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class ValkarnRandomTests
    {
        [Test]
        public void Create_SameSeed_SameSequence()
        {
            var a = ValkarnRandom.Create(42UL);
            var b = ValkarnRandom.Create(42UL);

            for (int i = 0; i < 10000; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void Fork_Deterministic()
        {
            var a = ValkarnRandom.Create(100UL);
            var b = ValkarnRandom.Create(100UL);

            var childA = a.Fork();
            var childB = b.Fork();

            for (int i = 0; i < 1000; i++)
                Assert.AreEqual(childA.NextUInt(), childB.NextUInt());
        }

        [Test]
        public void Fork_ChildDiffersFromParent()
        {
            var parent = ValkarnRandom.Create(200UL);
            var child = parent.Fork();

            bool anyDifferent = false;
            for (int i = 0; i < 100; i++)
            {
                if (parent.NextUInt() != child.NextUInt())
                {
                    anyDifferent = true;
                    break;
                }
            }

            Assert.IsTrue(anyDifferent);
        }

        [Test]
        public void ForkWithId_DifferentIds_DifferentChildren()
        {
            var parentA = ValkarnRandom.Create(300UL);
            var parentB = ValkarnRandom.Create(300UL);

            var childA = parentA.Fork(0UL);
            var childB = parentB.Fork(1UL);

            bool anyDifferent = false;
            for (int i = 0; i < 100; i++)
            {
                if (childA.NextUInt() != childB.NextUInt())
                {
                    anyDifferent = true;
                    break;
                }
            }

            Assert.IsTrue(anyDifferent);
        }

        [Test]
        public void Range_Int_AlwaysInBounds()
        {
            var rng = ValkarnRandom.Create(400UL);

            for (int i = 0; i < 100000; i++)
            {
                int v = rng.Range(5, 15);
                Assert.GreaterOrEqual(v, 5);
                Assert.Less(v, 15);
            }
        }

        [Test]
        public void Range_Float_AlwaysInBounds()
        {
            var rng = ValkarnRandom.Create(500UL);

            for (int i = 0; i < 100000; i++)
            {
                float v = rng.Range(-1f, 1f);
                Assert.GreaterOrEqual(v, -1f);
                Assert.Less(v, 1f);
            }
        }

        [Test]
        public void Choose_ReturnsElementFromArray()
        {
            var rng = ValkarnRandom.Create(600UL);
            var items = new[] { "a", "b", "c", "d" };

            for (int i = 0; i < 1000; i++)
            {
                string chosen = rng.Choose(items);
                Assert.Contains(chosen, items);
            }
        }

        [Test]
        public void FromState_RestoresExactly()
        {
            var rng = ValkarnRandom.Create(700UL);
            for (int i = 0; i < 50; i++)
                rng.NextUInt();

            ulong state = rng.State;
            ulong inc = rng.Increment;

            uint expected = rng.NextUInt();

            var restored = ValkarnRandom.FromState(state, inc);
            Assert.AreEqual(expected, restored.NextUInt());
        }
    }
}
