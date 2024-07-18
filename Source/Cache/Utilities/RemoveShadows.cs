namespace Universum.Cache.Utilities;

public class RemoveShadows : SubscriptionTracker {
    public bool[] maps = new bool[128];

    public override void Reset() {
        maps = new bool[128];
        ResetTracker();
    }

    public void UpdateMapValue(int mapIndex, int biomeIndex) {
        bool utilityActiveInMap = Loader.Defs.BiomeProperties[biomeIndex].activeUtilities[id];
        
        maps[mapIndex] = utilityActiveInMap;
        if (utilityActiveInMap) Subscribe();
    }
}
