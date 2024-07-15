using System.Linq;
using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public static class WorldLayer {
    public static class Tile {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldLayer_CurrentMapTile:get_Tile");

        public static void Postfix(ref int __result) {
            if (__result == -1) return;
            
            if (Loader.Defs.BiomeProperties[Verse.Find.World.grid.tiles.ElementAt(__result).biome.index]
                .activeUtilities[Cache.Utilities.OceanMasking.id]) __result = -1;
        }
    }
}
