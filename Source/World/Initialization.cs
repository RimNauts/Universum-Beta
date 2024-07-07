using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Universum.World;

public class Initialization : Verse.WorldGenStep {
    public enum Type {
        StartUp = 0,
        Random = 1
    }
    
    public override int SeedPart => 0;
    public static int nextId = 0;

    public override void GenerateFresh(string seed) {
        Game.MainLoop.instance.FreshGame();
        GenerateOnStartUp();
    }

    public static void GenerateOnStartUp() {
        foreach (Defs.ObjectGeneration objectGenerationStep in Loader.Defs.CelestialObjectGenerationStartUpSteps.Values) {
            int total = Settings.totalToSpawnGenStep[objectGenerationStep.defName];
            for (int i = 0; i < total; i++) {
                string celestialDefName = Verse.GenCollection.RandomElementByWeight(objectGenerationStep.objectGroup, o => o.tickets).celestialDefName;
                if (Loader.Defs.CelestialObjects[celestialDefName].objectHolder != null) {
                    CreateObjectHolder(celestialDefName);
                } else Create(celestialDefName);
            }
        }
    }

    public static void Regenerate() {
        foreach (Defs.ObjectGeneration objectGenerationStep in Loader.Defs.CelestialObjectGenerationStartUpSteps.Values) {
            bool respawnFlag = true;
            foreach (var objectToSpawn in objectGenerationStep.objectGroup.Where(objectToSpawn => Loader.Defs.CelestialObjects[objectToSpawn.celestialDefName].objectHolder != null)) {
                respawnFlag = false;
            }
            if (!respawnFlag) continue;

            foreach (var objectToSpawn in objectGenerationStep.objectGroup) Game.MainLoop.instance.ShouldDestroy(Loader.Defs.CelestialObjects[objectToSpawn.celestialDefName]);
            Game.MainLoop.instance.GameComponentUpdate();

            int total = Settings.totalToSpawnGenStep[objectGenerationStep.defName];
            for (int i = 0; i < total; i++) {
                string celestialDefName = Verse.GenCollection.RandomElementByWeight(objectGenerationStep.objectGroup, o => o.tickets).celestialDefName;
                if (Loader.Defs.CelestialObjects[celestialDefName].objectHolder != null) {
                    CreateObjectHolder(celestialDefName);
                } else Create(celestialDefName);
            }
        }
        Game.MainLoop.instance.dirtyCache = true;
    }

    public static void Generate(Defs.ObjectGeneration objectGenerationStep, Vector2 despawnBetweenDays, int? amount = null) {
        int totalObjectsAlive = objectGenerationStep.objectGroup.Sum(objectToSpawn => Game.MainLoop.instance.GetTotal(Loader.Defs.CelestialObjects[objectToSpawn.celestialDefName]));
        if (totalObjectsAlive >= Settings.totalToSpawnGenStep[objectGenerationStep.defName]) return;

        int total = amount ?? Settings.totalToSpawnGenStep[objectGenerationStep.defName];
        List<string> celestialDefNames = new List<string>();
        List<ObjectHolder> objectHolders = new List<ObjectHolder>();

        for (int i = 0; i < total; i++) {
            string celestialDefName = Verse.GenCollection.RandomElementByWeight(objectGenerationStep.objectGroup, o => o.tickets).celestialDefName;
            celestialDefNames.Add(celestialDefName);

            int? deathTick = null;
            if (despawnBetweenDays != Vector2.zero) deathTick = (int) Verse.Rand.Range(despawnBetweenDays[0] * 60000, despawnBetweenDays[1] * 60000) + Game.MainLoop.instance.tick;

            if (Loader.Defs.CelestialObjects[celestialDefName].objectHolder != null) {
                ObjectHolder objectHolder = CreateObjectHolder(celestialDefName, celestialObjectDeathTick: deathTick);
                objectHolders.Add(objectHolder);
            } else Create(celestialDefName, deathTick: deathTick);
        }

        SendLetter(objectGenerationStep, celestialDefNames, objectHolders);
    }

    public static void SendLetter(Defs.ObjectGeneration objectGenerationStep, List<string> celestialDefNames, List<ObjectHolder> objectHolders) { }

