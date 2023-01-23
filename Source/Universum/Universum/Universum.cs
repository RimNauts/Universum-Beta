﻿using System.Reflection;

namespace Universum {
    [Verse.StaticConstructorOnStartup]
    public static class Universum {
        static Universum() {
            // fix for Transparent_Vacuum.SectionRegenerateHelper
            // source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/ShipInteriorMod2.cs#L129
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("sindre0830.universum");
            harmony.Patch(
                original: HarmonyLib.AccessTools.TypeByName("SectionLayer_Terrain").GetMethod("Regenerate"),
                postfix: new HarmonyLib.HarmonyMethod(typeof(Utilities.SectionRegenerateHelper).GetMethod("Postfix"))
            );
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // print mod info
            Logger.print(
                Logger.Importance.Info,
                key: "Universum.Info.mod_loaded",
                args: new Verse.NamedArgument[] { Info.name, Info.version }
            );
            // load configuarations
            Settings.init();
            Utilities.Biome.Handler.init();
            Utilities.Terrain.Handler.init();
        }
    }
}
