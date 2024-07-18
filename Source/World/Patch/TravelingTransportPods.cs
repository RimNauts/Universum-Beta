using System.Reflection;
using HarmonyLib;
using UnityEngine;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class TravelingTransportPods {
    private const string TYPE_NAME = "RimWorld.Planet.TravelingTransportPods";
    
    [HarmonyPatch]
    private static class Start {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_Start";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.TravelingTransportPods __instance, ref Vector3 __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(__instance.initialTile);
            if (objectHolder is null) return true;

            __result = objectHolder.DrawPos;

            return false;
        }
    }
    
    [HarmonyPatch]
    private static class End {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_End";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.TravelingTransportPods __instance, ref Vector3 __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(__instance.destinationTile);
            if (objectHolder is null) return true;

            __result = objectHolder.DrawPos;

            return false;
        }
    }
}
