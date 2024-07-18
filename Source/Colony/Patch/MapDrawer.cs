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
public static class MapDrawer {
    private const string TYPE_NAME = "Verse.MapDrawer";
    public static bool rendered;
    public static float planetRenderAltitude = 1100.0f;
    private static readonly RenderTexture PLANET_RENDER = new(2048, 2048, 16);
    public static readonly Texture2D PLANET_SCREENSHOT = new(2048, 2048, TextureFormat.RGB24, false);
    
    [HarmonyPatch]
    public static class DrawMapMesh {
        private const string METHOD_NAME = $"{TYPE_NAME}:DrawMapMesh";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Prefix() {
            if (rendered) return;

            Verse.Map map = Verse.Find.CurrentMap;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex]) return;
            
            // block celestial object rendering
            Game.MainLoop.instance.blockRendering = true;
            Game.MainLoop.instance.ForceRender();

            RenderTexture oldTexture = Game.MainLoop.worldCamera.targetTexture;
            RenderTexture oldSkyboxTexture = Game.MainLoop.worldSkyboxCamera.targetTexture;

            Game.MainLoop.world.renderer.wantedMode = RimWorld.Planet.WorldRenderMode.Planet;

            World.ObjectHolder objectHolder = Cache.ObjectHolder.Get(map.Tile);
            if (objectHolder != null) {
                Game.MainLoop.worldCameraDriver.JumpTo(objectHolder.celestialObject.transformedPosition);
            } else {
                Game.MainLoop.worldCameraDriver.JumpTo(map.Tile);
            }
            Game.MainLoop.worldCameraDriver.altitude = planetRenderAltitude;
            Game.MainLoop.worldCameraDriver.desiredAltitude = planetRenderAltitude;

            Game.MainLoop.worldCameraDriver.Update();
            Game.MainLoop.world.renderer.CheckActivateWorldCamera();
            Game.MainLoop.world.renderer.DrawWorldLayers();
            RimWorld.Planet.WorldRendererUtility.UpdateWorldShadersParams();

            float aspect = (float) Verse.UI.screenWidth / Verse.UI.screenHeight;

            Game.MainLoop.worldSkyboxCamera.targetTexture = PLANET_RENDER;
            Game.MainLoop.worldSkyboxCamera.aspect = aspect;
            Game.MainLoop.worldSkyboxCamera.Render();

            Game.MainLoop.worldCamera.targetTexture = PLANET_RENDER;
            Game.MainLoop.worldCamera.aspect = aspect;
            Game.MainLoop.worldCamera.Render();

            RenderTexture.active = PLANET_RENDER;
            PLANET_SCREENSHOT.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
            PLANET_SCREENSHOT.Apply();
            RenderTexture.active = null;

            Game.MainLoop.worldCamera.targetTexture = oldTexture;
            Game.MainLoop.worldSkyboxCamera.targetTexture = oldSkyboxTexture;
            Game.MainLoop.world.renderer.wantedMode = RimWorld.Planet.WorldRenderMode.None;
            Game.MainLoop.world.renderer.CheckActivateWorldCamera();

            // unblock celestial object rendering
            Game.MainLoop.instance.blockRendering = false;
            Game.MainLoop.instance.ForceRender();

            rendered = true;
        }
    }
}
