using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

namespace Universum.Colony.Patch;

public static class MapTemperature {
    private const string TYPE_NAME = "Verse.MapTemperature";
    
    [HarmonyPatch]
    public static class OutdoorTemp {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_OutdoorTemp";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static bool Prefix(ref float __result, Verse.Map ___map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: ___map);
            
            if (!Cache.Utilities.Manager.TEMPERATURE.maps[mapIndex]) return true;

            __result = Loader.Defs.BiomeProperties[___map.Biome.index].temperature;
            return false;
        }
    }
    
    [HarmonyPatch]
    public static class SeasonalTemp {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_SeasonalTemp";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static bool Prefix(ref float __result, Verse.Map ___map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: ___map);
            
            if (!Cache.Utilities.Manager.TEMPERATURE.maps[mapIndex]) return true;

            __result = Loader.Defs.BiomeProperties[___map.Biome.index].temperature;
            return false;
        }
    }
}
