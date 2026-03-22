// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;
using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Fairness;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class PrdTests
    {
        [Test]
        public void Roll_ExpectedValueMatchesNominal_25Percent()
        {
            var prd = new PseudoRandomDistribution(0.25f);
            var rng = ValkarnRandom.Create(42UL);

            const int samples = 200000;
            int procs = 0;

            for (int i = 0; i < samples; i++)
            {
                if (prd.Roll(ref rng))
                    procs++;
            }

            float actual = procs / (float)samples;
            Assert.AreEqual(0.25f, actual, 0.02f,
                $"Expected ~25% proc rate, got {actual * 100f:F1}%");
        }

        [Test]
        public void Roll_ExpectedValueMatchesNominal_10Percent()
        {
            var prd = new PseudoRandomDistribution(0.10f);
            var rng = ValkarnRandom.Create(99UL);

            const int samples = 200000;
            int procs = 0;

            for (int i = 0; i < samples; i++)
            {
                if (prd.Roll(ref rng))
                    procs++;
            }

            float actual = procs / (float)samples;
            Assert.AreEqual(0.10f, actual, 0.02f,
                $"Expected ~10% proc rate, got {actual * 100f:F1}%");
        }

        [Test]
        public void Roll_MaxStreakBounded()
        {
            var prd = new PseudoRandomDistribution(0.25f);
            var rng = ValkarnRandom.Create(123UL);

            int maxStreak = 0;
            int currentStreak = 0;

            for (int i = 0; i < 100000; i++)
            {
                if (prd.Roll(ref rng))
                {
                    currentStreak = 0;
                }
                else
                {
                    currentStreak++;
                    if (currentStreak > maxStreak)
                        maxStreak = currentStreak;
                }
            }

            // PRD guarantees proc within ceil(1/C) attempts
            Assert.LessOrEqual(maxStreak, prd.MaxAttempts,
                $"Max streak {maxStreak} exceeds theoretical max {prd.MaxAttempts}");
        }

        [Test]
        public void Roll_LowerVarianceThanTrueRandom()
        {
            // Compare PRD streak variance to true random streak variance
            var prdDist = new PseudoRandomDistribution(0.25f);
            var rngPrd = ValkarnRandom.Create(42UL);
            var rngTrue = ValkarnRandom.Create(42UL);

            int prdMaxStreak = 0, trueMaxStreak = 0;
            int prdStreak = 0, trueStreak = 0;
            const int samples = 100000;

            for (int i = 0; i < samples; i++)
            {
                if (prdDist.Roll(ref rngPrd))
                    prdStreak = 0;
                else if (++prdStreak > prdMaxStreak)
                    prdMaxStreak = prdStreak;

                if (rngTrue.NextBool(0.25f))
                    trueStreak = 0;
                else if (++trueStreak > trueMaxStreak)
                    trueMaxStreak = trueStreak;
            }

            Assert.Less(prdMaxStreak, trueMaxStreak,
                $"PRD max streak ({prdMaxStreak}) should be less than true random ({trueMaxStreak})");
        }

        [Test]
        public void Reset_ClearsCounter()
        {
            var prd = new PseudoRandomDistribution(0.25f);
            var rng = ValkarnRandom.Create(456UL);

            prd.Roll(ref rng);
            prd.Roll(ref rng);

            prd.Reset();
            Assert.AreEqual(0, prd.Counter);
        }

        [Test]
        public void MaxAttempts_CorrectForKnownValues()
        {
            var prd25 = new PseudoRandomDistribution(0.25f);
            Assert.AreEqual(12, prd25.MaxAttempts, "25% PRD should guarantee within 12 attempts");

            var prd50 = new PseudoRandomDistribution(0.50f);
            Assert.AreEqual(4, prd50.MaxAttempts, "50% PRD should guarantee within 4 attempts");
        }
    }
}
