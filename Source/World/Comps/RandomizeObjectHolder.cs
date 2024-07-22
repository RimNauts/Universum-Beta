using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Global

namespace Universum.World.Comps;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class RandomizeObjectHolderProperties : RimWorld.WorldObjectCompProperties {
    public string label = "";
    public string desc = "";

    public RandomizeObjectHolderProperties() => compClass = typeof(RandomizeObjectHolder);
}

public class RandomizeObjectHolder : RimWorld.Planet.WorldObjectComp {
    public RandomizeObjectHolderProperties Props => (RandomizeObjectHolderProperties) props;

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        if (Verse.DebugSettings.godMode) {
            yield return new Verse.Command_Action {
                defaultLabel = Props.label,
                defaultDesc = Props.desc,
                action = RandomizeObject,
            };
        }
    }

    private void RandomizeObject() {
        ObjectHolder objectHolder = parent as ObjectHolder;
        objectHolder?.Randomize();
    }
}
