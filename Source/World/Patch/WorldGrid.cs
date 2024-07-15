using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public static class WorldGrid {
    [HarmonyPatch]
    static class TraversalDistanceBetween {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldGrid:TraversalDistanceBetween");

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
