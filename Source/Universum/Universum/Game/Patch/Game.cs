using HarmonyLib;
using System.Reflection;
using Verse;

namespace Universum.Game.Patch {
    public class Game {
        public static void Init(Harmony harmony) {
            _ = new PatchClassProcessor(instance: harmony, type: typeof(Game_ClearCaches)).Patch();
            _ = new PatchClassProcessor(instance: harmony, type: typeof(Game_AddMap)).Patch();
            _ = new PatchClassProcessor(instance: harmony, type: typeof(Game_DeinitAndRemoveMap)).Patch();
            _ = new PatchClassProcessor(instance: harmony, type: typeof(Game_LoadGame)).Patch();
        }
    }

    [HarmonyPatch]
    static class Game_ClearCaches {
        public static bool Prepare() => TargetMethod() != null;

        public static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:ClearCaches");

        public static void Postfix() {
            Utilities.CachingHandler.ResetCache();
            Utilities.Cache.clear();
        }
    }

    [HarmonyPatch]
    static class Game_AddMap {
        public static bool Prepare() => TargetMethod() != null;

        public static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:AddMap");

        public static void Postfix(Map map) {
            int mapIndex = Current.gameInt.maps.IndexOf(item: map);

            if (mapIndex == -1) return;

            bool isOuterSpace = Utilities.Cache.allowed_utility(map, utility: "universum.vacuum");
            Utilities.CachingHandler.mapIsOuterSpace[mapIndex] = isOuterSpace;

            if (!isOuterSpace) return;

            Utilities.CachingHandler.vacuum.Subscribe();
        }
    }

    [HarmonyPatch]
    static class Game_DeinitAndRemoveMap {
        public static bool Prepare() => TargetMethod() != null;

        public static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:DeinitAndRemoveMap");

        public static void Prefix(Map map, bool notifyPlayer) {
            int mapIndex = Current.gameInt.maps.IndexOf(item: map);

            if (mapIndex == -1) return;

            if (!Utilities.CachingHandler.mapIsOuterSpace[mapIndex]) return;

            Utilities.CachingHandler.mapIsOuterSpace[mapIndex] = false;
            Utilities.CachingHandler.vacuum.Unsubscribe();
        }
    }

    [HarmonyPatch]
    static class Game_LoadGame {
        public static bool Prepare() => TargetMethod() != null;

        public static MethodBase TargetMethod() => AccessTools.Method(typeColonName: "Verse.Game:LoadGame");

        public static void Postfix() {
            int mapCount = Current.gameInt.maps.Count;

            for (int mapIndex = 0; mapIndex < mapCount; mapIndex++) {
                bool isOuterSpace = Utilities.Cache.allowed_utility(map: Current.gameInt.maps[mapIndex], utility: "universum.vacuum");
                Utilities.CachingHandler.mapIsOuterSpace[mapIndex] = isOuterSpace;

                if (!isOuterSpace) return;

                Utilities.CachingHandler.vacuum.Subscribe();
            }
        }
    }
}
