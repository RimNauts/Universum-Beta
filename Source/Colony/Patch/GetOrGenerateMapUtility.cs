using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class GetOrGenerateMapUtility {
    [HarmonyPatch]
    static class GetOrGenerateMap {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() {
            var type = AccessTools.TypeByName("Verse.GetOrGenerateMapUtility");
            if (type == null) return null;

            var method = AccessTools.Method(
                type,
                "GetOrGenerateMap",
                [typeof(int), typeof(Verse.IntVec3), typeof(RimWorld.WorldObjectDef)]
            );
            
            return method == null ? null : method;
        }

        public static void Postfix(int tile, Verse.IntVec3 size, RimWorld.WorldObjectDef suggestedMapParentDef, ref Verse.Map __result) {
            if (__result == null) return;

            World.ObjectHolder objectHolder = Cache.ObjectHolder.Get(tile);
            if (objectHolder == null || objectHolder.Faction != null) return;

            objectHolder.SetFaction(RimWorld.Faction.OfPlayer);
        }
    }
}
