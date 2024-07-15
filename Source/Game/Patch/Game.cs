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

public static class Game {
    [HarmonyPatch]
    static class AddMap {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:AddMap");

        public static void Postfix(Verse.Map map) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            if (mapIndex == -1) return;

            bool removeShadows = Loader.Defs.BiomeProperties[map.Biome.index]
                .activeUtilities[Cache.Utilities.RemoveShadows.id];
            Cache.Utilities.Temperature.maps[mapIndex] = removeShadows;
            if (removeShadows) Cache.Utilities.RemoveShadows.tracker.Subscribe();

            bool hasCustomTemperature = Loader.Defs.BiomeProperties[map.Biome.index]
                .activeUtilities[Cache.Utilities.Temperature.id];
            Cache.Utilities.Temperature.maps[mapIndex] = hasCustomTemperature;
            if (hasCustomTemperature) Cache.Utilities.Temperature.tracker.Subscribe();

            bool isOuterSpace = Loader.Defs.BiomeProperties[map.Biome.index]
                .activeUtilities[Cache.Utilities.Vacuum.id];
            Cache.Utilities.Vacuum.maps[mapIndex] = isOuterSpace;
            if (isOuterSpace) Cache.Utilities.Vacuum.tracker.Subscribe();

            bool weatherNotChangeable = Loader.Defs.BiomeProperties[map.Biome.index]
                .activeUtilities[Cache.Utilities.WeatherChanger.id];
            Cache.Utilities.Vacuum.maps[mapIndex] = weatherNotChangeable;
            if (weatherNotChangeable) Cache.Utilities.WeatherChanger.tracker.Subscribe();
        }
    }

    [HarmonyPatch]
    static class DeinitAndRemoveMap {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:DeinitAndRemoveMap");

        public static void Prefix(Verse.Map map, bool notifyPlayer) {
            int mapIndex = Verse.Current.gameInt.maps.IndexOf(item: map);
            if (mapIndex == -1) return;

            if (Cache.Utilities.RemoveShadows.maps[mapIndex]) {
                Cache.Utilities.RemoveShadows.maps[mapIndex] = false;
                Cache.Utilities.RemoveShadows.tracker.Unsubscribe();
            }

            if (Cache.Utilities.Temperature.maps[mapIndex]) {
                Cache.Utilities.Temperature.maps[mapIndex] = false;
                Cache.Utilities.Temperature.tracker.Unsubscribe();
            }

            if (Cache.Utilities.Vacuum.maps[mapIndex]) {
                Cache.Utilities.Vacuum.maps[mapIndex] = false;
                Cache.Utilities.Vacuum.tracker.Unsubscribe();
            }

            if (Cache.Utilities.WeatherChanger.maps[mapIndex]) {
                Cache.Utilities.WeatherChanger.maps[mapIndex] = false;
                Cache.Utilities.WeatherChanger.tracker.Unsubscribe();
            }
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
                bool removeShadows = Loader.Defs.BiomeProperties[maps[mapIndex].Biome.index]
                    .activeUtilities[Cache.Utilities.RemoveShadows.id];
                Cache.Utilities.RemoveShadows.maps[mapIndex] = removeShadows;
                if (removeShadows) Cache.Utilities.RemoveShadows.tracker.Subscribe();
                
                bool hasCustomTemperature = Loader.Defs.BiomeProperties[maps[mapIndex].Biome.index]
                    .activeUtilities[Cache.Utilities.Temperature.id];
                Cache.Utilities.Temperature.maps[mapIndex] = hasCustomTemperature;
                if (hasCustomTemperature) Cache.Utilities.Temperature.tracker.Subscribe();
                
                bool isOuterSpace = Loader.Defs.BiomeProperties[maps[mapIndex].Biome.index]
                    .activeUtilities[Cache.Utilities.Vacuum.id];
                Cache.Utilities.Vacuum.maps[mapIndex] = isOuterSpace;
                if (isOuterSpace) Cache.Utilities.Vacuum.tracker.Subscribe();

                bool weatherNotChangeable = Loader.Defs.BiomeProperties[maps[mapIndex].Biome.index]
                    .activeUtilities[Cache.Utilities.WeatherChanger.id];
                Cache.Utilities.Vacuum.maps[mapIndex] = weatherNotChangeable;
                if (weatherNotChangeable) Cache.Utilities.WeatherChanger.tracker.Subscribe();
            }
        }
    }
}
