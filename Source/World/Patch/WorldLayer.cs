using System.Linq;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class WorldLayer {
    private const string TYPE_NAME = "RimWorld.Planet.WorldLayer_CurrentMapTile";
    
    [HarmonyPatch]
    public static class Tile {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_Tile";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix(ref int __result) {
            if (__result == -1) return;
            
            if (Loader.Defs.BiomeProperties[Verse.Find.World.grid.tiles.ElementAt(__result).biome.index]
                .activeUtilities[Cache.Utilities.Manager.OCEAN_MASKING.id]) __result = -1;
        }
    }
}
