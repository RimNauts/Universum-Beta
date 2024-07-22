using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Global

namespace Universum.World.Comps;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class DestroyObjectHolderProperties : RimWorld.WorldObjectCompProperties {
    public string label = "";
    public string desc = "";

    public DestroyObjectHolderProperties() => compClass = typeof(DestroyObjectHolder);
}

public class DestroyObjectHolder : RimWorld.Planet.WorldObjectComp {
    public DestroyObjectHolderProperties Props => (DestroyObjectHolderProperties) props;

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        if (Verse.DebugSettings.godMode && parent is ObjectHolder { HasMap: false }) {
            yield return new Verse.Command_Action {
                defaultLabel = Props.label,
                defaultDesc = Props.desc,
                action = DestroyObject,
            };
        }
    }

    private void DestroyObject() {
        ObjectHolder objectHolder = parent as ObjectHolder;

        objectHolder?.SignalDestruction();
    }
}
