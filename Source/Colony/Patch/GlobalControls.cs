using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class GlobalControls {
    public static class TemperatureString {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.GlobalControls:TemperatureString");
        
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
