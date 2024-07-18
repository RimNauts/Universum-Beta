using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class WeatherDecider {
    private const string TYPE_NAME = "RimWorld.WeatherDecider";
    private const string WEATHER_DEF_NAME = "OuterSpaceWeather";

    [HarmonyPatch]
    public static class CurrentWeatherCommonality {
        private const string METHOD_NAME = $"{TYPE_NAME}:CurrentWeatherCommonality";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static bool Prefix(Verse.WeatherDef weather, RimWorld.WeatherDecider __instance, ref float __result) {
            int mapIndex = Verse.Find.Maps.IndexOf(__instance.map);
            if (!Cache.Utilities.Manager.WEATHER_CHANGER.maps[mapIndex]) return true;
            
            if (__instance.map.weatherManager.curWeather is null || weather == __instance.map.weatherManager.curWeather) {
                __result = 1.0f;
                return false;
            }
            
            // use defName to support SOS2 space weather
            if (weather.defName == WEATHER_DEF_NAME) {
                __result = 1.0f;
                return false;
            }
            
            __result = 0.0f;
            return false;
        }
    }
}
