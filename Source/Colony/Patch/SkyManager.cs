using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class SkyManager {
    private static readonly Vector2 DEFAULT_SHADOW_VECTOR = new(0f, 1f);
    private static readonly Color32 FOG_OF_WAR_BASE_COLOR = new(77, 69, 66, byte.MaxValue);

    public static class SkyManagerUpdate {
        
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.SkyManager:SkyManagerUpdate");


        public static bool Prefix(ref Verse.SkyManager __instance) {
            Verse.Map map = __instance.map;
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex] || !Cache.Utilities.RemoveShadows.maps[mapIndex]) return true;
            
            Verse.SkyTarget curSky = __instance.CurrentSkyTarget();
            __instance.curSkyGlowInt = curSky.glow;

            if (map != Verse.Find.CurrentMap) return false;
            
            Verse.MatBases.LightOverlay.color = curSky.colors.sky;
            Verse.Find.CameraColor.saturation = curSky.colors.saturation;
            Color sky = curSky.colors.sky;
            sky.a = 1f;
            sky *= FOG_OF_WAR_BASE_COLOR;
            Verse.MatBases.FogOfWar.color = sky;
            Color color = curSky.colors.shadow;
            Vector3? overridenShadowVector = __instance.GetOverridenShadowVector();
            
            if (overridenShadowVector.HasValue) {
                __instance.SetSunShadowVector(overridenShadowVector.Value);
            } else {
                __instance.SetSunShadowVector(DEFAULT_SHADOW_VECTOR);
                color = Color.black;
            }
            
            Verse.MatBases.SunShadow.color = color;
            Verse.MatBases.SunShadowFade.color = color;
            __instance.UpdateOverlays(curSky);

            return false;
        }
    }
}
