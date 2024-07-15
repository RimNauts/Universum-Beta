using System.Collections.Generic;
using System.Linq;

namespace Universum.Cache.Utilities;

public static class VacuumDamage {
    public static int decompressionId;
    public static int suffocationId;
    private const string TRADE_TAG = "AnimalInsectSpace";
    private const string EVA_TAG = "EVA";

    public static readonly Dictionary<int, Colony.VacuumWeather.VacuumProtection> PAWN_PROTECTION = new();

    public static void Init() {
        Loader.Defs.UtilityId.TryGetValue(key: "universum.vacuum_decompression", out decompressionId);
        Loader.Defs.UtilityId.TryGetValue(key: "universum.vacuum_suffocation", out suffocationId);
    }

    public static void Reset() {
        PAWN_PROTECTION.Clear();
    }
    
    public static Colony.VacuumWeather.VacuumProtection CheckProtection(Verse.Pawn pawn) {
        if (pawn == null) return Colony.VacuumWeather.VacuumProtection.None;
        
        // branch if pawn is cached
        if (PAWN_PROTECTION.TryGetValue(pawn.thingIDNumber, out var protection)) return protection;
        
        // get value and cache result
        Colony.VacuumWeather.VacuumProtection value = Colony.VacuumWeather.VacuumProtection.None;
        if (pawn.RaceProps.IsMechanoid || !pawn.RaceProps.IsFlesh || (pawn.def.tradeTags?.Contains(TRADE_TAG) ?? false)) {
            value = Colony.VacuumWeather.VacuumProtection.All;
        } else if (pawn.apparel == null) {
            value = Colony.VacuumWeather.VacuumProtection.None;
        } else {
            bool helmet = false;
            bool suit = false;
            
            // check genes
            if (Verse.ModsConfig.BiotechActive) {
                List<Verse.Gene> genes = pawn.genes.GenesListForReading;
                foreach (var activeUtilities in genes.Select(gene => Loader.Defs.GeneProperties[gene.def.index].activeUtilities)) {
                    if (Settings.UtilityEnabled(decompressionId) && activeUtilities[decompressionId]) helmet = true;
                    if (Settings.UtilityEnabled(suffocationId) && activeUtilities[suffocationId]) suit = true;
                    if (helmet && suit) break;
                }
            }
            
            // check apparel
            List<RimWorld.Apparel> apparels = pawn.apparel.WornApparel;
            foreach (RimWorld.Apparel apparel in apparels) {
                if (apparel.def.apparel.tags.Contains(EVA_TAG)) {
                    if (apparel.def.apparel.layers.Contains(RimWorld.ApparelLayerDefOf.Overhead)) helmet = true;
                    if (apparel.def.apparel.layers.Contains(RimWorld.ApparelLayerDefOf.Shell) || apparel.def.apparel.layers.Contains(RimWorld.ApparelLayerDefOf.Middle)) suit = true;
                }
                if (helmet && suit) break;
            }

            value = helmet switch {
                true when suit => Colony.VacuumWeather.VacuumProtection.All,
                true => Colony.VacuumWeather.VacuumProtection.Oxygen,
                _ => value
            };
        }
        
        PAWN_PROTECTION.Add(pawn.thingIDNumber, value);
        return value;
    }
}
