using System.Collections.Generic;
using UnityEngine;

namespace Universum.Defs;

public class Mesh : Verse.Def {
    public class Noise {
        public bool isMask;
        public bool useMask;
        public Vector2 strengthBetween = Vector2.one;
        public Vector2 roughnessBetween = Vector2.one;
        public Vector2 iterationsBetween = Vector2.one;
        public Vector2 persistenceBetween = Vector2.one;
        public Vector2 baseRoughnessBetween = Vector2.one;
        public Vector2 minNoiseValueBetween = Vector2.zero;
    }
    
    public string materialDefName;
    public Common.Shape.Type type;
    public int subdivisionIterations;
    public Vector2 detailBetween = new Vector2(5, 5);
    public float radius = 1.0f;
    public Vector3 dimensions = Vector3.one;
    public Color? maxElevationColor;
    public Color? minElevationColor;
    public float craterDepth;
    public float craterRimHeight;
    public List<Noise> noiseLayers = [];
}
