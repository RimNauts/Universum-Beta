using HarmonyLib;

namespace Universum.Cache.Utilities;

public class WeatherChanger {
    public static int id;
    
    public static bool[] maps = new bool[128];

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.disable_weather_change", out id);
        
        tracker = new SubscriptionTracker(harmony);
        tracker.AddPatches([typeof(Colony.Patch.WeatherDecider.CurrentWeatherCommonality)]);
        tracker.Init();
    }

    public static void Reset() {
        maps = new bool[128];
        tracker.Reset();
    }
}
