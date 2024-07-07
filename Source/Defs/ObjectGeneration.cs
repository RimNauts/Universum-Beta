using System.Collections.Generic;
using UnityEngine;

namespace Universum.Defs;

public class ObjectGeneration : Verse.Def {
    public class ObjectGenerationChance {
        public string celestialDefName;
        public int tickets = 1;
    }
    
    public int total;
    public World.Initialization.Type initializationType = World.Initialization.Type.StartUp;
    public Vector2 spawnBetweenDays = Vector2.one;
    public Vector2 despawnBetweenDays = Vector2.zero;
    public Vector2 spawnAmountBetween = Vector2.one;
    public List<ObjectGenerationChance> objectGroup = [];
}
