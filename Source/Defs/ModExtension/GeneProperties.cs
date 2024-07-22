using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global

namespace Universum.Defs.ModExtension;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class GeneProperties : Verse.DefModExtension {
    public bool[] activeUtilities;
    public List<string> utilities = [];
}
