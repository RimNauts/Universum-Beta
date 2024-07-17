using HarmonyLib;

namespace Universum.Cache.Utilities;

public static class RemoveShadows {
    public static int id;
    
    public static bool[] maps = new bool[128];

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.remove_shadows", out id);
        
        tracker = new SubscriptionTracker(harmony, alwaysActive: true);
        tracker.AddPatches([
            typeof(Colony.Patch.SkyManager.SkyManagerUpdate),
            typeof(Colony.Patch.GenCelestial.CelestialSunGlow)
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
