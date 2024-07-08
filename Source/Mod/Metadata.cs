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

        modSettings = new Settings();
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        ((Settings) modSettings).Window(inRect);
    }

    public override string SettingsCategory() {
        return $"{NAME} (v{VERSION})";
    }
}
