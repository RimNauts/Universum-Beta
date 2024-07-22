using System;
using System.Collections.Generic;
using UnityEngine;

namespace Universum.Mod;

public class Settings : Verse.ModSettings {
    private static Vector2 _scrollPosUtilities = Vector2.zero;
    private static Vector2 _scrollPosObjectGeneration = Vector2.zero;
    
    public static void Init() {
        Manager.METADATA.modSettings = Manager.METADATA.GetSettings<Settings>();
        
        Debugger.Log(
            key: "Universum.Info.settings_loader_done",
            prefix: Debugger.TAB,
            args: [Cache.Settings.TotalConfigurations]
        );
    }
    
    public static void Window(Rect inRect) {
        DrawDefaultButton(inRect);
        DrawUtilitiesTable(inRect);
        DrawObjectGenerationTable(inRect);
    }

    private static void DrawDefaultButton(Rect inRect) {
        Rect buttonsRectangle = new Rect(inRect.x, inRect.y + 24f, inRect.width, inRect.height - 24f);
        Verse.Listing_Standard buttonsView = new Verse.Listing_Standard();
        buttonsView.Begin(buttonsRectangle);

        if (buttonsView.ButtonText(Verse.TranslatorFormattedStringExtensions.Translate("Universum.default"))) {
            Cache.Settings.ConvertUtilitiesFromExposable(null);
            Cache.Settings.ConvertCelestialBodiesCountFromExposable(null);
        }

        if (Verse.Current.Game != null && Game.MainLoop.instance != null && buttonsView.ButtonText(
            Verse.TranslatorFormattedStringExtensions.Translate("Universum.Info.regenerate"))) {
            World.Initialization.Regenerate();
            Colony.Patch.MapDrawer.rendered = false;
        }
        buttonsView.End();
    }

    private static void DrawUtilitiesTable(Rect inRect) {
        Rect tableHeaderRectangle = new Rect(inRect.x, inRect.y + 102f, inRect.width, 30f);
        DrawTableHeader(tableHeaderRectangle, "Universum.utilities", "Universum.enabled");

        Rect scrollViewRect = new Rect(tableHeaderRectangle.x, tableHeaderRectangle.y + 30f, tableHeaderRectangle.width, 200f);
        Rect contentRect = new Rect(0.0f, 0.0f, scrollViewRect.width - 20f, Loader.Defs.TotalUtilities * 30f);
        DrawUtilitiesContent(scrollViewRect, contentRect);
    }

    private static void DrawObjectGenerationTable(Rect inRect) {
        Rect tableHeaderRectangle = new Rect(inRect.x, inRect.y + 342f, inRect.width, 30f);
        DrawTableHeader(tableHeaderRectangle, "Universum.Info.SettingsGeneratorHeader", "Universum.Info.Total");

        Rect scrollViewRect = new Rect(tableHeaderRectangle.x, tableHeaderRectangle.y + 30f, tableHeaderRectangle.width, 200f);
        Rect contentRect = new Rect(0.0f, 0.0f, scrollViewRect.width - 20f, Loader.Defs.CelestialObjectGenerationSteps.Count * 45f);
        DrawObjectGenerationContent(scrollViewRect, contentRect);
    }

    private static void DrawTableHeader(Rect headerRect, string leftColumnKey, string rightColumnKey) {
        Verse.Listing_Standard headerView = new Verse.Listing_Standard();
        Verse.Widgets.DrawHighlight(headerRect);
        headerView.Begin(headerRect);
        headerView.Gap(5f);
        headerView.ColumnWidth = 460f;
        headerView.Label(Verse.TranslatorFormattedStringExtensions.Translate(leftColumnKey));
        headerView.NewColumn();
        headerView.Gap(5f);
        headerView.ColumnWidth = 100f;
        headerView.Label(Verse.TranslatorFormattedStringExtensions.Translate(rightColumnKey));
        headerView.End();
    }

    private static void DrawUtilitiesContent(Rect scrollViewRect, Rect contentRect) {
        Verse.Widgets.BeginScrollView(scrollViewRect, ref _scrollPosUtilities, contentRect);
        Verse.Listing_Standard contentList = new Verse.Listing_Standard();
        contentList.Begin(contentRect);

        foreach (Defs.Utility utilityDef in Loader.Defs.Utilities.Values) {
            if (utilityDef.hideInSettings) continue;
            int utilityId = Loader.Defs.UtilityId[utilityDef.id];
            bool isEnabled = Cache.Settings.UtilityEnabled(utilityId);
            string label = FormatUtilityLabel(utilityDef, out string tooltip);

            contentList.CheckboxLabeled(label, ref isEnabled, tooltip);
            if (Cache.Settings.UtilityEnabled(utilityId) != isEnabled) {
                Cache.Settings.SetUtility(utilityId, isEnabled);
            }
            
            contentList.GapLine(5f);
        }
        contentList.End();
        Verse.Widgets.EndScrollView();
    }
    
    private static void DrawObjectGenerationContent(Rect scrollViewRect, Rect contentRect) {
        Verse.Widgets.BeginScrollView(scrollViewRect, ref _scrollPosObjectGeneration, contentRect);
        Verse.Listing_Standard contentList = new Verse.Listing_Standard();
        contentList.Begin(contentRect);

        foreach (Defs.ObjectGeneration objectGenerationDef in Loader.Defs.CelestialObjectGenerationSteps.Values) {
            Verse.Listing_Standard section = contentList.BeginSection(30f);
            section.ColumnWidth = 460f;
            section.Label(objectGenerationDef.defName);
            section.NewColumn();
            section.ColumnWidth = 350f;

            int count = Cache.Settings.CelestialBodiesCount(objectGenerationDef.defName);
            string buffer = count.ToString();
            section.IntEntry(ref count, ref buffer);

            if (count != Cache.Settings.CelestialBodiesCount(objectGenerationDef.defName)) {
                Cache.Settings.SetCelestialBodiesCount(objectGenerationDef.defName, count);
            }

            contentList.EndSection(section);
            contentList.GapLine(5f);
        }

        contentList.End();
        Verse.Widgets.EndScrollView();
    }

    private static string FormatUtilityLabel(Defs.Utility utilityDef, out string tooltip) {
        string modName = string.IsNullOrEmpty(utilityDef.modName) ? "Unknown source" : utilityDef.modName;
        string utilityName = string.IsNullOrEmpty(utilityDef.labelKey) ? utilityDef.id : TryTranslate(utilityDef.labelKey);
        tooltip = string.IsNullOrEmpty(utilityDef.descriptionKey) ? null : TryTranslate(utilityDef.descriptionKey);
        return $"({modName}) {utilityName}";
    }

    private static string TryTranslate(string key) {
        try {
            return Verse.TranslatorFormattedStringExtensions.Translate(key);
        } catch {
            return key;
        }
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
    }

    private static void LoadData() {
        Dictionary<string, bool> exposableUtilityToggles = new();
        Dictionary<string, int> exposableCelestialBodiesCount = new();
        
        Verse.Scribe_Collections.Look(ref exposableUtilityToggles, "exposableUtilityToggles", Verse.LookMode.Value, Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref exposableCelestialBodiesCount, "exposableCelestialBodiesCount", Verse.LookMode.Value, Verse.LookMode.Value);
        
        Cache.Settings.ConvertUtilitiesFromExposable(exposableUtilityToggles);
        Cache.Settings.ConvertCelestialBodiesCountFromExposable(exposableCelestialBodiesCount);
    }
}
