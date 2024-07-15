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

public static class RoomTempTracker {
    public static class WallEqualizationTempChangePerInterval {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.RoomTempTracker:WallEqualizationTempChangePerInterval");


        public static void Postfix(Verse.RoomTempTracker __instance, ref float __result) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex] || !Cache.Utilities.Temperature.maps[mapIndex]) return;

            __result *= 0.01f;
        }
    }
    
    public static class ThinRoofEqualizationTempChangePerInterval {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.RoomTempTracker:ThinRoofEqualizationTempChangePerInterval");


        public static void Postfix(Verse.RoomTempTracker __instance, ref float __result) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex] || !Cache.Utilities.Temperature.maps[mapIndex]) return;

            __result *= 0.01f;
        }
    }
    
    public static class EqualizeTemperature {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.RoomTempTracker:EqualizeTemperature");


        public static bool Prefix(ref Verse.RoomTempTracker __instance) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex] || !Cache.Utilities.Temperature.maps[mapIndex]) return true;
            
            if (__instance.room.OpenRoofCount <= 0) return true;

            __instance.Temperature = Loader.Defs.BiomeProperties[__instance.Map.Biome.index].temperature;
            return false;
        }
    }
}
