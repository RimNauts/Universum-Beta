using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class WorldRendererUtility {
    private const string TYPE_NAME = "RimWorld.Planet.WorldRendererUtility";
    
    [HarmonyPatch]
    private static class HiddenBehindTerrainNow {
        private const string METHOD_NAME = $"{TYPE_NAME}:HiddenBehindTerrainNow";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(Vector3 pos, ref bool __result) {
            // ignore icons on surface (settlements)
            if (Vector3.Distance(pos, Game.MainLoop.instance.currentSphereFocusPoint) < 110.0f) return true;

            __result = ShouldHide(pos);
            return false;
        }
    }
    
    public static bool ShouldHideObjectHolder(Vector3 pos) {
        float distanceWithFocusPoint = Vector3.Distance(pos, Game.MainLoop.instance.currentSphereFocusPoint) + 2.3f;
        float cameraDistanceWithFocusPoint = Vector3.Distance(Game.MainLoop.instance.currentSphereFocusPoint, Game.MainLoop.instance.cameraPosition);

        return distanceWithFocusPoint > cameraDistanceWithFocusPoint || ShouldHide(pos);
    }

    private static bool ShouldHide(Vector3 pos) {
        float altitudePercent = Game.MainLoop.instance.altitudePercent;
        float degree = _CalculateDegreeBasedOnAltitude(altitudePercent);

        float minAlt = RimWorld.Planet.WorldCameraDriver.MinAltitude;
        float maxAlt = WorldCameraDriver.MAX_ALTITUDE;
        float alt = altitudePercent * (maxAlt - minAlt) + maxAlt;

        Vector3 normalized = pos.normalized;
        float mag = pos.magnitude;
        float angleWithFocus = Vector3.Angle(normalized, Game.MainLoop.instance.currentSphereFocusPoint);
        bool hideBasedOnAltitude = angleWithFocus > (Math.Acos(115.0f / alt) + Math.Acos(115.0f / mag)) * (degree / 3.14d);

        if (mag < 115.0f) return angleWithFocus > 73.0f;
        return hideBasedOnAltitude;
    }

    private static float _CalculateDegreeBasedOnAltitude(float altitudePercent) {
        float baseDegree = 165.0f + 15.0f * altitudePercent * 1.5f;
        if (baseDegree > 180.0f) {
            return 180.0f;
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
