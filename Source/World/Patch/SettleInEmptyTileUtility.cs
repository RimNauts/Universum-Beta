using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedType.Global

namespace Universum.World.Patch;

public static class SettleInEmptyTileUtility {
    private const string TYPE_NAME = "RimWorld.Planet.SettleInEmptyTileUtility";
    private static readonly Dictionary<Defs.CelestialObject, Verse.Command> COMMANDS = new();
    
    [HarmonyPatch]
    private static class Settle {
        private const string METHOD_NAME = $"{TYPE_NAME}:Settle";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.Caravan caravan) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(caravan.Tile);
            if (objectHolder == null) return true;

            objectHolder.Settle(caravan);

            return false;
        }
    }

    [HarmonyPatch]
    private static class SettleCommand {
        private const string METHOD_NAME = $"{TYPE_NAME}:SettleCommand";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(RimWorld.Planet.Caravan caravan, ref Verse.Command __result) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(caravan.Tile);
            if (objectHolder == null) return true;

            __result = GetCommand(objectHolder, caravan);

            return false;
        }
    }

    private static Verse.Command GetCommand(ObjectHolder objectHolder, RimWorld.Planet.Caravan caravan) {
        Defs.CelestialObject celestialObjectDef = objectHolder.celestialObjectDef;

        if (COMMANDS.TryGetValue(celestialObjectDef, out var command)) return command;

        Verse.Command_Settle newCommand = new Verse.Command_Settle {
            defaultLabel = Verse.TranslatorFormattedStringExtensions.Translate(celestialObjectDef.objectHolder.commandLabelKey),
            defaultDesc = Verse.TranslatorFormattedStringExtensions.Translate(celestialObjectDef.objectHolder.commandDescKey),
            icon = Loader.Assets.GetTexture(celestialObjectDef.objectHolder.commandIconPath),
            action = () => RimWorld.Planet.SettleInEmptyTileUtility.Settle(caravan)
        };

        COMMANDS[celestialObjectDef] = newCommand;
        return newCommand;
    }
}
