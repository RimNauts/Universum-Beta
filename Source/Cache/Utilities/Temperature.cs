using HarmonyLib;

namespace Universum.Cache.Utilities;

public static class Temperature {
    public static int id;
    
    public static bool[] maps = new bool[128];

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.temperature", out id);
        
        tracker = new SubscriptionTracker(harmony);
        tracker.AddPatches([
            typeof(Colony.Patch.MapTemperature.OutdoorTemp),
            typeof(Colony.Patch.MapTemperature.SeasonalTemp),
            typeof(Colony.Patch.RoomTempTracker.WallEqualizationTempChangePerInterval),
            typeof(Colony.Patch.RoomTempTracker.ThinRoofEqualizationTempChangePerInterval),
            typeof(Colony.Patch.RoomTempTracker.EqualizeTemperature),
            typeof(Colony.Patch.District.OpenRoofCountStopAt)
        ]);
        tracker.Init();
    }

    public static void Reset() {
        maps = new bool[128];
        tracker.Reset();
    }
}
