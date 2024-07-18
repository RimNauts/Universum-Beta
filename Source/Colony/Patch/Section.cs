using System;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.Colony.Patch;

[Verse.StaticConstructorOnStartup]
public static class Section {
    private const string TYPE_NAME = "Verse.Section";
    private static readonly Type SUN_SHADOWS_TYPE = typeof(Verse.SectionLayer_SunShadows);
    private static readonly Type TERRAIN_TYPE = typeof(Verse.SectionLayer_Terrain);
    
    [HarmonyPatch]
    public static class Constructor {
        private const string METHOD_NAME = $"{TYPE_NAME}::.ctor";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static ConstructorInfo TargetMethod() {
            return AccessTools.Constructor(typeof(Verse.Section), [typeof(Verse.IntVec3), typeof(Verse.Map)]);
        }

        public static void Postfix(Verse.IntVec3 sectCoords, Verse.Map map, ref Verse.Section __instance) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            if (Cache.Utilities.RemoveShadows.tracker.active && Cache.Utilities.RemoveShadows.maps[mapIndex]) {
                __instance.layers.RemoveAll(layer => SUN_SHADOWS_TYPE.IsInstanceOfType(layer));
            }
            
            var terrain = __instance.layers.Find(layer => TERRAIN_TYPE.IsInstanceOfType(layer));
            Game.Patch.Game.UpdatePlay.AddSection(map, __instance, terrain);
        }
    }
}
