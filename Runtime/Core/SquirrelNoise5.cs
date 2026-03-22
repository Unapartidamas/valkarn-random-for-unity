// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// SquirrelNoise5 — position-based noise hash (stateless PRNG).
// Reference: Squirrel Eiserloh, "Math for Game Programmers: Noise-Based RNG",
//            GDC 2017. Version 5 fixes repetition issues at high positions.
//
// Pure function: hash(position, seed) -> random value.
// No state, no sequence dependency. Lock-free, trivially parallel.
// Ideal for procedural generation, deterministic world features, network sync.

using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random
{
    public static class SquirrelNoise5
    {
        const uint BitNoise1 = 0xD2A80A3FU;
        const uint BitNoise2 = 0xA884F197U;
        const uint BitNoise3 = 0x6C736F4BU;
        const uint BitNoise4 = 0xB79F3ABBu;
        const uint BitNoise5 = 0x1B56C4E9U;

        /// <summary>1D noise: position -> random uint.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Get1D(int position, uint seed)
        {
            uint n = (uint)position;
            n *= BitNoise1;
            n += seed;
            n ^= (n >> 9);
            n += BitNoise2;
            n ^= (n >> 11);
            n *= BitNoise3;
            n ^= (n >> 13);
            n += BitNoise4;
            n ^= (n >> 15);
            n *= BitNoise5;
            n ^= (n >> 17);
            return n;
        }

        /// <summary>2D noise: (x, y) -> random uint. Folds y into position.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Get2D(int x, int y, uint seed)
        {
            return Get1D(x + (y * 198491317), seed);
        }

        /// <summary>3D noise: (x, y, z) -> random uint.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Get3D(int x, int y, int z, uint seed)
        {
            return Get1D(x + (y * 198491317) + (z * 6542989), seed);
        }

        /// <summary>4D noise: (x, y, z, w) -> random uint.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Get4D(int x, int y, int z, int w, uint seed)
        {
            return Get1D(x + (y * 198491317) + (z * 6542989) + (w * 357239), seed);
        }

        /// <summary>1D noise returning float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get1DFloat(int position, uint seed)
        {
            return Internal.FloatConversion.ToFloat01(Get1D(position, seed));
        }

        /// <summary>2D noise returning float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get2DFloat(int x, int y, uint seed)
        {
            return Internal.FloatConversion.ToFloat01(Get2D(x, y, seed));
        }

        /// <summary>3D noise returning float in [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get3DFloat(int x, int y, int z, uint seed)
        {
            return Internal.FloatConversion.ToFloat01(Get3D(x, y, z, seed));
        }
    }
}
