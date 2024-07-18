using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class GlobalControls {
    private const string TYPE_NAME = "RimWorld.GlobalControls";
    
    [HarmonyPatch]
    public static class TemperatureString {
        private const string METHOD_NAME = $"{TYPE_NAME}:TemperatureString";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(ref string __result) {
            if (__result is null) return;

            Verse.Map map = Verse.Find.CurrentMap;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            if (__result.Contains(Loader.Assets.IndoorsText)) {
                __result = __result.Replace(Loader.Assets.IndoorsText, Loader.Assets.CustomIndoorsText);
            } else if (__result.Contains(Loader.Assets.UnroofedText)) {
                __result = __result.Replace(Loader.Assets.UnroofedText, Loader.Assets.CustomUnroofedText);
            } else if (__result.Contains(Loader.Assets.OutdoorsText)) {
                __result = __result.Replace(Loader.Assets.OutdoorsText, Loader.Assets.CustomOutdoorsText);
            }
        }
    }
}
