using System.Collections.Generic;
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

public static class District {
    public static class OpenRoofCountStopAt {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.District:OpenRoofCountStopAt");
        
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
        }
    }
}
