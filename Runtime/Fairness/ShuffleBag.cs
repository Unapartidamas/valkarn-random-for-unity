// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Shuffle Bag — draw without replacement, reshuffle when empty.
// Reference: Shay Pierce, "Dark Secrets of the RNG", GDC 2017.
// Also: Tetris 7-bag system.
//
// Guarantees exact distribution over each cycle.
// Max streak bounded by bag size (cannot get same item twice in a row
// across bag boundaries without it being in both bags).

using System;

namespace UnaPartidaMas.Valkarn.Random.Fairness
{
    public sealed class ShuffleBag<T>
    {
        readonly T[] template;
        T[] bag;
        int cursor;

        /// <summary>
        /// Creates a shuffle bag from the given items.
        /// Each call to Draw() returns one item. When all items are drawn,
        /// the bag is reshuffled automatically.
        /// </summary>
        public ShuffleBag(T[] items)
        {
            if (items == null || items.Length == 0)
                throw new ArgumentException("Items must not be null or empty.", nameof(items));

            template = (T[])items.Clone();
            bag = (T[])items.Clone();
            cursor = items.Length;
        }

        /// <summary>
        /// Creates a weighted shuffle bag. Each item is repeated proportionally to its count.
        /// Example: ShuffleBag.CreateWeighted(("common", 7), ("rare", 2), ("epic", 1))
        /// creates a bag of 10 items with exact 70/20/10 distribution per cycle.
        /// </summary>
        public static ShuffleBag<T> CreateWeighted(params (T item, int count)[] entries)
        {
            int total = 0;
            foreach (var (_, count) in entries)
                total += count;

            var items = new T[total];
            int idx = 0;
            foreach (var (item, count) in entries)
            {
                for (int i = 0; i < count; i++)
                    items[idx++] = item;
            }

            return new ShuffleBag<T>(items);
        }

        /// <summary>
        /// Draws the next item from the bag.
        /// Reshuffles automatically when all items have been drawn.
        /// </summary>
        public T Draw(ref ValkarnRandom rng)
        {
            if (cursor <= 0)
                Reshuffle(ref rng);

            cursor--;
            int j = rng.Range(cursor + 1);
            (bag[cursor], bag[j]) = (bag[j], bag[cursor]);
            return bag[cursor];
        }

        /// <summary>
        /// Peeks at the number of items remaining before the next reshuffle.
        /// </summary>
        public int Remaining => cursor;

        /// <summary>Total items per cycle.</summary>
        public int Size => bag.Length;

        /// <summary>Forces a reshuffle, resetting the cycle.</summary>
        public void Reshuffle(ref ValkarnRandom rng)
        {
            Array.Copy(template, bag, template.Length);
            Selection.FisherYates.Shuffle(bag, ref rng);
            cursor = bag.Length;
        }
    }
}
