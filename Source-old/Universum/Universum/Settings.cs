using System;
using System.Collections.Generic;
using Verse;

namespace Universum {
    public class Settings : ModSettings {
        private static int total_configurations_found;
        public static Dictionary<string, ObjectsDef.Metadata> utilities = new Dictionary<string, ObjectsDef.Metadata>();
        public static Dictionary<string, bool> failed_attempts = new Dictionary<string, bool>();
        public static Dictionary<string, bool> saved_settings = new Dictionary<string, bool>();

        public static Dictionary<string, int> totalToSpawnGenStep = new Dictionary<string, int>();
        public static Dictionary<string, int> savedtotalToSpawnGenStep = new Dictionary<string, int>();

        public static void init() {
            // check with defs to update list
            foreach (ObjectsDef.Metadata metadata in DefOf.Objects.Utilities) utilities.Add(metadata.id, metadata);
            foreach (KeyValuePair<string, bool> saved_utility in saved_settings) {
                if (utilities.ContainsKey(saved_utility.Key)) utilities[saved_utility.Key].toggle = saved_utility.Value;
            }
            total_configurations_found = utilities.Count;

            foreach (var (defName, def) in Defs.Loader.celestialObjectGenerationStartUpSteps) totalToSpawnGenStep.Add(defName, def.total);
            foreach (var (defName, def) in Defs.Loader.celestialObjectGenerationRandomSteps) totalToSpawnGenStep.Add(defName, def.total);
            foreach (var (defName, savedTotal) in savedtotalToSpawnGenStep) if (totalToSpawnGenStep.ContainsKey(defName)) totalToSpawnGenStep[defName] = savedTotal;
            // print stats
            Logger.print(
                Logger.Importance.Info,
                key: "Universum.Info.settings_loader_done",
                prefix: Style.tab,
                args: new NamedArgument[] { total_configurations_found }
            );
        }

        public static bool utility_turned_on(string id) {
            if (utilities.TryGetValue(id, out ObjectsDef.Metadata utility)) {
                return utility.toggle;
            } else {
                if (failed_attempts.TryGetValue(id, out bool value)) return value;
                Logger.print(
                    Logger.Importance.Error,
                    key: "Universum.Error.failed_to_find_utility",
                    prefix: Style.name_prefix,
                    args: new NamedArgument[] { id }
                );
                failed_attempts.Add(id, false);
                return false;
            }
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref saved_settings, "saved_settings", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref savedtotalToSpawnGenStep, "savedtotalToSpawnGenStep", LookMode.Value, LookMode.Value);
            if (saved_settings == null) saved_settings = new Dictionary<string, bool>();
            if (savedtotalToSpawnGenStep == null) savedtotalToSpawnGenStep = new Dictionary<string, int>();
        }
    }

    public class Settings_Page : Mod {
        public static Settings settings;

        public Settings_Page(ModContentPack content) : base(content) => settings = GetSettings<Settings>();

        public override void WriteSettings() {
            Settings.saved_settings = new Dictionary<string, bool>();
            Settings.savedtotalToSpawnGenStep = new Dictionary<string, int>();

            foreach (KeyValuePair<string, ObjectsDef.Metadata> utility in Settings.utilities) {
                Settings.saved_settings.Add(utility.Value.id, utility.Value.toggle);
            }

            foreach (var (defName, total) in Settings.totalToSpawnGenStep) Settings.savedtotalToSpawnGenStep.Add(defName, total);

            base.WriteSettings();
        }
    }
}
