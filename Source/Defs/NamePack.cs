using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable ConvertToConstant.Global

namespace Universum.Defs;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
public class NamePack : Verse.Def {
    public List<string> prefix;
    public List<string> postfix;
}
