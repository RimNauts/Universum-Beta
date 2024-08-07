﻿using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

namespace Universum.Colony.Patch;

public static class SectionLayerTerrain {
    private const string TYPE_NAME = "Verse.SectionLayer_Terrain";
    
    [HarmonyPatch]
    public static class Regenerate {
        private const string METHOD_NAME = $"{TYPE_NAME}:Regenerate";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix(Verse.SectionLayer __instance) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Manager.VACUUM_OVERLAY.maps[mapIndex]) return;
            
            Game.Patch.Game.UpdatePlay.MeshRecalculateHelper.RecalculateLayer(__instance);
        }
    }
}
