using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public class WorldObjectsHolder {
    [HarmonyPatch]
    static class AddToCache {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldObjectsHolder:AddToCache");

        public static void Prefix(RimWorld.Planet.WorldObject o) {
            if (o is ObjectHolder objectHolder) Cache.ObjectHolder.Add(objectHolder);
        }
    }

    [HarmonyPatch]
    static class RemoveFromCache {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldObjectsHolder:RemoveFromCache");

        public static void Prefix(RimWorld.Planet.WorldObject o) {
            if (o is ObjectHolder objectHolder) Cache.ObjectHolder.Remove(objectHolder);
        }
    }

    [HarmonyPatch]
    static class Recache {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldObjectsHolder:Recache");

        public static void Prefix() {
            Cache.ObjectHolder.Clear();
        }
    }
}
