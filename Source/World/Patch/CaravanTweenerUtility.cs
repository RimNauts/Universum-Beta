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

public static class CaravanTweenerUtility {
    private const string TYPE_NAME = "RimWorld.Planet.CaravanTweenerUtility";
    
    [HarmonyPatch]
    static class PatherTweenedPosRoot {
        private const string METHOD_NAME = $"{TYPE_NAME}:PatherTweenedPosRoot";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.Caravan caravan, ref Vector3 __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(caravan.Tile);
            if (objectHolder == null) return true;

            __result = objectHolder.DrawPos;

            return false;
        }
    }
}
