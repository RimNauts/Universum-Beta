using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class GeneUtility {
    private const string TYPE_NAME = "RimWorld.GeneUtility";
    
    [HarmonyPatch]
    private static class ReimplantXenogerm {
        private const string METHOD_NAME = $"{TYPE_NAME}:ReimplantXenogerm";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(Verse.Pawn caster, Verse.Pawn recipient) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(caster.thingIDNumber);
        }
    }
    
    [HarmonyPatch]
    private static class ExtractXenogerm {
        private const string METHOD_NAME = $"{TYPE_NAME}:ExtractXenogerm";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(Verse.Pawn pawn, int overrideDurationTicks) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(pawn.thingIDNumber);
        }
    }
    
    [HarmonyPatch]
    private static class ImplantXenogermItem {
        private const string METHOD_NAME = $"{TYPE_NAME}:ImplantXenogermItem";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(Verse.Pawn pawn, RimWorld.Xenogerm xenogerm) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(pawn.thingIDNumber);
        }
    }
    
    [HarmonyPatch]
    private static class UpdateXenogermReplication {
        private const string METHOD_NAME = $"{TYPE_NAME}:UpdateXenogermReplication";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(Verse.Pawn pawn) {
            Cache.Utilities.VacuumDamage.PAWN_PROTECTION.Remove(pawn.thingIDNumber);
        }
    }
}
