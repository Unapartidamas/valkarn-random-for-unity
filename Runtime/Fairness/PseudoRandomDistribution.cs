// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// Pseudo-Random Distribution (PRD) — Warcraft 3 / Dota 2 style anti-streak system.
// Reference: https://liquipedia.net/dota2/Pseudo_Random_Distribution
//
// P(N) = C * N, where N = attempts since last proc, C = constant derived from nominal P.
// Same expected value as true random, but much lower variance.
// Reduces lucky/unlucky streaks significantly.
// Guaranteed proc within ceil(1/C) attempts.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnaPartidaMas.Valkarn.Random.Fairness
{
    [StructLayout(LayoutKind.Auto)]
    public struct PseudoRandomDistribution
    {
        float cValue;
        int counter;

        /// <summary>
        /// Pre-computed C values for common nominal probabilities.
        /// C is the per-attempt increment such that E[proc] matches the nominal rate.
        /// Computed via iterative solver (no closed form exists).
        /// </summary>
        static readonly (float nominal, float c)[] CTable =
        {
            (0.05f, 0.00380f),
            (0.10f, 0.01475f),
            (0.15f, 0.03222f),
            (0.20f, 0.05570f),
            (0.25f, 0.08474f),
            (0.30f, 0.11895f),
            (0.35f, 0.15764f),
            (0.40f, 0.20155f),
            (0.45f, 0.25000f),
            (0.50f, 0.30210f),
            (0.55f, 0.36040f),
            (0.60f, 0.42265f),
            (0.65f, 0.49620f),
            (0.70f, 0.57143f),
            (0.75f, 0.66667f),
            (0.80f, 0.75000f),
            (0.85f, 0.82353f),
            (0.90f, 0.90000f),
            (0.95f, 0.95000f),
        };

        /// <summary>
        /// Creates a PRD for the given nominal probability.
        /// The C value is looked up or interpolated from the table.
        /// </summary>
        public PseudoRandomDistribution(float nominalProbability)
        {
            cValue = ComputeC(nominalProbability);
            counter = 0;
        }

        /// <summary>
        /// Creates a PRD with an explicit C value.
        /// Use when you have pre-computed C values.
        /// </summary>
        public static PseudoRandomDistribution FromC(float c)
        {
            PseudoRandomDistribution prd;
            prd.cValue = c;
            prd.counter = 0;
            return prd;
        }

        /// <summary>
        /// Rolls the PRD. Returns true if the effect procs.
        /// On proc, resets the counter. On fail, increments it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Roll(ref ValkarnRandom rng)
        {
            counter++;
            float threshold = cValue * counter;

            if (rng.NextFloat() < threshold)
            {
                counter = 0;
                return true;
            }

            return false;
        }

        /// <summary>Resets the attempt counter (e.g., on round start).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => counter = 0;

        /// <summary>Current attempt count since last proc.</summary>
        public int Counter => counter;

        /// <summary>The C value used for probability scaling.</summary>
        public float CValue => cValue;

        /// <summary>Maximum attempts before guaranteed proc: ceil(1/C). Returns int.MaxValue if C is 0.</summary>
        public int MaxAttempts => cValue > 0f ? (int)MathF.Ceiling(1f / cValue) : int.MaxValue;

        static float ComputeC(float p)
        {
            if (p <= 0f) return 0f;
            if (p >= 1f) return 1f;

            // Find bracketing entries in the table
            for (int i = 0; i < CTable.Length - 1; i++)
            {
                if (p >= CTable[i].nominal && p <= CTable[i + 1].nominal)
                {
                    float t = (p - CTable[i].nominal) / (CTable[i + 1].nominal - CTable[i].nominal);
                    return CTable[i].c + t * (CTable[i + 1].c - CTable[i].c);
                }
            }

            // Below table minimum — use iterative approximation
            if (p < CTable[0].nominal)
                return ComputeCIterative(p);

            // Above table maximum
            return p;
        }

        static float ComputeCIterative(float p)
        {
            float lo = 0f, hi = p;

            for (int i = 0; i < 50; i++)
            {
                float mid = (lo + hi) * 0.5f;
                float expected = ExpectedValue(mid);

                if (expected > p)
                    hi = mid;
                else
                    lo = mid;
            }

            return (lo + hi) * 0.5f;
        }

        static float ExpectedValue(float c)
        {
            float sum = 0f;
            float prevCumulativeFailure = 1f;
            int maxN = (int)MathF.Ceiling(1f / c);

            for (int n = 1; n <= maxN; n++)
            {
                float pN = MathF.Min(1f, c * n);
                float contribution = pN * prevCumulativeFailure;
                sum += contribution;
                prevCumulativeFailure *= (1f - pN);
            }

            return sum;
        }
    }
}
