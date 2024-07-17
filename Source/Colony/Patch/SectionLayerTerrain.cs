using System.Reflection;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class SectionLayerTerrain {
    private const string TYPE_NAME = "Verse.SectionLayer_Terrain";
    
    public static class Regenerate {
        private const string METHOD_NAME = $"{TYPE_NAME}:Regenerate";
        private static bool _verboseError = true;

        public static bool Prepare() {
            if (TargetMethod() != null) return true;

            if (!_verboseError) return false;
            
            Debugger.Log(
                key: "Universum.Error.FailedToPatch",
                prefix: $"{Mod.Manager.METADATA.NAME}: ",
                args: [METHOD_NAME],
                severity: Debugger.Severity.Error
            );
            _verboseError = false;

            return false;
        }

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Postfix(Verse.SectionLayer __instance) {
            Verse.Map map = __instance.Map;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            Game.Patch.Game.UpdatePlay.MeshRecalculateHelper.RecalculateLayer(__instance);
        }
    }
}
