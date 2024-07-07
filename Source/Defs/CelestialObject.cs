using System;
using System.Collections.Generic;
using UnityEngine;

namespace Universum.Defs;

public class CelestialObject : Verse.Def {
    public class ObjectHolder {
        public Verse.MapGeneratorDef mapGeneratorDef;
        public RimWorld.BiomeDef biomeDef;
        public bool keepAfterAbandon;
        public string description = "";
        public string overlayIconPath = "Universum_Transparent";
        public string commandLabelKey = "CommandSettle";
        public string commandDescKey = "CommandSettleDesc";
        public string commandIconPath = "UI/Commands/Settle";
    }
    
    public class Shape {
        public List<Mesh> meshes = [];
    }
    
    public class Icon {
        public string texturePath;
    }
    
    public Type celestialObjectClass = typeof(World.CelestialObject);

    public string namePackDefName;

    public Vector2 scalePercentageBetween = Vector2.one;
    public float? minSize = null;
    public Vector2 speedPercentageBetween = Vector2.one;
    public float orbitPathOffsetPercentage = 1.0f;
    public Vector2 orbitSpreadBetween = Vector2.one;
    public Vector2 yOffsetBetween = Vector2.zero;
    public Vector2 orbitEccentricityBetween = Vector2.zero;
    public World.CelestialObject.OrbitDirection orbitDirection = World.CelestialObject.OrbitDirection.Left;
    public Vector2 axialAngleBetween = Vector2.zero;
    public Vector2 spinRotationSpeedBetween = Vector2.zero;
    public Vector2 inclinationAngleBetween = Vector2.zero;

    public ObjectHolder objectHolder = null;

    public List<Component> components = [];

    public Shape shape = null;
    public Icon icon = null;
}
