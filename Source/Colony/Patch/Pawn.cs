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

public static class Pawn {
    [HarmonyPatch]
    static class Kill {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.Pawn:Kill");
        
        public static void Postfix(Verse.Pawn __instance) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(__instance.thingIDNumber);
        }
    }
}
