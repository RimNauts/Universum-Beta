using System;
using System.Collections.Generic;
using UnityEngine;

namespace Universum.Mod;

public class Settings : Verse.ModSettings {
    private static Vector2 _scrollPos = Vector2.zero;
    private static string _inputBuffer;
    
    public static void Init() {
        Cache.Settings.ConvertUtilitiesFromExposable(null);
        Cache.Settings.ConvertCelestialBodiesCountFromExposable(null);
        
        Debugger.Log(
            key: "Universum.Info.settings_loader_done",
            prefix: Debugger.TAB,
            args: [Cache.Settings.TotalConfigurations]
        );
    }
    
    public static void Window(Rect inRect) {
        // default button
        Rect buttonsRectangle = new Rect(inRect.x, inRect.y + 24f, inRect.width, inRect.height - 24f);
        Verse.Listing_Standard buttonsView = new Verse.Listing_Standard();
        buttonsView.Begin(buttonsRectangle);
        if (buttonsView.ButtonText(Verse.TranslatorFormattedStringExtensions.Translate("Universum.default"))) {
            Cache.Settings.ConvertUtilitiesFromExposable(exposableUtilityToggles: null);
            Cache.Settings.ConvertCelestialBodiesCountFromExposable(exposableCelestialBodiesCount: null);
        }
        if (Verse.Current.Game != null && Game.MainLoop.instance != null && buttonsView.ButtonText(
            Verse.TranslatorFormattedStringExtensions.Translate("Universum.Info.regenerate")
        )) {
            World.Initialization.Regenerate();
        }
        buttonsView.End();
        
        // table header
        Rect tableHeaderRectangle = new Rect(buttonsRectangle.x, buttonsRectangle.y + 34f * 3, buttonsRectangle.width, 30f);
        Verse.Listing_Standard tableHeaderView = new Verse.Listing_Standard();
        Verse.Widgets.DrawHighlight(tableHeaderRectangle);
        tableHeaderView.Begin(tableHeaderRectangle);
        tableHeaderView.Gap(5f);
        tableHeaderView.ColumnWidth = 460f;
        tableHeaderView.Label(Verse.TranslatorFormattedStringExtensions.Translate("Universum.utilities"));
        tableHeaderView.NewColumn();
        tableHeaderView.Gap(5f);
        tableHeaderView.ColumnWidth = 100f;
        tableHeaderView.Label(Verse.TranslatorFormattedStringExtensions.Translate("Universum.enabled"));
        tableHeaderView.End();
        
        // table content
        Rect tableContentRectangle = new Rect(tableHeaderRectangle.x, tableHeaderRectangle.y - 34f * 4, tableHeaderRectangle.width, Loader.Defs.TotalUtilities * 38f);
        Rect viewRect = new Rect(0.0f, 0.0f, 100f, Loader.Defs.TotalUtilities * 30f);
        Verse.Widgets.BeginScrollView(new Rect(tableContentRectangle.x, tableContentRectangle.y + 34f * 5, tableContentRectangle.width, 150f), ref _scrollPos, viewRect);
        Verse.Listing_Standard tableHeaderContent = new Verse.Listing_Standard();
        tableHeaderContent.Begin(tableContentRectangle);
        tableHeaderContent.verticalSpacing = 8f;
        tableHeaderContent.ColumnWidth = 500f;
        tableHeaderContent.Gap(4f);
        foreach (Defs.Utility utilityDef in Loader.Defs.Utilities.Values) {
            if (utilityDef.hideInSettings) continue;

            int utilityId = Loader.Defs.UtilityId[utilityDef.id];
            
            bool checkOn = Cache.Settings.UtilityEnabled(utilityId);
            string modName = "Unknown source";
            if (!string.IsNullOrEmpty(utilityDef.modName)) modName = utilityDef.modName;
            string utilityName = utilityDef.id;
            if (!string.IsNullOrEmpty(utilityDef.labelKey)) {
                try {
                    utilityName = Verse.TranslatorFormattedStringExtensions.Translate(utilityDef.labelKey);
                } catch {
                    // ignored
                }
            }
            string label = $"({modName}) {utilityName}";
            string utilityDescription = null;
            if (!string.IsNullOrEmpty(utilityDef.descriptionKey)) {
                try {
                    utilityDescription = Verse.TranslatorFormattedStringExtensions.Translate(utilityDef.descriptionKey);
                } catch {
                    // ignored
                }
            }
            tableHeaderContent.CheckboxLabeled(label, ref checkOn, tooltip: utilityDescription);
            if (Cache.Settings.UtilityEnabled(utilityId) != checkOn) {
                Cache.Settings.SetUtility(utilityId, checkOn);
            }
        }
        tableHeaderContent.End();
        Verse.Widgets.EndScrollView();
        
        // table header
        Rect tableHeaderRectangleNew = new Rect(tableContentRectangle.x, tableContentRectangle.y + 34f * 10, tableContentRectangle.width, 30f);
        Verse.Listing_Standard tableHeaderViewNew = new Verse.Listing_Standard();
        Verse.Widgets.DrawHighlight(tableHeaderRectangleNew);
        tableHeaderViewNew.Begin(tableHeaderRectangleNew);
        tableHeaderViewNew.Gap(5f);
        tableHeaderViewNew.ColumnWidth = 460f;
        tableHeaderViewNew.Label("Generator step def name");
        tableHeaderViewNew.NewColumn();
        tableHeaderViewNew.Gap(5f);
        tableHeaderViewNew.ColumnWidth = 100f;
        tableHeaderViewNew.Label("Total");
        tableHeaderViewNew.End();

        // table content
        Rect tableContentRectangleNew = new Rect(tableHeaderRectangleNew.x, tableHeaderRectangleNew.y + 34f * 2, tableHeaderRectangleNew.width, Loader.Defs.TotalUtilities * 38f);
        Verse.Listing_Standard tableHeaderContentNew = new Verse.Listing_Standard();
        tableHeaderContentNew.Begin(tableContentRectangleNew);
        foreach (Defs.ObjectGeneration objectGenerationDef in Loader.Defs.CelestialObjectGenerationSteps.Values) {
            int buffer = Cache.Settings.CelestialBodiesCount(objectGenerationDef.defName);
            Verse.Listing_Standard rowView = tableHeaderContentNew.BeginSection(30f);
            rowView.Gap(5f);
            rowView.ColumnWidth = 460f;
            rowView.Label(objectGenerationDef.defName);
            rowView.NewColumn();
            rowView.Gap(5f);
            rowView.ColumnWidth = 350f;
            _inputBuffer = buffer.ToString();
            rowView.IntEntry(ref buffer, ref _inputBuffer);
            Cache.Settings.SetCelestialBodiesCount(objectGenerationDef.defName, buffer);
            tableHeaderContentNew.EndSection(rowView);
        }
        tableHeaderContentNew.End();
    }

