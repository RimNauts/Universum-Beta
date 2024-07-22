using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

namespace Universum.Colony.Patch;

public static class Pawn {
    private const string TYPE_NAME = "Verse.Pawn";
    
    [HarmonyPatch]
    private static class Kill {
        private const string METHOD_NAME = $"{TYPE_NAME}:Kill";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(Verse.Pawn __instance) {
            Cache.VacuumDamage.PAWN_PROTECTION.Remove(__instance.thingIDNumber);
        }
    }
}
