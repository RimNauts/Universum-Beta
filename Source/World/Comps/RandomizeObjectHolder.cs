using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Universum.World.Comps;

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
        objectHolder.Randomize();
    }
}
