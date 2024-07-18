using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Global

namespace Universum.World.Comps;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class GenerateObjectHolderMapProperties : RimWorld.WorldObjectCompProperties {
    public string label = "";
    public string desc = "";

    public GenerateObjectHolderMapProperties() => compClass = typeof(GenerateObjectHolderMap);
}

public class GenerateObjectHolderMap : RimWorld.Planet.WorldObjectComp {
    public GenerateObjectHolderMapProperties Props => (GenerateObjectHolderMapProperties) props;

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        if (Verse.DebugSettings.godMode && parent is ObjectHolder { HasMap: false, MapGeneratorDef: not null }) {
            yield return new Verse.Command_Action {
                defaultLabel = Props.label,
                defaultDesc = Props.desc,
                action = GenerateMap,
            };
        }
    }

    private void GenerateMap() {
        ObjectHolder objectHolder = parent as ObjectHolder;
        
        objectHolder?.CreateMap(RimWorld.Faction.OfPlayer, clearFog: true);
    }
}
