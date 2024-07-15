using System.Reflection;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public static class TravelingTransportPods {
    [HarmonyPatch]
    static class Start {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.TravelingTransportPods:get_Start");

        public static bool Prefix(RimWorld.Planet.TravelingTransportPods __instance, ref Vector3 __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(__instance.initialTile);
            if (objectHolder is null) return true;

            __result = objectHolder.DrawPos;

            return false;
        }
    }
    
    [HarmonyPatch]
    static class End {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.TravelingTransportPods:get_End");

        public static bool Prefix(RimWorld.Planet.TravelingTransportPods __instance, ref Vector3 __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(__instance.destinationTile);
            if (objectHolder is null) return true;

            __result = objectHolder.DrawPos;

            return false;
        }
    }
}
