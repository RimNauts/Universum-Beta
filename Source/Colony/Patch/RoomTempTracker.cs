using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class RoomTempTracker {
    private const string TYPE_NAME = "Verse.RoomTempTracker";
    
    [HarmonyPatch]
    public static class WallEqualizationTempChangePerInterval {
        private const string METHOD_NAME = $"{TYPE_NAME}:WallEqualizationTempChangePerInterval";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Postfix(Verse.RoomTempTracker __instance, ref float __result) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex] || !Cache.Utilities.Manager.TEMPERATURE.maps[mapIndex]) return;

            __result *= 0.01f;
        }
    }
    
    [HarmonyPatch]
    public static class ThinRoofEqualizationTempChangePerInterval {
        private const string METHOD_NAME = $"{TYPE_NAME}:ThinRoofEqualizationTempChangePerInterval";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Postfix(Verse.RoomTempTracker __instance, ref float __result) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex] || !Cache.Utilities.Manager.TEMPERATURE.maps[mapIndex]) return;

            __result *= 0.01f;
        }
    }
    
    [HarmonyPatch]
    public static class EqualizeTemperature {
        private const string METHOD_NAME = $"{TYPE_NAME}:EqualizeTemperature";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static bool Prefix(ref Verse.RoomTempTracker __instance) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex] || !Cache.Utilities.Manager.TEMPERATURE.maps[mapIndex]) return true;
            
            if (__instance.room.OpenRoofCount <= 0) return true;

            __instance.Temperature = Loader.Defs.BiomeProperties[__instance.Map.Biome.index].temperature;
            return false;
        }
    }
}
