﻿using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace Universum.Utilities {
    [HarmonyPatch(typeof(MapTemperature), "OutdoorTemp", MethodType.Getter)]
    public static class MapTemperature_OutdoorTemp {
        public static void Postfix(ref float __result, Map ___map) {
            if (!Cache.allowed_utility(___map, "universum.temperature")) return;
            __result = Cache.temperature(___map);
        }
    }

    [HarmonyPatch(typeof(MapTemperature), "SeasonalTemp", MethodType.Getter)]
    public static class MapTemperature_SeasonalTemp {
        public static void Postfix(ref float __result, Map ___map) {
            if (!Cache.allowed_utility(___map, "universum.temperature")) return;
            __result = Cache.temperature(___map);
        }
    }

    [HarmonyPatch(typeof(RoomTempTracker), "WallEqualizationTempChangePerInterval")]
    public static class RoomTempTracker_WallEqualizationTempChangePerInterval {
        public static void Postfix(ref RoomTempTracker __instance, ref float __result) {
            if (!Cache.allowed_utility(__instance.Map, "universum.vacuum")) return;
            if (!Cache.allowed_utility(__instance.Map, "universum.temperature")) return;
            __result *= 0.01f;
        }
    }

    [HarmonyPatch(typeof(RoomTempTracker), "ThinRoofEqualizationTempChangePerInterval")]
    public static class RoomTempTracker_ThinRoofEqualizationTempChangePerInterval {
        public static void Postfix(ref RoomTempTracker __instance, ref float __result) {
            if (!Cache.allowed_utility(__instance.Map, "universum.vacuum")) return;
            if (!Cache.allowed_utility(__instance.Map, "universum.temperature")) return;
            __result *= 0.01f;
        }
    }

    [HarmonyPatch(typeof(RoomTempTracker), "EqualizeTemperature")]
    public static class RoomTempTracker_EqualizeTemperature {
        public static void Postfix(RoomTempTracker __instance) {
            if (!Cache.allowed_utility(__instance.Map, "universum.vacuum")) return;
            if (!Cache.allowed_utility(__instance.Map, "universum.temperature")) return;
            if (__instance.room.OpenRoofCount <= 0) return;
            __instance.Temperature = Cache.temperature(__instance.Map);
        }
    }

    [HarmonyPatch(typeof(District), "OpenRoofCountStopAt")]
    public static class District_OpenRoofCountStopAt {
        public static void Postfix(int threshold, ref int __result, District __instance) {
            if (!Cache.allowed_utility(__instance.Map, "universum.vacuum")) return;
            IEnumerator<IntVec3> cells = __instance.Cells.GetEnumerator();
            if (__result < threshold && cells != null) {
                TerrainGrid terrainGrid = __instance.Map.terrainGrid;
                while (__result < threshold && cells.MoveNext()) {
                    if (Cache.allowed_utility(terrainGrid.TerrainAt(cells.Current), "universum.vacuum")) __result++;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Room), "Notify_TerrainChanged")]
    public static class Room_Notify_TerrainChanged {
        public static void Postfix(Room __instance) {
            if (!Cache.allowed_utility(__instance.Map, "universum.vacuum")) return;
            __instance.Notify_RoofChanged();
        }
    }

    [HarmonyPatch(typeof(RimWorld.GlobalControls), "TemperatureString")]
    public static class GlobalControls_TemperatureString {
        public static void Postfix(ref string __result) {
            if (!Cache.allowed_utility(Find.CurrentMap, "universum.vacuum")) return;
            if (__result.Contains("Indoors".Translate())) {
                __result = __result.Replace("Indoors".Translate(), "Universum.indoors".Translate());
            } else if (__result.Contains("IndoorsUnroofed".Translate())) {
                __result = __result.Replace("IndoorsUnroofed".Translate(), "Universum.unroofed".Translate());
            } else if (__result.Contains("Outdoors".Translate().CapitalizeFirst())) {
                __result = __result.Replace("Outdoors".Translate().CapitalizeFirst(), "Universum.outdoors".Translate());
            }
        }
    }
}
