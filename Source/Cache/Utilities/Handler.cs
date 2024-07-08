namespace Universum.Cache.Utilities;

public static class Handler {
    public static bool[] UtilitiesEnabled { get; set; }

    public static void Init() {
        UtilitiesEnabled = new bool[Loader.Defs.UtilityId.Count];
    }
}
