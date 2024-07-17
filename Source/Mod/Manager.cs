using System.Reflection;
using HarmonyLib;

namespace Universum.Mod;

[Verse.StaticConstructorOnStartup]
public static class Manager {
    public static readonly Metadata METADATA;

    static Manager() {
        METADATA = Metadata.Instance;
        
        Debugger.Log(
            key: "Universum.Info.mod_loaded",
            args: [METADATA.NAME, METADATA.VERSION]
        );

        Loader.Defs.Init();
        Loader.Assets.Init();
        
        Harmony harmony = new Harmony(METADATA.ID);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        Cache.Utilities.OceanMasking.Init(harmony);
        Cache.Utilities.RemoveShadows.Init(harmony);
        Cache.Utilities.Temperature.Init(harmony);
        Cache.Utilities.Vacuum.Init(harmony);
        Cache.Utilities.VacuumDamage.Init();
        Cache.Utilities.VacuumOverlay.Init(harmony);
        Cache.Utilities.WeatherChanger.Init(harmony);
        
        Settings.Init();
    }
}
