using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Game.Patch;

public static class Game {
    private const string TYPE_NAME = "Verse.Game";
    
    [HarmonyPatch]
    private class FinalizeInit {
        private const string METHOD_NAME = $"{TYPE_NAME}:FinalizeInit";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix() {
            MainLoop.world = Verse.Find.World;
            MainLoop.tickManager = Verse.Find.TickManager;
            MainLoop.worldCameraDriver = Verse.Find.WorldCameraDriver;
            MainLoop.worldCamera = Verse.Find.WorldCameraDriver.GetComponent<Camera>();
            MainLoop.worldSkyboxCamera = RimWorld.Planet.WorldCameraManager.WorldSkyboxCamera;
            MainLoop.colonyCameraDriver = Verse.Find.CameraDriver;
            MainLoop.colonyCamera = Verse.Find.CameraDriver.GetComponent<Camera>();

            Colony.Patch.MapDrawer.rendered = false;
        }
    }
    
    [HarmonyPatch]
    private static class AddMap {
        private const string METHOD_NAME = $"{TYPE_NAME}:AddMap";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix(Verse.Map map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            if (mapIndex == -1) return;
            
            int biomeIndex = map.Biome.index;

            Cache.Utilities.RemoveShadows.UpdateMapValue(mapIndex, biomeIndex);
            Cache.Utilities.Temperature.UpdateMapValue(mapIndex, biomeIndex);
            Cache.Utilities.Vacuum.UpdateMapValue(mapIndex, biomeIndex);
            Cache.Utilities.VacuumOverlay.UpdateMapValue(mapIndex, biomeIndex);
            Cache.Utilities.WeatherChanger.UpdateMapValue(mapIndex, biomeIndex);
        }
    }

    [HarmonyPatch]
    private static class DeinitAndRemoveMap {
        private const string METHOD_NAME = $"{TYPE_NAME}:DeinitAndRemoveMap";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix(Verse.Map map, bool notifyPlayer) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            if (mapIndex == -1) return;

            if (Cache.Utilities.RemoveShadows.maps[mapIndex]) {
                Cache.Utilities.RemoveShadows.maps[mapIndex] = false;
                Cache.Utilities.RemoveShadows.tracker.Unsubscribe();
            }

            if (Cache.Utilities.Temperature.maps[mapIndex]) {
                Cache.Utilities.Temperature.maps[mapIndex] = false;
                Cache.Utilities.Temperature.tracker.Unsubscribe();
            }

            if (Cache.Utilities.Vacuum.maps[mapIndex]) {
                Cache.Utilities.Vacuum.maps[mapIndex] = false;
                Cache.Utilities.Vacuum.tracker.Unsubscribe();
            }

