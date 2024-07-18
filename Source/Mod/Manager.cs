using System.Reflection;
using HarmonyLib;

namespace Universum.Mod;

[Verse.StaticConstructorOnStartup]
public static class Manager {
    public static readonly Metadata METADATA = Metadata.Instance;
    public static readonly Harmony HARMONY = new(METADATA.ID);

    static Manager() {
        Debugger.Log(
            key: "Universum.Info.mod_loaded",
            args: [METADATA.NAME, METADATA.VERSION]
        );

        Loader.Defs.Init();
        Loader.Assets.Init();
        
        Cache.Utilities.Manager.Init();
        
        Cache.Utilities.VacuumDamage.Init();
        
        Settings.Init();
        
        HARMONY.PatchAll(Assembly.GetExecutingAssembly());
        Cache.Utilities.Manager.ResetPatching();
    }
}
