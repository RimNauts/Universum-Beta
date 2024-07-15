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

public static class MapTemperature {
    public static class OutdoorTemp {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.MapTemperature:get_OutdoorTemp");


        public static bool Prefix(ref float __result, Verse.Map ___map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: ___map);
            
            if (!Cache.Utilities.Temperature.maps[mapIndex]) return true;

            __result = Loader.Defs.BiomeProperties[___map.Biome.index].temperature;
            return false;
        }
    }
    
    public static class SeasonalTemp {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.MapTemperature:get_SeasonalTemp");


        public static bool Prefix(ref float __result, Verse.Map ___map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: ___map);
            
            if (!Cache.Utilities.Temperature.maps[mapIndex]) return true;

            __result = Loader.Defs.BiomeProperties[___map.Biome.index].temperature;
            return false;
        }
    }
}
