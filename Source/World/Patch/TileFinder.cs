using System.Reflection;
using System.Text;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

namespace Universum.World.Patch;

public static class TileFinder {
    private const string TYPE_NAME = "RimWorld.Planet.TileFinder";
    
    [HarmonyPatch]
    static class IsValidTileForNewSettlement {
        private const string METHOD_NAME = $"{TYPE_NAME}:IsValidTileForNewSettlement";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Postfix(int tile, StringBuilder reason, ref bool __result) {
            if (tile == -1 || __result || reason == null || !Cache.ObjectHolder.Exists(tile)) return;

            if (Verse.Find.WorldObjects.SettlementBaseAt(tile) != null) return;
            if (!reason.ToString()
                    .Contains(Verse.TranslatorFormattedStringExtensions.Translate("TileOccupied"))) return;

            __result = true;
        }
    }
}
