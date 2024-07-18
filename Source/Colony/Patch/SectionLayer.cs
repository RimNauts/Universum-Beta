using System.Reflection;
using HarmonyLib;
using UnityEngine;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

[Verse.StaticConstructorOnStartup]
public static class SectionLayer {
    private const string TYPE_NAME = "Verse.SectionLayer";
    public static Material vacuumTerrainMaterial;
    public static Material vacuumGlassTerrainMaterial;
    public static Verse.TerrainDef vacuumGlassTerrainDef;
    
    [HarmonyPatch]
    public static class FinalizeMesh {
        private const string METHOD_NAME = $"{TYPE_NAME}:FinalizeMesh";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Prefix(Verse.SectionLayer __instance, Verse.Section ___section) {
            if (__instance is not Verse.SectionLayer_Terrain) return;
            
            Verse.Map map = ___section.map;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            bool foundSpace = false;
            foreach (Verse.IntVec3 cell in ___section.CellRect.Cells) {
                Verse.TerrainDef terrain = map.terrainGrid.TerrainAt(cell);

                if (!Loader.Defs.TerrainProperties[terrain.index].activeUtilities[Cache.Utilities.VacuumOverlay.id]) {
                    continue;
                }
                
                foundSpace = true;
                Material terrainMaterial = vacuumTerrainMaterial;

                if (terrain == vacuumGlassTerrainDef) terrainMaterial = vacuumGlassTerrainMaterial;

                Verse.Printer_Mesh.PrintMesh(
                    __instance,
                    Matrix4x4.TRS(cell.ToVector3() + new Vector3(0.5f, 0f, 0.5f), Quaternion.identity, Vector3.one),
                    Verse.MeshMakerPlanes.NewPlaneMesh(1f),
                    terrainMaterial
                );
            }

            if (foundSpace) return;
            
            // is this needed?
            for (int i = 0; i < __instance.subMeshes.Count; i++) {
                if (__instance.subMeshes[i].material == vacuumTerrainMaterial ||
                    __instance.subMeshes[i].material == vacuumGlassTerrainMaterial) {
                    __instance.subMeshes.RemoveAt(i);
                }
            }
        }
    }
}
