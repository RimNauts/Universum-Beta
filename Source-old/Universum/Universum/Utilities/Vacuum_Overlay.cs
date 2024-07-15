﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Universum.World;
using Verse;

namespace Universum.Utilities {
    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/ShipInteriorMod2.cs#L2220
     */
    [HarmonyPatch(typeof(MapDrawer), "DrawMapMesh", null)]
    public static class MapDrawer_DrawMapMesh {
        public static void Prefix() {
            if (!Cache.allowed_utility("universum.vacuum_overlay")) return;

            Map map = Find.CurrentMap;

            if (Globals.rendered || !Cache.allowed_utility(map, "universum.vacuum")) return;

            get_world_map_render(map);

            if (!Find.World.renderer.layers.FirstOrFallback().ShouldRegenerate) Globals.rendered = true;
        }

        public static void get_world_map_render(Map map) {
            Camera camera = Find.Camera;
            Camera worldCamera = Find.WorldCamera;
            Camera worldSkyboxCamera = RimWorld.Planet.WorldCameraManager.WorldSkyboxCamera;
            RimWorld.Planet.WorldCameraDriver worldCameraDriver = Find.WorldCameraDriver;
            RimWorld.Planet.World world = Find.World;

            // block celestial object rendering
            Game.MainLoop.instance.blockRendering = true;
            Game.MainLoop.instance.ForceRender();

            RenderTexture oldTexture = worldCamera.targetTexture;
            RenderTexture oldSkyboxTexture = worldSkyboxCamera.targetTexture;

            world.renderer.wantedMode = RimWorld.Planet.WorldRenderMode.Planet;

            ObjectHolder objectHolder = ObjectHolderCache.Get(map.Tile);
            if (objectHolder != null) {
                worldCameraDriver.JumpTo(objectHolder.celestialObject.transformedPosition);
            } else {
                worldCameraDriver.JumpTo(map.Tile);
            }
            worldCameraDriver.altitude = Globals.planet_render_altitude;
            worldCameraDriver.desiredAltitude = Globals.planet_render_altitude;

            worldCameraDriver.Update();
            world.renderer.CheckActivateWorldCamera();
            world.renderer.DrawWorldLayers();
            RimWorld.Planet.WorldRendererUtility.UpdateWorldShadersParams();

            float aspect = (float) UI.screenWidth / UI.screenHeight;

            worldSkyboxCamera.targetTexture = Globals.render;
            worldSkyboxCamera.aspect = aspect;
            worldSkyboxCamera.Render();

            worldCamera.targetTexture = Globals.render;
            worldCamera.aspect = aspect;
            worldCamera.Render();

            RenderTexture.active = Globals.render;
            Globals.planet_screenshot.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
            Globals.planet_screenshot.Apply();
            RenderTexture.active = null;

            worldCamera.targetTexture = oldTexture;
            worldSkyboxCamera.targetTexture = oldSkyboxTexture;
            world.renderer.wantedMode = RimWorld.Planet.WorldRenderMode.None;
            world.renderer.CheckActivateWorldCamera();

            // unblock celestial object rendering
            Game.MainLoop.instance.blockRendering = false;
            Game.MainLoop.instance.ForceRender();
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/ShipInteriorMod2.cs#L2283
     */
    [HarmonyPatch(typeof(SectionLayer), "FinalizeMesh", null)]
    public static class SectionLayer_FinalizeMesh {
        public static bool Prefix(SectionLayer __instance, Section ___section) {
            if (!Cache.allowed_utility("universum.vacuum_overlay")) return true;

            if (__instance.GetType().Name != "SectionLayer_Terrain" || !Cache.allowed_utility(___section.map, "universum.vacuum")) return true;
            
            bool foundSpace = false;
            foreach (IntVec3 cell in ___section.CellRect.Cells) {
                TerrainDef terrain1 = ___section.map.terrainGrid.TerrainAt(cell);
                if (Cache.allowed_utility(terrain1, "universum.vacuum_overlay")) {
                    foundSpace = true;
                    Material mat = Globals.planet_mat;

                    if (terrain1.defName == "RimNauts2_Vacuum_Glass") mat = Globals.planet_mat_glass;

                    Printer_Mesh.PrintMesh(
                        __instance,
                        Matrix4x4.TRS(cell.ToVector3() + new Vector3(0.5f, 0f, 0.5f), Quaternion.identity, Vector3.one),
                        MeshMakerPlanes.NewPlaneMesh(1f),
                        mat
                    );
                }
            }
            if (!foundSpace) {
                for (int i = 0; i < __instance.subMeshes.Count; i++) {
                    if (__instance.subMeshes[i].material == Globals.planet_mat || __instance.subMeshes[i].material == Globals.planet_mat_glass) {
                        __instance.subMeshes.RemoveAt(i);
                    }
                }
            }
            return true;
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L110
     */
    [HarmonyPatch(typeof(RimWorld.MapInterface), "Notify_SwitchedMap")]
    public class MapInterface_Notify_SwitchedMap {
        public static bool MapIsSpace;

        public static void Postfix() {
            Map map = Find.CurrentMap;

            if (map == null || Scribe.mode != LoadSaveMode.Inactive) return;

            MapIsSpace = Cache.allowed_utility(map, "universum.vacuum");
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L126
     */
    [HarmonyPatch(typeof(Verse.Game), "LoadGame")]
    public class Game_LoadGame {
        public static void Postfix() {
            Globals.rendered = false;

            MapInterface_Notify_SwitchedMap.Postfix();
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L153
     */
    [HarmonyPatch(typeof(Verse.Game), "UpdatePlay")]
    public class Game_UpdatePlay {
        public static CameraDriver Driver;
        public static Camera GameCamera;
        public static Vector3 Center;
        public static float CellsHigh;
        public static float CellsWide;
        public static Dictionary<Map, Dictionary<Section, SectionLayer>> MapSections = new Dictionary<Map, Dictionary<Section, SectionLayer>>();
        private static Vector3 lastCameraPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public static void add_section(Map map, Section section, SectionLayer layer) {
            if (!MapSections.TryGetValue(map, out Dictionary<Section, SectionLayer> sections)) {
                sections = new Dictionary<Section, SectionLayer>();
                MapSections.Add(map, sections);
            }
            sections.Add(section, layer);
        }

        public static void Prefix() {
            if (!Cache.allowed_utility("universum.vacuum_overlay")) return;

            if (!MapInterface_Notify_SwitchedMap.MapIsSpace || !MapSections.ContainsKey(Find.CurrentMap)) return;

            Center = GameCamera.transform.position;
            var ratio = (float) UI.screenWidth / UI.screenHeight;
            CellsHigh = UI.screenHeight / Find.CameraDriver.CellSizePixels;
            CellsWide = CellsHigh * ratio;

            if ((lastCameraPosition - Center).magnitude < 1e-4) return;

            lastCameraPosition = Center;
            var sections = MapSections[Find.CurrentMap];
            var visibleRect = Driver.CurrentViewRect;
            foreach (var entry in sections) {
                if (!visibleRect.Overlaps(entry.Key.CellRect)) continue;
                MeshRecalculateHelper.recalculate_layer(entry.Value);
            }
        }

        public static void Postfix() {
            if (!MeshRecalculateHelper.Tasks.Any()) return;

            Task.WaitAll(MeshRecalculateHelper.Tasks.ToArray());
            MeshRecalculateHelper.Tasks.Clear();

            foreach (var layer in MeshRecalculateHelper.LayersToDraw) {
                var mesh = layer.GetSubMesh(Globals.planet_mat);
                var mesh_glass = layer.GetSubMesh(Globals.planet_mat_glass);

                if (!(!mesh.finalized || mesh.disabled)) Graphics.DrawMesh(mesh.mesh, Vector3.zero, Quaternion.identity, mesh.material, 0);
                if (!(!mesh_glass.finalized || mesh_glass.disabled)) Graphics.DrawMesh(mesh_glass.mesh, Vector3.zero, Quaternion.identity, mesh_glass.material, 0);
            }

            MeshRecalculateHelper.LayersToDraw.Clear();
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L138
     */
    [HarmonyPatch(typeof(Verse.Game), "FinalizeInit")]
    public class Game_FinalizeInit {
        public static void Postfix() {
            Game_UpdatePlay.Driver = Find.CameraDriver;
            Game_UpdatePlay.GameCamera = Find.CameraDriver.GetComponent<Camera>();
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L69
     */
    public class MeshRecalculateHelper {
        public static List<Task> Tasks = new List<Task>();
        public static List<SectionLayer> LayersToDraw = new List<SectionLayer>();

        public static void recalculate_layer(SectionLayer instance) {
            var mesh = instance.GetSubMesh(Globals.planet_mat);
            var mesh_glass = instance.GetSubMesh(Globals.planet_mat_glass);

            if (mesh.verts.Count > 0) Tasks.Add(Task.Factory.StartNew(() => recalculate_mesh(mesh)));
            if (mesh_glass.verts.Count > 0) Tasks.Add(Task.Factory.StartNew(() => recalculate_mesh(mesh_glass)));

            LayersToDraw.Add(instance);
        }

        private static void recalculate_mesh(object info) {
            if (!(info is LayerSubMesh mesh)) {
                Logger.print(
                    Logger.Importance.Error,
                    key: "Universum.Error.thread_with_wrong_type",
                    prefix: Style.name_prefix
                );
                return;
            }

            lock (mesh) {
                mesh.finalized = false;
                mesh.Clear(MeshParts.UVs);

                for (var i = 0; i < mesh.verts.Count; i++) {
                    var xdiff = mesh.verts[i].x - Game_UpdatePlay.Center.x;
                    var xfromEdge = xdiff + Game_UpdatePlay.CellsWide / 2.0f;
                    var zdiff = mesh.verts[i].z - Game_UpdatePlay.Center.z;
                    var zfromEdge = zdiff + Game_UpdatePlay.CellsHigh / 2.0f;

                    mesh.uvs.Add(new Vector3(xfromEdge / Game_UpdatePlay.CellsWide, zfromEdge / Game_UpdatePlay.CellsHigh, 0.0f));
                }

                mesh.FinalizeMesh(MeshParts.UVs);
            }
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L27
     */
    [HarmonyPatch(typeof(Section), MethodType.Constructor, typeof(IntVec3), typeof(Map))]
    [StaticConstructorOnStartup]
    public class Section_Constructor {
        private static readonly Type SunShadowsType;
        private static readonly Type TerrainType;

        static Section_Constructor() {
            SunShadowsType = AccessTools.TypeByName("SectionLayer_SunShadows");
            TerrainType = AccessTools.TypeByName("SectionLayer_Terrain");
        }

        public static void Postfix(Map map, Section __instance, List<SectionLayer> ___layers) {
            if (!Cache.allowed_utility("universum.vacuum_overlay")) return;
            if (!Cache.allowed_utility(map, "universum.vacuum")) return;
            // Kill shadows
            if (Cache.allowed_utility(map, "universum.remove_shadows")) ___layers.RemoveAll(layer => SunShadowsType.IsInstanceOfType(layer));
            // Get and store terrain layer for recalculation
            var terrain = ___layers.Find(layer => TerrainType.IsInstanceOfType(layer));
            Game_UpdatePlay.add_section(map, __instance, terrain);
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/ecaf9bba7975524b61bb1d7f1a37655f5be35e20/Source/1.4/HideLightingLayersInSpace.cs#L56
     */
    public class SectionLayer_Terrain_Regenerate {
        public static void Postfix(SectionLayer __instance, Section ___section) {
            if (!Cache.allowed_utility("universum.vacuum_overlay")) return;
            if (!Cache.allowed_utility(___section.map, "universum.vacuum")) return;
            MeshRecalculateHelper.recalculate_layer(__instance);
        }
    }

    /**
     * Source: https://github.com/SonicTHI/SaveOurShip2Experimental/blob/main/Source/1.4/ShipInteriorMod2.cs#L4402
     */
    [HarmonyPatch(typeof(RimWorld.Scenario), "PostWorldGenerate")]
    public class Scenario_PostWorldGenerate {
        public static void Prefix() => Globals.rendered = false;
    }

    [HarmonyPatch(typeof(Verse.Game), "InitNewGame")]
    public class Game_GameInitData {
        public static void Prefix() => Globals.rendered = false;
    }
}