    public static List<CelestialObject> Create(
        List<string> celestialObjectDefNames,
        List<int?> seeds = null,
        List<int?> ids = null,
        List<int?> targetIds = null,
        List<Vector3?> positions = null,
        List<int?> deathTicks = null
    ) {
        List<CelestialObject> celestialObjects = [];
        for (int i = 0; i < celestialObjectDefNames.Count; i++) {
            string celestialObjectDefName = celestialObjectDefNames[i];

            if (!Loader.Defs.CelestialObjects.ContainsKey(celestialObjectDefName)) continue;
            
            int? seed = null;
            if (seeds is not null || seeds.Count > 0) seed = seeds[i];
            
            int? id = null;
            if (ids is not null || ids.Count > 0) id = ids[i];

            int? targetId = null;
            if (targetIds is not null || targetIds.Count > 0) targetId = targetIds[i];

            Vector3? position = null;
            if (positions is not null || positions.Count > 0) position = positions[i];

            int? deathTick = null;
            if (deathTicks is not null || deathTicks.Count > 0) deathTick = deathTicks[i];

            CelestialObject celestialObject = (CelestialObject) Activator.CreateInstance(
                Loader.Defs.CelestialObjects[celestialObjectDefName].celestialObjectClass,
                [celestialObjectDefName]
            );
            
            celestialObject.Init(seed, id, targetId, position, deathTick);
            celestialObjects.Add(celestialObject);
        }
        Game.MainLoop.instance.AddObject(celestialObjects);

        return celestialObjects;
    }

    public static CelestialObject Create(string celestialObjectDefName, int? seed = null, int? id = null, int? targetId = null, Vector3? position = null, int? deathTick = null) {
        CelestialObject celestialObject = (CelestialObject) Activator.CreateInstance(
            Loader.Defs.CelestialObjects[celestialObjectDefName].celestialObjectClass,
            [celestialObjectDefName]
        );

        celestialObject.Init(seed, id, targetId, position, deathTick);

        Game.MainLoop.instance.AddObject(celestialObject);

        return celestialObject;
    }

    public static ObjectHolder CreateObjectHolder(
        string celestialObjectDefName,
        int? celestialObjectSeed = null,
        int? celestialObjectId = null,
        int? celestialObjectTargetId = null,
        Vector3? celestialObjectPosition = null,
        int? celestialObjectDeathTick = null,
        CelestialObject celestialObject = null,
        int tile = -1
    ) {
        if (tile == -1) tile = GetFreeTile();
        if (tile == -1) return null;
        UpdateTile(tile, Loader.Defs.CelestialObjects[celestialObjectDefName].objectHolder.biomeDef);

        ObjectHolder objectHolder = (ObjectHolder) Activator.CreateInstance(Loader.Defs.ObjectHolderDef.worldObjectClass);
        objectHolder.def = Loader.Defs.ObjectHolderDef;
        objectHolder.ID = Verse.Find.UniqueIDsManager.GetNextWorldObjectID();
        objectHolder.creationGameTicks = Verse.Find.TickManager.TicksGame;
        objectHolder.Tile = tile;

        objectHolder.Init(celestialObjectDefName, celestialObjectSeed, celestialObjectId, celestialObjectTargetId, celestialObjectPosition, celestialObjectDeathTick, celestialObject);
        objectHolder.PostMake();
        Verse.Find.WorldObjects.Add(objectHolder);

        return objectHolder;
    }

    public static void UpdateTile(int tile, RimWorld.BiomeDef biome) {
        Verse.Find.World.grid.tiles.ElementAt(tile).biome = biome;
        Verse.Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(tile);
    }

    public static int GetFreeTile(int startIndex = 1) {
        for (int i = startIndex; i < Verse.Find.World.grid.TilesCount; i++) {
            if (Verse.Find.World.grid.tiles.ElementAt(i).biome != Loader.Defs.OceanBiomeDef ||
                Verse.Find.World.worldObjects.AnyWorldObjectAt(i)) continue;
            
            List<int> neighbors = [];
            Verse.Find.World.grid.GetTileNeighbors(i, neighbors);
            if (neighbors.Count != 6) continue;

            var flag = neighbors.Select(neighbour => Verse.Find.World.grid.tiles.ElementAtOrDefault(neighbour)).Where(neighbourTile => neighbourTile != default(RimWorld.Planet.Tile)).Any(neighbourTile => neighbourTile.biome != Loader.Defs.OceanBiomeDef);

            if (flag) continue;

            return i;
        }
        
        return -1;
    }
}
