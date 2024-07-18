using UnityEngine;

namespace Universum.Mod;

[Verse.StaticConstructorOnStartup]
public class Metadata : Verse.Mod {
    public static Metadata Instance { get; private set; }

    public readonly string
        ID,
        NAME,
        VERSION;
    
    public Metadata(Verse.ModContentPack content) : base(content) {
        Instance = this;

        ID = content.ModMetaData.PackageId;
        NAME = content.ModMetaData.Name;
        VERSION = content.ModMetaData.ModVersion;
    }

    public override void DoSettingsWindowContents(Rect inRect) => Settings.Window(inRect);

    public override string SettingsCategory() => $"{NAME} (v{VERSION})";
}
