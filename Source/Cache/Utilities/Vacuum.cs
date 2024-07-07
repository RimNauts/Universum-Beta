using HarmonyLib;

namespace Universum.Cache.Utilities;

public static class Vacuum {
    public static int index;
    
    public static bool[] mapIsOuterSpace = new bool[128];

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.vacuum", out index);
        
        tracker = new SubscriptionTracker(harmony);
    }

    public static void Reset() {
        mapIsOuterSpace = new bool[128];
        tracker.Reset();
    }
}
