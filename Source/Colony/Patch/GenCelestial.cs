using System.Linq;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class GenCelestial {
    private const string TYPE_NAME = "RimWorld.GenCelestial";
    
    [HarmonyPatch]
    public static class CelestialSunGlow {
        private const string METHOD_NAME = $"{TYPE_NAME}:CelestialSunGlow";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME, [typeof(int), typeof(int)]);

        public static bool Prefix(int tile, int ticksAbs, ref float __result) {
            if (tile == -1) return true;
            
            int biomeIndex = Verse.Find.World.grid.tiles.ElementAt(tile).biome.index;

            if (!Cache.Utilities.Manager.VACUUM.CheckBiome(biomeIndex) || !Cache.Utilities.Manager.REMOVE_SHADOWS.CheckBiome(biomeIndex)) {
                return true;
            }

            __result = 1.0f;
            return false;
        }
    }
}
