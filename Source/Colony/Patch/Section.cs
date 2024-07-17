using System;
using System.Collections.Generic;
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

[Verse.StaticConstructorOnStartup]
public static class Section {
    private const string TYPE_NAME = "Verse.Section";
    private static readonly Type SUN_SHADOWS_TYPE = typeof(Verse.SectionLayer_SunShadows);
    private static readonly Type TERRAIN_TYPE = typeof(Verse.SectionLayer_Terrain);
    
    public static class FinalizeMesh {
        private const string METHOD_NAME = $"{TYPE_NAME}:..ctor";
        private static bool _verboseError = true;

        public static bool Prepare() {
            if (TargetMethod() != null) return true;

            if (!_verboseError) return false;
            
            Debugger.Log(
                key: "Universum.Error.FailedToPatch",
                prefix: $"{Mod.Manager.METADATA.NAME}: ",
                args: [METHOD_NAME],
                severity: Debugger.Severity.Error
            );
            _verboseError = false;

            return false;
        }

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);


        public static void Postfix(Verse.Map map, Verse.Section __instance, List<Verse.SectionLayer> ___layers) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            
            if (!Cache.Utilities.Vacuum.maps[mapIndex]) return;
            
            if (Cache.Utilities.RemoveShadows.tracker.active && Cache.Utilities.RemoveShadows.maps[mapIndex]) {
                ___layers.RemoveAll(layer => SUN_SHADOWS_TYPE.IsInstanceOfType(layer));
            }
            
            var terrain = ___layers.Find(layer => TERRAIN_TYPE.IsInstanceOfType(layer));
            Game.Patch.Game.UpdatePlay.AddSection(map, __instance, terrain);
        }
    }
}