    public override void ExposeData() {
        switch (Verse.Scribe.mode) {
            case Verse.LoadSaveMode.Saving:
                SaveData();
                break;
            case Verse.LoadSaveMode.LoadingVars:
                LoadData();
                break;
            case Verse.LoadSaveMode.PostLoadInit:
                PostLoadData();
                break;
            case Verse.LoadSaveMode.Inactive:
                break;
            case Verse.LoadSaveMode.ResolvingCrossRefs:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void SaveData() {
        Dictionary<string, bool> exposableUtilityToggles = Cache.Settings.ConvertUtilitiesToExposable();
        Dictionary<string, int> exposableCelestialBodiesCount = Cache.Settings.ConvertCelestialBodiesCountToExposable();
        
        Verse.Scribe_Collections.Look(ref exposableUtilityToggles, "exposableUtilityToggles", Verse.LookMode.Value, Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref exposableCelestialBodiesCount, "exposableCelestialBodiesCount", Verse.LookMode.Value, Verse.LookMode.Value);
        
        Debugger.Log(message: "saved");
    }

    private static void LoadData() {
        Dictionary<string, bool> exposableUtilityToggles = new();
        Dictionary<string, int> exposableCelestialBodiesCount = new();
        
        Verse.Scribe_Collections.Look(ref exposableUtilityToggles, "exposableUtilityToggles", Verse.LookMode.Value, Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref exposableCelestialBodiesCount, "exposableCelestialBodiesCount", Verse.LookMode.Value, Verse.LookMode.Value);
        
        Cache.Settings.ConvertUtilitiesFromExposable(exposableUtilityToggles);
        Cache.Settings.ConvertCelestialBodiesCountFromExposable(exposableCelestialBodiesCount);
        
        Debugger.Log(message: "loaded");
    }

    private static void PostLoadData() {
        Cache.Utilities.Vacuum.tracker.SetSettingsEnabled(Cache.Settings.UtilityEnabled(Cache.Utilities.Vacuum.id));
    }
}
