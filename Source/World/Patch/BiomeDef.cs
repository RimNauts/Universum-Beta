using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class BiomeDef {
    private const string TYPE_NAME = "RimWorld.BiomeDef";
    
    [HarmonyPatch]
    public static class DrawMaterial {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_DrawMaterial";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix(ref RimWorld.BiomeDef __instance) {
            if (Loader.Defs.BiomeProperties[__instance.index].activeUtilities[Cache.Utilities.Manager.OCEAN_MASKING.id]) {
                __instance = RimWorld.BiomeDefOf.Ocean;
            }
        }
    }
}
