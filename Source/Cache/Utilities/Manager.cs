namespace Universum.Cache.Utilities;

public static class Manager {
    public static readonly OceanMasking OCEAN_MASKING = new();
    public static readonly RemoveShadows REMOVE_SHADOWS = new();
    public static readonly Temperature TEMPERATURE = new();
    public static readonly Vacuum VACUUM = new();
    public static readonly VacuumOverlay VACUUM_OVERLAY = new();
    public static readonly WeatherChanger WEATHER_CHANGER = new();

    private static readonly SubscriptionTracker[] UTILITIES = [
        OCEAN_MASKING,
        REMOVE_SHADOWS,
        TEMPERATURE,
        VACUUM,
        VACUUM_OVERLAY,
        WEATHER_CHANGER
    ];

    private static readonly int TOTAL_UTILITIES = UTILITIES.Length;

    public static void Init() {
        OCEAN_MASKING.Init(
            key: "universum.ocean_masking",
            classesToPatch: [
                typeof(World.Patch.BiomeDef.DrawMaterial),
                typeof(World.Patch.WorldLayer.Tile)
            ],
            alwaysActive: true
        );
        
        REMOVE_SHADOWS.Init(
            key: "universum.remove_shadows",
            classesToPatch: [
                typeof(Colony.Patch.SkyManager.SkyManagerUpdate),
                typeof(Colony.Patch.GenCelestial.CelestialSunGlow)
            ],
            alwaysActive: false
        );
        
        TEMPERATURE.Init(
            key: "universum.temperature",
            classesToPatch: [
                typeof(Colony.Patch.MapTemperature.OutdoorTemp),
                typeof(Colony.Patch.MapTemperature.SeasonalTemp),
                typeof(Colony.Patch.RoomTempTracker.WallEqualizationTempChangePerInterval),
                typeof(Colony.Patch.RoomTempTracker.ThinRoofEqualizationTempChangePerInterval),
                typeof(Colony.Patch.RoomTempTracker.EqualizeTemperature)
            ],
            alwaysActive: false
        );
        
        VACUUM.Init(
            key: "universum.vacuum",
            classesToPatch: [
                typeof(Colony.Patch.ExitMapGrid.Color),
                typeof(Colony.Patch.PollutionGrid.SetPolluted),
                typeof(Colony.Patch.Room.NotifyTerrainChanged),
                typeof(Colony.Patch.GlobalControls.TemperatureString),
                typeof(Colony.Patch.District.OpenRoofCountStopAt)
            ],
            alwaysActive: false
        );
        
        VACUUM_OVERLAY.Init(
            key: "universum.vacuum_overlay",
            classesToPatch: [
                typeof(Colony.Patch.MapDrawer.DrawMapMesh),
                typeof(Colony.Patch.SectionLayer.FinalizeMesh),
                typeof(Colony.Patch.SectionLayerTerrain.Regenerate),
                typeof(Game.Patch.Game.UpdatePlay),
                typeof(Colony.Patch.Section.Constructor)
            ],
            alwaysActive: false
        );
        
        WEATHER_CHANGER.Init(
            key: "universum.disable_weather_change",
            classesToPatch: [
                typeof(Colony.Patch.WeatherDecider.CurrentWeatherCommonality)
            ],
            alwaysActive: false
        );
    }

    public static void Reset() {
        for (int i = 0; i < TOTAL_UTILITIES; i++) UTILITIES[i].Reset();
    }

    public static void ResetPatching() {
        for (int i = 0; i < TOTAL_UTILITIES; i++) UTILITIES[i].ResetPatching();
    }

    public static void UpdateSettings(int id, bool value) {
        for (int i = 0; i < TOTAL_UTILITIES; i++) {
            if (id != UTILITIES[i].id) continue;
            
            UTILITIES[i].SetSettingsEnabled(value);
            return;
        }
    }
}
