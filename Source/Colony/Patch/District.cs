using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

public static class District {
    private const string TYPE_NAME = "Verse.District";

    [HarmonyPatch]
    public static class OpenRoofCountStopAt {
        private const string METHOD_NAME = $"{TYPE_NAME}:OpenRoofCountStopAt";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(int threshold, Verse.District __instance, ref int __result) {
            if (__result >= threshold) return;

            Verse.Map map = __instance.Map;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            IEnumerator<Verse.IntVec3> cells = __instance.Cells.GetEnumerator();
            Verse.TerrainGrid terrainGrid = map.terrainGrid;
            
            while (__result < threshold && cells.MoveNext()) {
                if (Loader.Defs.TerrainProperties[terrainGrid.TerrainAt(cells.Current).index].activeUtilities[Cache.Utilities.Vacuum.id]) {
                    __result++;
                }
            }
            
            cells.Dispose();
        }
    }
}
