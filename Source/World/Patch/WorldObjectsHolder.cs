using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class WorldObjectsHolder {
    private const string TYPE_NAME = "RimWorld.Planet.WorldObjectsHolder";
    
    [HarmonyPatch]
    private static class AddToCache {
        private const string METHOD_NAME = $"{TYPE_NAME}:AddToCache";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix(RimWorld.Planet.WorldObject o) {
            if (o is ObjectHolder objectHolder) Cache.ObjectHolder.Add(objectHolder);
        }
    }

    [HarmonyPatch]
    private static class RemoveFromCache {
        private const string METHOD_NAME = $"{TYPE_NAME}:RemoveFromCache";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix(RimWorld.Planet.WorldObject o) {
            if (o is ObjectHolder objectHolder) Cache.ObjectHolder.Remove(objectHolder);
        }
    }

    [HarmonyPatch]
    private static class Recache {
        private const string METHOD_NAME = $"{TYPE_NAME}:Recache";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix() {
            Cache.ObjectHolder.Clear();
        }
    }
}
