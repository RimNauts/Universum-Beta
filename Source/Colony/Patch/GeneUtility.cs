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

public static class GeneUtility {
    [HarmonyPatch]
    static class ReimplantXenogerm {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.GeneUtility:ReimplantXenogerm");
        
        public static void Postfix(Verse.Pawn caster, Verse.Pawn recipient) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(caster.thingIDNumber);
        }
    }
    
    [HarmonyPatch]
    static class ExtractXenogerm {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.GeneUtility:ExtractXenogerm");
        
        public static void Postfix(Verse.Pawn pawn, int overrideDurationTicks) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(pawn.thingIDNumber);
        }
    }
    
    [HarmonyPatch]
    static class ImplantXenogermItem {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.GeneUtility:ImplantXenogermItem");
        
        public static void Postfix(Verse.Pawn pawn, RimWorld.Xenogerm xenogerm) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(pawn.thingIDNumber);
        }
    }
    
    [HarmonyPatch]
    static class UpdateXenogermReplication {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.GeneUtility:UpdateXenogermReplication");
        
        public static void Postfix(Verse.Pawn pawn) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(pawn.thingIDNumber);
        }
    }
}
