// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.
//
// ValkarnNoise — static facade for position-based noise (stateless random).
// Wraps SquirrelNoise5 for convenient access.
//
// Use when you need random values keyed by position, entity ID, or any index.
// No state, no sequence dependency, lock-free, trivially parallel.

using System.Runtime.CompilerServices;

namespace UnaPartidaMas.Valkarn.Random
{
    public static class ValkarnNoise
    {
        /// <summary>1D noise: position -> uint.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(int position, uint seed) =>
            SquirrelNoise5.Get1D(position, seed);

        /// <summary>2D noise: (x, y) -> uint.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(int x, int y, uint seed) =>
            SquirrelNoise5.Get2D(x, y, seed);

        /// <summary>3D noise: (x, y, z) -> uint.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Hash(int x, int y, int z, uint seed) =>
            SquirrelNoise5.Get3D(x, y, z, seed);

        /// <summary>1D noise: position -> float [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Float(int position, uint seed) =>
            SquirrelNoise5.Get1DFloat(position, seed);

        /// <summary>2D noise: (x, y) -> float [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Float(int x, int y, uint seed) =>
            SquirrelNoise5.Get2DFloat(x, y, seed);

        /// <summary>3D noise: (x, y, z) -> float [0, 1).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Float(int x, int y, int z, uint seed) =>
            SquirrelNoise5.Get3DFloat(x, y, z, seed);

        /// <summary>
        /// Derives a seed by mixing a base seed with an ID.
        /// Useful for creating per-entity or per-subsystem seeds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DeriveSeed(ulong baseSeed, ulong id) =>
            SplitMix64.Derive(baseSeed, id);
    }
}
