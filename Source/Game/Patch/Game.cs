using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.Game.Patch;

public class Game {
    [HarmonyPatch]
    static class AddMap {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:AddMap");

        public static void Postfix(Verse.Map map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);

            if (mapIndex == -1) return;

            bool isOuterSpace = Loader.Defs.BiomeProperties[map.Biome.index]
                .activeUtilities[Cache.Utilities.Vacuum.index];
            Cache.Utilities.Vacuum.mapIsOuterSpace[mapIndex] = isOuterSpace;

            if (!isOuterSpace) return;

            Cache.Utilities.Vacuum.tracker.Subscribe();
        }
    }

    [HarmonyPatch]
    static class DeinitAndRemoveMap {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:DeinitAndRemoveMap");

        public static void Prefix(Verse.Map map, bool notifyPlayer) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);

            if (mapIndex == -1) return;

            if (!Cache.Utilities.Vacuum.mapIsOuterSpace[mapIndex]) return;

            Cache.Utilities.Vacuum.mapIsOuterSpace[mapIndex] = false;
            Cache.Utilities.Vacuum.tracker.Unsubscribe();
        }
    }

    [HarmonyPatch]
    static class LoadGame {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:LoadGame");

        public static void Postfix() {
            List<Verse.Map> maps = Verse.Current.gameInt.maps;
            int mapCount = maps.Count;

            for (int mapIndex = 0; mapIndex < mapCount; mapIndex++) {
                bool isOuterSpace = Loader.Defs.BiomeProperties[maps[mapIndex].Biome.index]
                    .activeUtilities[Cache.Utilities.Vacuum.index];
                Cache.Utilities.Vacuum.mapIsOuterSpace[mapIndex] = isOuterSpace;

                if (!isOuterSpace) return;

                Cache.Utilities.Vacuum.tracker.Subscribe();
            }
        }
    }
}
