using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Cache.Patch;

public class MemoryUtility {
    [HarmonyPatch]
    static class ClearAllMapsAndWorld {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("Verse.Profile.MemoryUtility:ClearAllMapsAndWorld");

        public static void Prefix() {
            if (Game.MainLoop.instance is not null) {
                Game.MainLoop.instance.Destroy();
                Game.MainLoop.instance = null;
            }
            
            ObjectHolder.Clear();
            
            Utilities.Vacuum.Reset();
        }
    }
}
