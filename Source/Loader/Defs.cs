using System;
using System.Collections.Generic;

namespace Universum.Loader;

[Verse.StaticConstructorOnStartup]
public static class Defs {
    public static RimWorld.WorldObjectDef ObjectHolderDef { get; private set; }
    public static RimWorld.BiomeDef OceanBiomeDef { get; private set; }
    public static Verse.DamageDef DecompressionDamageDef { get; private set; }
    public static Verse.HediffDef SuffocationHediffDef { get; private set; }
    public static Dictionary<string, Universum.Defs.Utility> Utilities { get; private set; }
    public static Dictionary<string, int> UtilityId { get; private set; }
    public static int TotalUtilities { get; private set; }
    public static Dictionary<string, Universum.Defs.CelestialObject> CelestialObjects { get; private set; }
    public static Dictionary<string, Universum.Defs.ObjectGeneration> CelestialObjectGenerationSteps  { get; private set; }
    public static Dictionary<string, Universum.Defs.ObjectGeneration> CelestialObjectGenerationStartUpSteps  { get; private set; }
    public static Dictionary<string, Universum.Defs.ObjectGeneration> CelestialObjectGenerationRandomSteps { get; private set; }
    public static Dictionary<string, Universum.Defs.Material> Materials { get; private set; }
    public static Dictionary<string, Universum.Defs.NamePack> NamePacks { get; private set; }
    public static Universum.Defs.ModExtension.BiomeProperties[] BiomeProperties { get; private set; }
    public static Universum.Defs.ModExtension.GeneProperties[] GeneProperties { get; private set; }
    public static Universum.Defs.ModExtension.TerrainProperties[] TerrainProperties { get; private set; }
    
    public static void Init() {
        ObjectHolderDef = Verse.DefDatabase<RimWorld.WorldObjectDef>.GetNamed("Universum_ObjectHolder");
        OceanBiomeDef = Verse.DefDatabase<RimWorld.BiomeDef>.GetNamed("Ocean");
        DecompressionDamageDef = Verse.DefDatabase<Verse.DamageDef>.GetNamed("Universum_Decompression_Damage");
        SuffocationHediffDef = Verse.DefDatabase<Verse.HediffDef>.GetNamed("Universum_Suffocation_Hediff");
        Colony.Patch.SectionLayer.vacuumGlassTerrainDef = Verse.DefDatabase<Verse.TerrainDef>.GetNamed(
            defName: "RimNauts2_Vacuum_Glass",
            errorOnFail: false
        );
        Utilities = new Dictionary<string, Universum.Defs.Utility>();
        UtilityId = new Dictionary<string, int>();
        CelestialObjects = new Dictionary<string, Universum.Defs.CelestialObject>();
        CelestialObjectGenerationSteps = new Dictionary<string, Universum.Defs.ObjectGeneration>();
        CelestialObjectGenerationStartUpSteps = new Dictionary<string, Universum.Defs.ObjectGeneration>();
        CelestialObjectGenerationRandomSteps = new Dictionary<string, Universum.Defs.ObjectGeneration>();
        Materials = new Dictionary<string, Universum.Defs.Material>();
        NamePacks = new Dictionary<string, Universum.Defs.NamePack>();
        BiomeProperties = [];
        GeneProperties = [];
        TerrainProperties = [];
        
        int totalDefsLoaded = LoadDefs();
        Debugger.Log(
            key: "Universum.Info.def_loader_done",
            prefix: Debugger.TAB,
            args: [totalDefsLoaded]
        );
        
        int totalBiomePropertiesLoaded = LoadBiomeProperties();
        Debugger.Log(
            key: "Universum.Info.biome_handler_done",
            prefix: Debugger.TAB,
            args: [BiomeProperties.Length, totalBiomePropertiesLoaded]
        );

        if (Verse.ModsConfig.BiotechActive) {
            int totalGenePropertiesLoaded = LoadGeneProperties();
            Debugger.Log(
                key: "Universum.Info.gene_handler_done",
                prefix: Debugger.TAB,
                args: [GeneProperties.Length, totalGenePropertiesLoaded]
            );
        }
        
        int totalTerrainPropertiesLoaded = LoadTerrainProperties();
        Debugger.Log(
            key: "Universum.Info.terrain_handler_done",
            prefix: Debugger.TAB,
            args: [TerrainProperties.Length, totalTerrainPropertiesLoaded]
        );
    }

    private static int LoadDefs() {
        int totalDefsLoaded = 0;

        List<Universum.Defs.Utility> utilityList = Verse.DefDatabase<Universum.Defs.Utility>.AllDefsListForReading;
        for (int i = 0; i < utilityList.Count; i++) {
            Universum.Defs.Utility utility = utilityList[i];
            Utilities[utility.defName] = utility;
            UtilityId[utility.id] = i;
            TotalUtilities++;
            
            totalDefsLoaded++;
        }

        List<Universum.Defs.CelestialObject> celestialObjectList = Verse.DefDatabase<Universum.Defs.CelestialObject>.AllDefsListForReading;
        foreach (Universum.Defs.CelestialObject celestialObject in celestialObjectList) {
            CelestialObjects[celestialObject.defName] = celestialObject;
            
            totalDefsLoaded++;
        }
        
        List<Universum.Defs.ObjectGeneration> objectGenerationList = Verse.DefDatabase<Universum.Defs.ObjectGeneration>.AllDefsListForReading;
        foreach (Universum.Defs.ObjectGeneration objectGeneration in objectGenerationList) {
            CelestialObjectGenerationSteps[objectGeneration.defName] = objectGeneration;
            
            switch (objectGeneration.initializationType) {
                case World.Initialization.Type.StartUp:
                    CelestialObjectGenerationStartUpSteps[objectGeneration.defName] = objectGeneration;
                    break;
                case World.Initialization.Type.Random:
                    CelestialObjectGenerationRandomSteps[objectGeneration.defName] = objectGeneration;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(World.Initialization.Type));
            }
            
            totalDefsLoaded++;
        }
        
