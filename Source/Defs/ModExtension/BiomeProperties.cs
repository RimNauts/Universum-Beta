using System.Collections.Generic;

namespace Universum.Defs.ModExtension;

public class BiomeProperties : Verse.DefModExtension {
    public bool[] activeUtilities;
    public List<string> utilities = [];
    public float temperature = 0.0f;
}
