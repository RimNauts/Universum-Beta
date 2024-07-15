using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Universum.World.Comps;

public class GenerateObjectHolderMapProperties : RimWorld.WorldObjectCompProperties {
    public string label = "";
    public string desc = "";

    public GenerateObjectHolderMapProperties() => compClass = typeof(GenerateObjectHolderMap);
}

public class GenerateObjectHolderMap : RimWorld.Planet.WorldObjectComp {
    public GenerateObjectHolderMapProperties Props => (GenerateObjectHolderMapProperties) props;

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        ObjectHolder objectHolder = parent as ObjectHolder;
        if (Verse.DebugSettings.godMode && !objectHolder.HasMap && objectHolder.MapGeneratorDef != null) {
            yield return new Verse.Command_Action {
                defaultLabel = Props.label,
                defaultDesc = Props.desc,
                action = GenerateMap,
            };
        }
    }

    private void GenerateMap() {
        ObjectHolder objectHolder = parent as ObjectHolder;
        objectHolder.CreateMap(RimWorld.Faction.OfPlayer, clearFog: true);
    }
}
