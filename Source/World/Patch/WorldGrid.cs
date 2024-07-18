using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class WorldGrid {
    private const string TYPE_NAME = "RimWorld.Planet.WorldGrid";
    
    [HarmonyPatch]
    private static class TraversalDistanceBetween {
        private const string METHOD_NAME = $"{TYPE_NAME}:TraversalDistanceBetween";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix(int start, int end, bool passImpassable, int maxDist, ref int __result) {
            bool fromOrbit = Cache.ObjectHolder.Exists(start);
            if (fromOrbit) {
                __result = 20;
                return;
            }

            bool toOrbit = Cache.ObjectHolder.Exists(end);
            if (toOrbit) __result = 100;
        }
    }
}
