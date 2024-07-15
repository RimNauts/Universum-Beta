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

public static class WeatherDecider {
    public static class CurrentWeatherCommonality {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.WeatherDecider:CurrentWeatherCommonality");


        public static bool Prefix(Verse.WeatherDef weather, RimWorld.WeatherDecider __instance, ref float __result) {
            int mapIndex = Verse.Find.Maps.IndexOf(__instance.map);
            if (!Cache.Utilities.WeatherChanger.maps[mapIndex]) return true;
            
            if (__instance.map.weatherManager.curWeather is null || weather == __instance.map.weatherManager.curWeather) {
                __result = 1.0f;
                return false;
            }
            
            // use defName to support SOS2 space weather
            if (weather.defName == "OuterSpaceWeather") {
                __result = 1.0f;
                return false;
            }
            
            __result = 0.0f;
            return false;
        }
    }
}
