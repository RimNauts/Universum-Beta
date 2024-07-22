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

public static class Room {
    private const string TYPE_NAME = "Verse.Room";

    [HarmonyPatch]
    public static class NotifyTerrainChanged {
        private const string METHOD_NAME = $"{TYPE_NAME}:Notify_TerrainChanged";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Postfix(ref Verse.Room __instance) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.Map);
            
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex]) return;
            
            __instance.Notify_RoofChanged();
        }
    }
}