        List<Universum.Defs.Material> materialList = Verse.DefDatabase<Universum.Defs.Material>.AllDefsListForReading;
        foreach (Universum.Defs.Material material in materialList) {
            Materials[material.defName] = material;
            
            totalDefsLoaded++;
        }
        
        List<Universum.Defs.NamePack> namePackList = Verse.DefDatabase<Universum.Defs.NamePack>.AllDefsListForReading;
        foreach (Universum.Defs.NamePack namePack in namePackList) {
            NamePacks[namePack.defName] = namePack;
            
            totalDefsLoaded++;
        }

        return totalDefsLoaded;
    }

    private static int LoadBiomeProperties() {
        int totalBiomePropertiesLoaded = 0;
        List<RimWorld.BiomeDef> biomeDefs = Verse.DefDatabase<RimWorld.BiomeDef>.AllDefsListForReading;
        int numBiomeDefs = biomeDefs.Count;
        
        BiomeProperties = new Universum.Defs.ModExtension.BiomeProperties[numBiomeDefs];

        for (int i = 0; i < numBiomeDefs; i++) {
            RimWorld.BiomeDef biomeDef = biomeDefs[i];
            
            BiomeProperties[biomeDef.index] =
                biomeDef.GetModExtension<Universum.Defs.ModExtension.BiomeProperties>() ??
                new Universum.Defs.ModExtension.BiomeProperties();

            totalBiomePropertiesLoaded += BiomeProperties[biomeDef.index].utilities.Count;

            int numUtilities = UtilityId.Count;
            
            BiomeProperties[biomeDef.index].activeUtilities = new bool[numUtilities];

            for (int j = 0; j < BiomeProperties[biomeDef.index].utilities.Count; j++) {
                if (UtilityId.TryGetValue(BiomeProperties[biomeDef.index].utilities[j], out int propertyIndex)) {
                    BiomeProperties[biomeDef.index].activeUtilities[propertyIndex] = true;
                }
            }
        }

        return totalBiomePropertiesLoaded;
    }

    private static int LoadGeneProperties() {
        int totalGenePropertiesLoaded = 0;
        List<Verse.GeneDef> geneDefs = Verse.DefDatabase<Verse.GeneDef>.AllDefsListForReading;
        int numGeneDefs = geneDefs.Count;
        
        GeneProperties = new Universum.Defs.ModExtension.GeneProperties[numGeneDefs];

        for (int i = 0; i < numGeneDefs; i++) {
            Verse.GeneDef geneDef = geneDefs[i];
            
            GeneProperties[geneDef.index] =
                geneDef.GetModExtension<Universum.Defs.ModExtension.GeneProperties>() ??
                new Universum.Defs.ModExtension.GeneProperties();

            totalGenePropertiesLoaded += GeneProperties[geneDef.index].utilities.Count;

            int numUtilities = UtilityId.Count;
            
            GeneProperties[geneDef.index].activeUtilities = new bool[numUtilities];

            for (int j = 0; j < GeneProperties[geneDef.index].utilities.Count; j++) {
                if (UtilityId.TryGetValue(GeneProperties[geneDef.index].utilities[j], out int propertyIndex)) {
                    GeneProperties[geneDef.index].activeUtilities[propertyIndex] = true;
                }
            }
        }

        return totalGenePropertiesLoaded;
    }

    private static int LoadTerrainProperties() {
        int totalTerrainPropertiesLoaded = 0;
        List<Verse.TerrainDef> terrainDefs = Verse.DefDatabase<Verse.TerrainDef>.AllDefsListForReading;
        int numTerrainDefs = terrainDefs.Count;
        
        TerrainProperties = new Universum.Defs.ModExtension.TerrainProperties[numTerrainDefs];

        for (int i = 0; i < numTerrainDefs; i++) {
            Verse.TerrainDef terrainDef = terrainDefs[i];
            
            TerrainProperties[terrainDef.index] =
                terrainDef.GetModExtension<Universum.Defs.ModExtension.TerrainProperties>() ??
                new Universum.Defs.ModExtension.TerrainProperties();

            totalTerrainPropertiesLoaded += TerrainProperties[terrainDef.index].utilities.Count;

            int numUtilities = UtilityId.Count;
            
            TerrainProperties[terrainDef.index].activeUtilities = new bool[numUtilities];

            for (int j = 0; j < TerrainProperties[terrainDef.index].utilities.Count; j++) {
                if (UtilityId.TryGetValue(TerrainProperties[terrainDef.index].utilities[j], out int propertyIndex)) {
                    TerrainProperties[terrainDef.index].activeUtilities[propertyIndex] = true;
                }
            }
        }

        return totalTerrainPropertiesLoaded;
    }
}
