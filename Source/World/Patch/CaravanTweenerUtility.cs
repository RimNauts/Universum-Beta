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

public static class CaravanTweenerUtility {
    private const string TYPE_NAME = "RimWorld.Planet.CaravanTweenerUtility";
    
    [HarmonyPatch]
    static class PatherTweenedPosRoot {
        private const string METHOD_NAME = $"{TYPE_NAME}:PatherTweenedPosRoot";

        public static bool Prepare() {
            if (TargetMethod() != null) return true;
            
            Debugger.Log(
                key: "Universum.Error.FailedToPatch",
                prefix: $"{Mod.Manager.METADATA.NAME}: ",
                args: [METHOD_NAME],
                severity: Debugger.Severity.Error
            );
            return false;
        }

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.Caravan caravan, ref Vector3 __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(caravan.Tile);
            if (objectHolder == null) return true;

            __result = objectHolder.DrawPos;

            return false;
        }
    }
}
