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

public static class PollutionGrid {
    private const string TYPE_NAME = "Verse.PollutionGrid";
    
    [HarmonyPatch]
    public static class SetPolluted {
        private const string METHOD_NAME = $"{TYPE_NAME}:SetPolluted";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(Verse.IntVec3 cell, bool isPolluted, bool silent, Verse.PollutionGrid __instance) {
            int terrainIndex = __instance.map.terrainGrid.TerrainAt(cell).index;
            return !Cache.Utilities.Manager.VACUUM.CheckTerrain(terrainIndex);
        }
    }
}
