using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Cache.Patch;

public static class MemoryUtility {
    private const string TYPE_NAME = "Verse.Profile.MemoryUtility";
    
    [HarmonyPatch]
    private static class ClearAllMapsAndWorld {
        private const string METHOD_NAME = $"{TYPE_NAME}:ClearAllMapsAndWorld";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static void Prefix() {
            if (Game.MainLoop.instance is not null) {
                Game.MainLoop.instance.Destroy();
                Game.MainLoop.instance = null;
            }
            
            ObjectHolder.Clear();
            
            Utilities.OceanMasking.Reset();
            Utilities.RemoveShadows.Reset();
            Utilities.Temperature.Reset();
            Utilities.Vacuum.Reset();
            Utilities.VacuumDamage.Reset();
            Utilities.VacuumOverlay.Reset();
            Utilities.WeatherChanger.Reset();
        }
    }
}
