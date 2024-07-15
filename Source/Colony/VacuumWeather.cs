using System;
using System.Collections.Generic;
using System.Linq;

namespace Universum.Colony;

public class VacuumWeather(Verse.Map map) : Verse.WeatherEvent(map) {
    public enum VacuumProtection {
        None = 0,
        Oxygen = 1,
        Decompression = 2,
        All = 3,
    }

    public override void FireEvent() {
        bool decompressionEnabled =
            Cache.Settings.UtilityEnabled(Cache.Utilities.VacuumDamage.decompressionId) &&
            Loader.Defs.BiomeProperties[map.Biome.index].activeUtilities[Cache.Utilities.VacuumDamage.decompressionId];
        bool suffocationEnabled =
            Cache.Settings.UtilityEnabled(Cache.Utilities.VacuumDamage.suffocationId) &&
            Loader.Defs.BiomeProperties[map.Biome.index].activeUtilities[Cache.Utilities.VacuumDamage.suffocationId];
        if (!decompressionEnabled && !suffocationEnabled) return;
        
        IReadOnlyList<Verse.Pawn> pawns = map.mapPawns.AllPawnsSpawned;
        List<Verse.Pawn> pawnsToSuffocate = [];
        List<Verse.Pawn> pawnsToDecompress = [];
        
        foreach (Verse.Pawn pawn in pawns.Where(p => !p.Dead)) {
            Verse.Room room = Verse.GridsUtility.GetRoom(pawn.Position, map);
            bool vacuum = room == null || room.OpenRoofCount > 0 || room.TouchesMapEdge;
            if (vacuum) {
                VacuumProtection protection = Cache.Utilities.VacuumDamage.CheckProtection(pawn);
                switch (protection) {
                    case VacuumProtection.None:
                        if (decompressionEnabled) pawnsToDecompress.Add(pawn);
                        if (suffocationEnabled) pawnsToSuffocate.Add(pawn);
                        break;
                    case VacuumProtection.Oxygen:
                        if (decompressionEnabled) pawnsToDecompress.Add(pawn);
                        break;
                    case VacuumProtection.Decompression:
                        if (suffocationEnabled) pawnsToSuffocate.Add(pawn);
                        break;
                    case VacuumProtection.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        foreach (Verse.Pawn pawn in pawnsToDecompress) {
            pawn.TakeDamage(new Verse.DamageInfo(Loader.Defs.DecompressionDamageDef, 1.0f));
        }
        foreach (Verse.Pawn pawn in pawnsToSuffocate) {
            Verse.HealthUtility.AdjustSeverity(pawn, Loader.Defs.SuffocationHediffDef, 0.05f);
        }
    }

    public override void WeatherEventTick() { }

    public override bool Expired => true;
}
