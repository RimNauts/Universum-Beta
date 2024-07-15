using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public class WorldObjectSelectionUtility {
    [HarmonyPatch]
    static class HiddenBehindTerrainNow {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldObjectSelectionUtility:HiddenBehindTerrainNow");

        public static bool Prefix(RimWorld.Planet.WorldObject o, ref bool __result) {
            if (o is not ObjectHolder objectHolder) return true;
            
            __result = objectHolder.hideIcon;
            return false;

        }
    }
}
