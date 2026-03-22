// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random.Internal
{
    internal static class BitOps
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong RotateLeft(ulong value, int count)
        {
            return (value << count) | (value >> (64 - count));
        }
    }
}
