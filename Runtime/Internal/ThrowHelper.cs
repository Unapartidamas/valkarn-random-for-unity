// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System;

namespace UnaPartidaMas.Valkarn.Random.Internal
{
    internal static class ThrowHelper
    {
        internal static void ThrowIfNegativeOrZero(float value, string paramName)
        {
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        }

        internal static void ThrowIfMinGreaterThanMax(int min, int max)
        {
            if (min > max)
                throw new ArgumentException($"min ({min}) must be <= max ({max}).");
        }

        internal static void ThrowIfMinGreaterThanMax(float min, float max)
        {
            if (min > max)
                throw new ArgumentException($"min ({min}) must be <= max ({max}).");
        }

        internal static void ThrowIfEmpty<T>(T[] array, string paramName)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Array must not be null or empty.", paramName);
        }

        internal static void ThrowIfNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }
    }
}
