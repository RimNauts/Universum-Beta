using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class PawnApparelTracker {
    private const string TYPE_NAME = "RimWorld.Pawn_ApparelTracker";
    
    [HarmonyPatch]
    private static class NotifyApparelChanged {
        private const string METHOD_NAME = $"{TYPE_NAME}:Notify_ApparelChanged";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(RimWorld.Pawn_ApparelTracker __instance) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(__instance.pawn.thingIDNumber);
        }
    }
}
