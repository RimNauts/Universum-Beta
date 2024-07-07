using System.Reflection;
using HarmonyLib;

namespace Universum.Mod;

[Verse.StaticConstructorOnStartup]
public static class Manager {
    public static readonly Metadata METADATA;
    public static readonly Harmony HARMONY;
    
    static Manager() {
        METADATA = Metadata.Instance;

        
        Debugger.Log(
            key: "Universum.Info.mod_loaded",
            args: [METADATA.NAME, METADATA.VERSION]
        );

        Loader.Defs.Init();
        Loader.Assets.Init();
        
        HARMONY = new Harmony(METADATA.ID);
        HARMONY.PatchAll(Assembly.GetExecutingAssembly());
        
        Cache.Utilities.Vacuum.Init(HARMONY);
    }
}