            if (Cache.Utilities.WeatherChanger.maps[mapIndex]) {
                Cache.Utilities.WeatherChanger.maps[mapIndex] = false;
                Cache.Utilities.WeatherChanger.tracker.Unsubscribe();
            }
        }
    }

    [HarmonyPatch]
    private static class LoadGame {
        private const string METHOD_NAME = $"{TYPE_NAME}:LoadGame";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix() {
            List<Verse.Map> maps = Verse.Current.gameInt.maps;
            int mapCount = maps.Count;

            for (int mapIndex = 0; mapIndex < mapCount; mapIndex++) {
                int biomeIndex = maps[mapIndex].Biome.index;
                
                Cache.Utilities.RemoveShadows.UpdateMapValue(mapIndex, biomeIndex);
                Cache.Utilities.Temperature.UpdateMapValue(mapIndex, biomeIndex);
                Cache.Utilities.Vacuum.UpdateMapValue(mapIndex, biomeIndex);
                Cache.Utilities.VacuumOverlay.UpdateMapValue(mapIndex, biomeIndex);
                Cache.Utilities.WeatherChanger.UpdateMapValue(mapIndex, biomeIndex);
            }
        }
    }

    [HarmonyPatch]
    public static class UpdatePlay {
        public static class MeshRecalculateHelper {
            public static readonly List<Task> TASKS = [];
            public static readonly List<Verse.SectionLayer> LAYERS_TO_DRAW = [];

            public static void RecalculateLayer(Verse.SectionLayer instance) {
                var vacuumTerrainMesh = instance.GetSubMesh(Colony.Patch.SectionLayer.vacuumTerrainMaterial);
                var vacuumGlassTerrainMesh = instance.GetSubMesh(Colony.Patch.SectionLayer.vacuumGlassTerrainMaterial);

                if (vacuumTerrainMesh.verts.Count > 0) TASKS.Add(Task.Factory.StartNew(() => RecalculateMesh(vacuumTerrainMesh)));
                if (vacuumGlassTerrainMesh.verts.Count > 0) TASKS.Add(Task.Factory.StartNew(() => RecalculateMesh(vacuumGlassTerrainMesh)));

                LAYERS_TO_DRAW.Add(instance);
            }

            private static void RecalculateMesh(object info) {
                if (info is not Verse.LayerSubMesh mesh) {
                    Debugger.Log(
                        key: "Universum.Error.thread_with_wrong_type",
                        prefix: $"{Mod.Manager.METADATA.NAME}: ",
                        severity: Debugger.Severity.Error
                    );
                    return;
                }

                lock (mesh) {
                    mesh.finalized = false;
                    mesh.Clear(Verse.MeshParts.UVs);

                    int totalVerts = mesh.verts.Count;
                    for (int i = 0; i < totalVerts; i++) {
                        var xDiff = mesh.verts[i].x - _center.x;
                        var xFromEdge = xDiff + _cellsWide / 2.0f;
                        var zDiff = mesh.verts[i].z - _center.z;
                        var zFromEdge = zDiff + _cellsHigh / 2.0f;

                        mesh.uvs.Add(new Vector3(xFromEdge / _cellsWide, zFromEdge / _cellsHigh, 0.0f));
                    }

                    mesh.FinalizeMesh(Verse.MeshParts.UVs);
                }
            }
        }
        
        private const string METHOD_NAME = $"{TYPE_NAME}:UpdatePlay";
        private static bool _verboseError = true;

        private static Vector3 _center;
        private static float _cellsHigh;
        private static float _cellsWide;
        private static readonly Dictionary<Verse.Map, Dictionary<Verse.Section, Verse.SectionLayer>> MAP_SECTIONS = new();
        private static Vector3 _lastCameraPosition = new(float.MaxValue, float.MaxValue, float.MaxValue);

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix() {
            Verse.Map map = Verse.Find.CurrentMap;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            if (mapIndex == -1 || !Cache.Utilities.Vacuum.maps[mapIndex]) return;
            if (!MAP_SECTIONS.TryGetValue(map, out Dictionary<Verse.Section, Verse.SectionLayer> sections)) return;
            
            _center = MainLoop.colonyCamera.transform.position;
            float ratio = (float) Verse.UI.screenWidth / Verse.UI.screenHeight;
            _cellsHigh = Verse.UI.screenHeight / MainLoop.colonyCameraDriver.CellSizePixels;
            _cellsWide = _cellsHigh * ratio;

            if ((_lastCameraPosition - _center).magnitude < 1e-4) return;

            _lastCameraPosition = _center;
            Verse.CellRect visibleRect = MainLoop.colonyCameraDriver.CurrentViewRect;
            foreach (var entry in sections.Where(entry => visibleRect.Overlaps(entry.Key.CellRect))) {
                MeshRecalculateHelper.RecalculateLayer(entry.Value);
            }
        }

        public static void Postfix() {
            if (MeshRecalculateHelper.TASKS.Count == 0) return;

            Task.WaitAll(MeshRecalculateHelper.TASKS.ToArray());
            MeshRecalculateHelper.TASKS.Clear();

            foreach (var layer in MeshRecalculateHelper.LAYERS_TO_DRAW) {
                var vacuumTerrainMesh = layer.GetSubMesh(Colony.Patch.SectionLayer.vacuumTerrainMaterial);
                var vacuumGlassTerrainMesh = layer.GetSubMesh(Colony.Patch.SectionLayer.vacuumGlassTerrainMaterial);

                if (!(!vacuumTerrainMesh.finalized || vacuumTerrainMesh.disabled)) {
                    Graphics.DrawMesh(
                        vacuumTerrainMesh.mesh,
                        Vector3.zero,
                        Quaternion.identity,
                        vacuumTerrainMesh.material,
                        layer: 0
                    );
                }

                if (!(!vacuumGlassTerrainMesh.finalized || vacuumGlassTerrainMesh.disabled)) {
                    Graphics.DrawMesh(
                        vacuumGlassTerrainMesh.mesh,
                        Vector3.zero,
                        Quaternion.identity,
                        vacuumGlassTerrainMesh.material,
                        layer: 0
                    );
                }
            }

            MeshRecalculateHelper.LAYERS_TO_DRAW.Clear();
        }
        
        public static void AddSection(Verse.Map map, Verse.Section section, Verse.SectionLayer layer) {
            if (!MAP_SECTIONS.TryGetValue(map, out Dictionary<Verse.Section, Verse.SectionLayer> sections)) {
                sections = new Dictionary<Verse.Section, Verse.SectionLayer>();
                MAP_SECTIONS.Add(map, sections);
            }
            
            sections.Add(section, layer);
        }
    }
}
