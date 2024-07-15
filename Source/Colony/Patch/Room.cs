using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class Room {
    public static class NotifyTerrainChanged {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.Room:Notify_TerrainChanged");


        public static void Postfix(ref Verse.Room __instance) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            __instance.Notify_RoofChanged();
        }
    }
}
