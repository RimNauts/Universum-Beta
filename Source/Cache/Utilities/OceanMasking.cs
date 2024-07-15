using HarmonyLib;

namespace Universum.Cache.Utilities;

public static class OceanMasking {
    public static int id;

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.ocean_masking", out id);
        
        tracker = new SubscriptionTracker(harmony, alwaysActive: true);
        tracker.AddPatches([
            typeof(World.Patch.BiomeDef.DrawMaterial),
            typeof(World.Patch.WorldLayer.Tile)
        ]);
        tracker.Init();
    }

    public static void Reset() {
        tracker.Reset();
    }
}
