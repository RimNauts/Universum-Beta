using System.Collections.Generic;

namespace Universum.Cache;

public static class Settings {
    private static readonly Dictionary<int, bool> UTILITY_TOGGLES = new();
    private static readonly Dictionary<string, int> CELESTIAL_BODIES_COUNT = new();

    public static int TotalConfigurations => UTILITY_TOGGLES.Count + CELESTIAL_BODIES_COUNT.Count;

    public static void SetUtility(int id, bool value) {
        UTILITY_TOGGLES[id] = value;
        
        Utilities.Manager.UpdateSettings(id, value);
    }
    
    public static void SetCelestialBodiesCount(string defName, int value) => CELESTIAL_BODIES_COUNT[defName] = value;

    public static bool UtilityEnabled(int id) {
        return UTILITY_TOGGLES.TryGetValue(id, out bool value) && value;
    }
    
    public static int CelestialBodiesCount(string defName) => CELESTIAL_BODIES_COUNT.GetValueOrDefault(defName, 0);

    public static Dictionary<string, bool> ConvertUtilitiesToExposable() {
        Dictionary<string, bool> exposableUtilityToggles = new();

        foreach (Defs.Utility utilityDef in Loader.Defs.Utilities.Values) {
            int id = Loader.Defs.UtilityId[utilityDef.id];

            exposableUtilityToggles[utilityDef.id] = UtilityEnabled(id);
        }

        return exposableUtilityToggles;
    }

    public static Dictionary<string, int> ConvertCelestialBodiesCountToExposable() {
        return CELESTIAL_BODIES_COUNT;
    }

    public static void ConvertUtilitiesFromExposable(Dictionary<string, bool> exposableUtilityToggles) {
        exposableUtilityToggles ??= new Dictionary<string, bool>();

        foreach (Defs.Utility utilityDef in Loader.Defs.Utilities.Values) {
            int id = Loader.Defs.UtilityId[utilityDef.id];

            bool newValue = exposableUtilityToggles.TryGetValue(utilityDef.id, out bool value)
                ? value
                : utilityDef.defaultToggle;
            
            SetUtility(id, newValue);
        }
    }

    public static void ConvertCelestialBodiesCountFromExposable(Dictionary<string, int> exposableCelestialBodiesCount) {
        exposableCelestialBodiesCount ??= new Dictionary<string, int>();

        foreach (Defs.ObjectGeneration objectGenerationDef in Loader.Defs.CelestialObjectGenerationSteps.Values) {
            CELESTIAL_BODIES_COUNT[objectGenerationDef.defName] =
                exposableCelestialBodiesCount.TryGetValue(objectGenerationDef.defName, out int value)
                ? value
                : objectGenerationDef.total;
        }
    }
}
