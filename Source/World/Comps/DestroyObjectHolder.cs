using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Universum.World.Comps;

public class DestroyObjectHolderProperties : RimWorld.WorldObjectCompProperties {
    public string label = "";
    public string desc = "";

    public DestroyObjectHolderProperties() => compClass = typeof(DestroyObjectHolder);
}

public class DestroyObjectHolder : RimWorld.Planet.WorldObjectComp {
    public DestroyObjectHolderProperties Props => (DestroyObjectHolderProperties) props;

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        ObjectHolder objectHolder = parent as ObjectHolder;
        if (Verse.DebugSettings.godMode && !objectHolder.HasMap) {
            yield return new Verse.Command_Action {
                defaultLabel = Props.label,
                defaultDesc = Props.desc,
                action = DestroyObject,
            };
        }
    }

    private void DestroyObject() {
        ObjectHolder objectHolder = parent as ObjectHolder;
        objectHolder.SignalDestruction();
    }
}
