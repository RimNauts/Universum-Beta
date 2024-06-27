﻿using System;
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
        private UnityEngine.Vector2 scrollpos = UnityEngine.Vector2.zero;
        string input_buffer;

        public Settings_Page(ModContentPack content) : base(content) => settings = GetSettings<Settings>();

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect) {
            // default button
            UnityEngine.Rect buttons_rectangle = new UnityEngine.Rect(inRect.x, inRect.y + 24f, inRect.width, inRect.height - 24f);
            Listing_Standard buttons_view = new Listing_Standard();
            buttons_view.Begin(buttons_rectangle);
            if (buttons_view.ButtonText(TranslatorFormattedStringExtensions.Translate("Universum.default"))) {
                foreach (KeyValuePair<string, ObjectsDef.Metadata> utility in Settings.utilities) Settings.utilities[utility.Key].toggle = Settings.utilities[utility.Key].default_toggle;

                foreach (var (defName, def) in Defs.Loader.celestialObjectGenerationStartUpSteps) Settings.totalToSpawnGenStep[defName] = def.total;
                foreach (var (defName, def) in Defs.Loader.celestialObjectGenerationRandomSteps) Settings.totalToSpawnGenStep[defName] = def.total;

                try {
                    Utilities.Cache.clear_utility_toggle();
                    Utilities.CachingHandler.ResetSettingsEnabledCache();
                } catch { }
            }
            if (Current.Game != null && Game.MainLoop.instance != null && buttons_view.ButtonText(TranslatorFormattedStringExtensions.Translate("Universum.Info.regenerate"))) {
                World.Generator.Regenerate();
            }
            buttons_view.End();
            // table header
            UnityEngine.Rect table_header_rectangle = new UnityEngine.Rect(buttons_rectangle.x, buttons_rectangle.y + 34f * 3, buttons_rectangle.width, 30f);
            Listing_Standard table_header_view = new Listing_Standard();
            Widgets.DrawHighlight(table_header_rectangle);
            table_header_view.Begin(table_header_rectangle);
            table_header_view.Gap(5f);
            table_header_view.ColumnWidth = 460f;
            table_header_view.Label(TranslatorFormattedStringExtensions.Translate("Universum.utilities"));
            table_header_view.NewColumn();
            table_header_view.Gap(5f);
            table_header_view.ColumnWidth = 100f;
            table_header_view.Label(TranslatorFormattedStringExtensions.Translate("Universum.enabled"));
            table_header_view.End();
            // table content
            UnityEngine.Rect table_content_rectangle = new UnityEngine.Rect(table_header_rectangle.x, table_header_rectangle.y - 34f * 4, table_header_rectangle.width, Settings.utilities.Count * 38f);
            UnityEngine.Rect viewRect = new UnityEngine.Rect(0.0f, 0.0f, 100f, Settings.utilities.Count * 30f);
            Widgets.BeginScrollView(new UnityEngine.Rect(table_content_rectangle.x, table_content_rectangle.y + 34f * 5, table_content_rectangle.width, 150f), ref scrollpos, viewRect);
            Listing_Standard table_header_content = new Listing_Standard();
            table_header_content.Begin(table_content_rectangle);
            table_header_content.verticalSpacing = 8f;
            table_header_content.ColumnWidth = 500f;
            table_header_content.Gap(4f);
            foreach (KeyValuePair<string, ObjectsDef.Metadata> utility in Settings.utilities) {
                if (utility.Value.hide_in_settings) continue;
                bool checkOn = utility.Value.toggle;
                string mod_name = "Unknown source";
                if (utility.Value.mod_name != null && utility.Value.mod_name.Length > 0) mod_name = utility.Value.mod_name;
                string utility_name = utility.Key;
                if (utility.Value.label_key != null && utility.Value.label_key.Length > 0) {
                    try {
                        utility_name = TranslatorFormattedStringExtensions.Translate(utility.Value.label_key);
                    } catch { /* couldn't find the language key provided */ }
                }
                string label = "(" + mod_name + ") " + utility_name;
                string utility_description = null;
                if (utility.Value.description_key != null && utility.Value.description_key.Length > 0) {
                    try {
                        utility_description = TranslatorFormattedStringExtensions.Translate(utility.Value.description_key);
                    } catch { /* couldn't find the language key provided */ }
                }
                table_header_content.CheckboxLabeled(label, ref checkOn, tooltip: utility_description);
                if (Settings.utilities[utility.Key].toggle != checkOn) {
                    Settings.utilities[utility.Key].toggle = checkOn;
                    try {
                        Utilities.Cache.clear_utility_toggle();
                        Utilities.CachingHandler.ResetSettingsEnabledCache();
                    } catch { }
                }
            }
            table_header_content.End();
            Widgets.EndScrollView();
            // table header
            UnityEngine.Rect table_header_rectangle_new = new UnityEngine.Rect(table_content_rectangle.x, table_content_rectangle.y + 34f * 10, table_content_rectangle.width, 30f);
            Listing_Standard table_header_view_new = new Listing_Standard();
            Widgets.DrawHighlight(table_header_rectangle_new);
            table_header_view_new.Begin(table_header_rectangle_new);
            table_header_view_new.Gap(5f);
            table_header_view_new.ColumnWidth = 460f;
            table_header_view_new.Label("Generator step def name");
            table_header_view_new.NewColumn();
            table_header_view_new.Gap(5f);
            table_header_view_new.ColumnWidth = 100f;
            table_header_view_new.Label("Total");
            table_header_view_new.End();
            // table content
            UnityEngine.Rect table_content_rectangle_new = new UnityEngine.Rect(table_header_rectangle_new.x, table_header_rectangle_new.y + 34f * 2, table_header_rectangle_new.width, Settings.utilities.Count * 38f);
            Listing_Standard table_header_content_new = new Listing_Standard();
            table_header_content_new.Begin(table_content_rectangle_new);
            foreach (var (defName, _) in Defs.Loader.celestialObjectGenerationStartUpSteps) {
                int buffer = Settings.totalToSpawnGenStep[defName];
                Listing_Standard row_view = table_header_content_new.BeginSection(30f);
                row_view.Gap(5f);
                row_view.ColumnWidth = 460f;
                row_view.Label(defName);
                row_view.NewColumn();
                row_view.Gap(5f);
                row_view.ColumnWidth = 350f;
                input_buffer = buffer.ToString();
                row_view.IntEntry(ref buffer, ref input_buffer);
                Settings.totalToSpawnGenStep[defName] = buffer;
                table_header_content_new.EndSection(row_view);
            }
            foreach (var (defName, _) in Defs.Loader.celestialObjectGenerationRandomSteps) {
                int buffer = Settings.totalToSpawnGenStep[defName];
                Listing_Standard row_view = table_header_content_new.BeginSection(30f);
                row_view.Gap(5f);
                row_view.ColumnWidth = 460f;
                row_view.Label(defName);
                row_view.NewColumn();
                row_view.Gap(5f);
                row_view.ColumnWidth = 350f;
                input_buffer = buffer.ToString();
                row_view.IntEntry(ref buffer, ref input_buffer);
                Settings.totalToSpawnGenStep[defName] = buffer;
                table_header_content_new.EndSection(row_view);
            }
            table_header_content_new.End();
        }

        public override string SettingsCategory() => "Universum";

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
