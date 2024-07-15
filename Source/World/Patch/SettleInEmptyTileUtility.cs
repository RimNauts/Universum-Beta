using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedType.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace Universum.World.Patch;

public static class SettleInEmptyTileUtility {
    private static readonly Dictionary<Defs.CelestialObject, Verse.Command> COMMANDS = new();
    
    [HarmonyPatch]
    static class Settle {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.SettleInEmptyTileUtility:Settle");

        public static bool Prefix(RimWorld.Planet.Caravan caravan) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(caravan.Tile);
            if (objectHolder == null) return true;

            objectHolder.Settle(caravan);

            return false;
        }
    }

    [HarmonyPatch]
    static class SettleCommand {
        public static bool Prepare() => TargetMethod() != null;

        private static MethodBase TargetMethod() => AccessTools.Method("RimWorld.Planet.SettleInEmptyTileUtility:SettleCommand");

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
