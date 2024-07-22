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

public static class ExitMapGrid {
    private const string TYPE_NAME = "Verse.ExitMapGrid";
    
    [HarmonyPatch]
    public static class Color {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_Color";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);
        
        public static void Postfix(ref Verse.ExitMapGrid __instance, ref UnityEngine.Color __result) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: __instance.map);
            if (!Cache.Utilities.Manager.VACUUM.maps[mapIndex]) return;
            
            __result.a = 0;
        }
    }
}
