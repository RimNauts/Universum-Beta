using System.Collections.Generic;
using UnityEngine;

namespace Universum.Defs;

public class Material : Verse.Def {
    public class ShaderProperties {
        public string name;
        public float? floatValue;
        public Color? colorValue;
        public string texturePathValue;
    }
    
    public string shaderName;
    public string texturePath;
    public Color color = Color.white;
    public int renderQueue = RimWorld.Planet.WorldMaterials.WorldObjectRenderQueue;
    public string bumpMap;
    public List<ShaderProperties> shaderProperties = [];
}
