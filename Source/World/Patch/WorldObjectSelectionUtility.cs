﻿using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class WorldObjectSelectionUtility {
    private const string TYPE_NAME = "RimWorld.Planet.WorldObjectSelectionUtility";
    
    [HarmonyPatch]
    private static class HiddenBehindTerrainNow {
        private const string METHOD_NAME = $"{TYPE_NAME}:HiddenBehindTerrainNow";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.WorldObject o, ref bool __result) {
            if (o is not ObjectHolder objectHolder) return true;
            
            __result = objectHolder.hideIcon;
            return false;

        }
    }
}
