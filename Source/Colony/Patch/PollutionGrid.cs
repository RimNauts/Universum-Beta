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

public static class PollutionGrid {
    public static class SetPolluted {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.PollutionGrid:SetPolluted");


        public static bool Prefix(Verse.IntVec3 cell, Verse.Map ___map) {
            return !Loader.Defs.TerrainProperties[___map.terrainGrid.TerrainAt(cell).index]
                .activeUtilities[Cache.Utilities.Vacuum.id];
        }
    }
}
