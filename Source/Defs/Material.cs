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
    public List<ShaderProperties> shaderProperties = [];
}
