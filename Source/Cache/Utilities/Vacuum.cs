using HarmonyLib;

namespace Universum.Cache.Utilities;

public static class Vacuum {
    public static int id;
    
    public static bool[] maps = new bool[128];

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.vacuum", out id);
        
        tracker = new SubscriptionTracker(harmony);
        tracker.AddPatches([
            typeof(Colony.Patch.ExitMapGrid.Color),
            typeof(Colony.Patch.PollutionGrid.SetPolluted),
            typeof(Colony.Patch.Room.NotifyTerrainChanged),
            typeof(Colony.Patch.GlobalControls.TemperatureString)
        ]);
        tracker.Init();
    }

    public static void Reset() {
        maps = new bool[128];
        tracker.Reset();
    }

    public static void UpdateMapValue(int mapIndex, int biomeIndex) {
        bool utilityActiveInMap = Loader.Defs.BiomeProperties[biomeIndex].activeUtilities[id];
        
        maps[mapIndex] = utilityActiveInMap;
        if (utilityActiveInMap) tracker.Subscribe();
    }
}
