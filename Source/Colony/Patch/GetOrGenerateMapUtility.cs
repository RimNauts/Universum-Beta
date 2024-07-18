using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class GetOrGenerateMapUtility {
    private const string TYPE_NAME = "Verse.GetOrGenerateMapUtility";
    
    [HarmonyPatch]
    private static class GetOrGenerateMap {
        private const string METHOD_NAME = $"{TYPE_NAME}:GetOrGenerateMap";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME, [typeof(int), typeof(Verse.IntVec3), typeof(RimWorld.WorldObjectDef)]);

        public static void Postfix(int tile, Verse.IntVec3 size, RimWorld.WorldObjectDef suggestedMapParentDef, ref Verse.Map __result) {
            if (__result is null) return;

            World.ObjectHolder objectHolder = Cache.ObjectHolder.Get(tile);
            if (objectHolder is null || objectHolder.Faction is not null) return;

            objectHolder.SetFaction(RimWorld.Faction.OfPlayer);
        }
    }
}
