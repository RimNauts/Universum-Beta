using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class PawnApparelTracker {
    [HarmonyPatch]
    static class NotifyApparelChanged {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Pawn_ApparelTracker:Notify_ApparelChanged");
        
        public static void Postfix(RimWorld.Pawn_ApparelTracker __instance) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(__instance.pawn.thingIDNumber);
        }
    }
}
