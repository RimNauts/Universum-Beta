using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable ConvertToConstant.Global

namespace Universum.Defs;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
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
