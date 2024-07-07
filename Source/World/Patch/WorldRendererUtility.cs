using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public class WorldRendererUtility {
    [HarmonyPatch]
    static class HiddenBehindTerrainNow {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.WorldRendererUtility:HiddenBehindTerrainNow");

        public static bool Prefix(Vector3 pos, ref bool __result) {
            // ignore icons on surface (settlements)
            if (Vector3.Distance(pos, Game.MainLoop.instance.currentSphereFocusPoint) < 110) return true;

            __result = ShouldHide(pos);
            return false;
        }
    }
    
    public static bool ShouldHideObjectHolder(Vector3 pos) {
        float distanceWithFocusPoint = Vector3.Distance(pos, Game.MainLoop.instance.currentSphereFocusPoint) + 2.3f;
        float cameraDistanceWithFocusPoint = Vector3.Distance(Game.MainLoop.instance.currentSphereFocusPoint, Game.MainLoop.instance.cameraPosition);

        return distanceWithFocusPoint > cameraDistanceWithFocusPoint || ShouldHide(pos);
    }

    public static bool ShouldHide(Vector3 pos) {
        float altitudePercent = Game.MainLoop.instance.altitudePercent;
        float degree = _CalculateDegreeBasedOnAltitude(altitudePercent);

        float minAlt = RimWorld.Planet.WorldCameraDriver.MinAltitude;
        float maxAlt = WorldCameraDriver.MAX_ALTITUDE;
        float alt = altitudePercent * (maxAlt - minAlt) + maxAlt;

        Vector3 normalized = pos.normalized;
        float mag = pos.magnitude;
        float angleWithFocus = Vector3.Angle(normalized, Game.MainLoop.instance.currentSphereFocusPoint);
        bool hideBasedOnAltitude = angleWithFocus > (Math.Acos(115 / alt) + Math.Acos(115 / mag)) * (degree / 3.14d);

        if (mag < 115) return angleWithFocus > 73.0f;
        return hideBasedOnAltitude;
    }

    private static float _CalculateDegreeBasedOnAltitude(float altitudePercent) {
        float baseDegree = 165 + (15 * altitudePercent * 1.5f);
        if (baseDegree > 180) {
            return 180;
        }

        var altitudeRanges = new List<(float Min, float Max, float Value)> {
            (0.87f, 0.93f, 179.5f),
            (0.63f, 0.87f, 179f),
            (0.53f, 0.63f, 178f),
            (0.49f, 0.53f, 177f),
            (0.46f, 0.49f, 176f),
            (0.37f, 0.46f, 175f),
            (0.35f, 0.37f, 174f),
            (0.30f, 0.35f, 173f)
        };

        foreach (var (min, max, value) in altitudeRanges) {
            if (altitudePercent > min && altitudePercent < max) return value;
        }

        return altitudePercent < 0.24f ? 160 : baseDegree;
    }
}
