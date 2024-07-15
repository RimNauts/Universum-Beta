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

public static class BiomeDef {
    private const string TYPE_NAME = "RimWorld.BiomeDef";
    
    public static class DrawMaterial {
        private const string METHOD_NAME = $"{TYPE_NAME}:DrawMaterial";

        public static bool Prepare() {
            if (TargetMethod() != null) return true;
            
            Debugger.Log(
                key: "Universum.Error.FailedToPatch",
                prefix: $"{Mod.Manager.METADATA.NAME}: ",
                args: [METHOD_NAME],
                severity: Debugger.Severity.Error
            );
            return false;
        }

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix(ref RimWorld.BiomeDef __instance) {
            if (Loader.Defs.BiomeProperties[__instance.index].activeUtilities[Cache.Utilities.OceanMasking.id]) {
                __instance = RimWorld.BiomeDefOf.Ocean;
            }
        }
    }
}
