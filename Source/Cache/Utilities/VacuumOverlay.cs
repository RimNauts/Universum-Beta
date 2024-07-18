using HarmonyLib;

namespace Universum.Cache.Utilities;

public static class VacuumOverlay {
    public static int id;
    
    public static bool[] maps = new bool[128];

    public static SubscriptionTracker tracker;

    public static void Init(Harmony harmony) {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.vacuum_overlay", out id);
        
        tracker = new SubscriptionTracker(harmony, alwaysActive: true);
        tracker.AddPatches([
            typeof(Colony.Patch.MapDrawer.DrawMapMesh),
            typeof(Colony.Patch.SectionLayer.FinalizeMesh),
            typeof(Colony.Patch.SectionLayerTerrain.Regenerate),
            typeof(Game.Patch.Game.UpdatePlay),
            typeof(Colony.Patch.Section.Constructor)
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
