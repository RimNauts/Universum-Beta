using UnityEngine;

namespace Universum.Mod;

[Verse.StaticConstructorOnStartup]
public class Metadata : Verse.Mod {
    public static Metadata Instance { get; private set; }
    public string ID { get; }
    public string Name { get; }
    public string Version { get; }

    public Metadata(Verse.ModContentPack content) : base(content) {
        Instance = this;

        ID = content.ModMetaData.PackageId;
        Name = content.ModMetaData.Name;
        Version = content.ModMetaData.ModVersion;
    }

    public override void DoSettingsWindowContents(Rect inRect) => Settings.Window(inRect);

    public override string SettingsCategory() => $"{Name} (v{Version})";
}
