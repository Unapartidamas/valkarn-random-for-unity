// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Internal
{
    internal static class FloatConversion
    {
        const float Inv24 = 1f / 16777216f;        // 1 / 2^24 — for unsigned 24-bit [0,1)
        const float Inv23 = 1f / 8388608f;         // 1 / 2^23 — for signed 24-bit [-1,1)
        const double Inv53 = 1.0 / 9007199254740992.0; // 1 / 2^53

        /// <summary>
        /// Converts a uint to a float in [0, 1). Uses 24-bit mantissa resolution.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ToFloat01(uint value)
        {
            return (value >> 8) * Inv24;
        }

        /// <summary>
        /// Converts a uint to a float in (0, 1]. Never returns 0.0, can return 1.0.
        /// Safe for log-based transforms (Box-Muller, exponential).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ToFloatNonZero(uint value)
        {
            return ((value >> 8) + 1) * Inv24;
        }

        /// <summary>
        /// Converts a uint to a float in [-1, 1). Uses 24-bit signed resolution.
        /// Reinterprets bit 31 as sign, then arithmetic right-shifts.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ToFloatSigned(uint value)
        {
            // (int)value reinterprets bit 31 as sign.
            // >> 8 is arithmetic right shift (sign-preserving), yielding [-2^23, 2^23-1].
            // * Inv23 (1/2^23) normalizes to [-1.0, ~1.0).
            return ((int)value >> 8) * Inv23;
        }

        /// <summary>
        /// Converts a ulong to a double in [0, 1). Uses 53-bit mantissa resolution.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double ToDouble01(ulong value)
        {
            return (value >> 11) * Inv53;
        }
    }
}
