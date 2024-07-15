using System.Reflection;
using System.Text;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public static class TileFinder {
    [HarmonyPatch]
    static class IsValidTileForNewSettlement {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() =>
            AccessTools.Method("RimWorld.Planet.TileFinder:IsValidTileForNewSettlement");

        public static void Postfix(int tile, StringBuilder reason, ref bool __result) {
            if (tile == -1 || __result || reason == null || !Cache.ObjectHolder.Exists(tile)) return;

            if (Verse.Find.WorldObjects.SettlementBaseAt(tile) != null) return;
            if (!reason.ToString()
                    .Contains(Verse.TranslatorFormattedStringExtensions.Translate("TileOccupied"))) return;

            __result = true;
        }
    }
}
