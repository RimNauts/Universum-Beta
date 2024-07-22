using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable ConvertToConstant.Global

namespace Universum.Defs;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
public class Component : Verse.Def {
    public Type componentClass = typeof(World.ObjectComponent);
    public string materialDefName;
    public Vector3 offSet = Vector3.zero;
    public Color color = Color.white;
    public float hideAtMinAltitude = float.MaxValue;
    public float hideAtMaxAltitude = float.MinValue;
    public string overwriteText;
    public float fontSize;
    public Color outlineColor = Color.black;
    public float outlineWidth;
    public float trailWidth;
    public float trailLength;
    public float trailTransparency;
}
