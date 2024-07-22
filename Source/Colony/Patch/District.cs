using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

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
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex]) return;

            IEnumerator<Verse.IntVec3> cells = __instance.Cells.GetEnumerator();
            while (__result < threshold && cells.MoveNext()) {
                int terrainIndex = map.terrainGrid.TerrainAt(cells.Current).index;
                if (!Cache.Utilities.Manager.VACUUM.CheckTerrain(terrainIndex)) continue;
                
                __result++;
            }
            
            cells.Dispose();
        }
    }
}
