namespace Universum.Utilities {
    public static class CachingHandler {
        public static SubscriberManager vacuum;

        public static bool[] mapIsOuterSpace = new bool[128];

        public static void Init() {
            vacuum = new SubscriberManager();

            ResetSettingsEnabledCache();
        }

        public static void ResetCache() {
            // reset subscriber managers
            vacuum.Reset();

            // reset cache
            mapIsOuterSpace = new bool[128];
        }

        public static void ResetSettingsEnabledCache() {
            vacuum.SetSettingsEnabled(enabled: Settings.utility_turned_on(id: "universum.vacuum"));
        }
    }
}
