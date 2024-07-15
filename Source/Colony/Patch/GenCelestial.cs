using System.Linq;
using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedParameter.Local

namespace Universum.Colony.Patch;

public static class GenCelestial {
    public static class CelestialSunGlow {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() {
            var type = AccessTools.TypeByName("RimWorld.GenCelestial:CelestialSunGlow");
            if (type == null) return null;

            var method = AccessTools.Method(
                type,
                "CelestialSunGlow",
                [typeof(int), typeof(int)]
            );
            
            return method == null ? null : method;
        }

        public static bool Prefix(int tile, int ticksAbs, ref float __result) {
            if (tile == -1) return true;
            
            int biomeIndex = Verse.Find.World.grid.tiles.ElementAt(tile).biome.index;
            bool[] activeUtilities = Loader.Defs.BiomeProperties[biomeIndex].activeUtilities;

            if (!activeUtilities[Cache.Utilities.Vacuum.id] || !activeUtilities[Cache.Utilities.RemoveShadows.id]) {
                return true;
            }

            __result = 1.0f;
            return false;
        }
    }
}
