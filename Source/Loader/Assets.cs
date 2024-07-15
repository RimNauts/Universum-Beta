using System.Collections.Generic;
using UnityEngine;

namespace Universum.Loader;

[Verse.StaticConstructorOnStartup]
public static class Assets {
    private static AssetBundle _assets;
    public static Dictionary<string, Shader> Shaders { get; private set; }
    public static Dictionary<string, Texture2D> Textures { get; private set; }
    public static Dictionary<string, Material> Materials { get; private set; }
    public static GameObject GameObjectWorldText { get; private set; }
    public static string IndoorsText { get; private set; }
    public static string CustomIndoorsText { get; private set; }
    public static string OutdoorsText { get; private set; }
    public static string CustomOutdoorsText { get; private set; }
    public static string UnroofedText { get; private set; }
    public static string CustomUnroofedText { get; private set; }

    public static void Init() {
        Shaders = new Dictionary<string, Shader>();
        Textures = new Dictionary<string, Texture2D>();
        Materials = new Dictionary<string, Material>();
        
        GetAssets();

        Shaders["Sprites/Default"] = Shader.Find("Sprites/Default");
        GameObjectWorldText = Resources.Load<GameObject>("Prefabs/WorldText");

        IndoorsText = Verse.TranslatorFormattedStringExtensions.Translate("Indoors");
        CustomIndoorsText = Verse.TranslatorFormattedStringExtensions.Translate("Universum.indoors");
        OutdoorsText = Verse.TranslatorFormattedStringExtensions.Translate("Outdoors").CapitalizeFirst();
        CustomOutdoorsText = Verse.TranslatorFormattedStringExtensions.Translate("Universum.outdoors").CapitalizeFirst();
        UnroofedText = Verse.TranslatorFormattedStringExtensions.Translate("IndoorsUnroofed");
        CustomUnroofedText = Verse.TranslatorFormattedStringExtensions.Translate("Universum.unroofed");
        
        // populate cache
        foreach (var (_, materialDef) in Defs.Materials) {
            Shader shaderInstance = GetShader(materialDef.shaderName);
            Material material = new Material(shaderInstance);

            material.renderQueue = materialDef.renderQueue;
            material.color = materialDef.color;

            if (materialDef.texturePath != null) material.mainTexture = GetTexture(materialDef.texturePath);

            foreach (Universum.Defs.Material.ShaderProperties shaderProperties in materialDef.shaderProperties) {
                if (shaderProperties.floatValue != null) {
                    material.SetFloat(shaderProperties.name, (float) shaderProperties.floatValue);
                } else if (shaderProperties.colorValue != null) {
                    material.SetColor(shaderProperties.name, (Color) shaderProperties.colorValue);
                } else if (shaderProperties.texturePathValue != null) {
                    material.SetTexture(shaderProperties.name, GetTexture(shaderProperties.texturePathValue));
                }
            }

            Materials[materialDef.defName] = material;
        }

        foreach (var (_, celestialObjectDef) in Defs.CelestialObjects) {
            if (celestialObjectDef.icon != null) GetTexture(celestialObjectDef.icon.texturePath);
            if (celestialObjectDef.objectHolder == null) continue;
            if (celestialObjectDef.objectHolder.overlayIconPath != null) GetTexture(celestialObjectDef.objectHolder.overlayIconPath);
            if (celestialObjectDef.objectHolder.commandIconPath != null) GetTexture(celestialObjectDef.objectHolder.commandIconPath);
        }
    }
    
    private static void GetAssets() {
        string platformStr;
        switch (Application.platform) {
            case RuntimePlatform.OSXPlayer:
                platformStr = "mac";
                break;
            case RuntimePlatform.WindowsPlayer:
                platformStr = "windows";
                break;
            case RuntimePlatform.LinuxPlayer:
                platformStr = "linux";
                break;
            default:
                Debugger.Log(
                    key: "Universum.Info.assets_loaded",
                    prefix: Debugger.TAB,
                    args: ["no supported"]
                );
                return;
        }
        
        foreach (var assets in Mod.Manager.METADATA.Content.assetBundles.loadedAssetBundles) {
            if (!assets.name.Contains(platformStr)) continue;
            
            _assets = assets;
            
            Debugger.Log(
                key: "Universum.Info.assets_loaded",
                prefix: Debugger.TAB,
                args: [platformStr]
            );
            return;
        }
        
        Debugger.Log(
            key: "Universum.Info.assets_loaded",
            prefix: Debugger.TAB,
            args: ["no supported"]
        );
    }

    public static Shader GetShader(string shaderName) {
        if (Shaders.TryGetValue(shaderName, out Shader value)) return value;
        
        Shaders[shaderName] = GetAsset(shaderName, Verse.ShaderDatabase.WorldOverlayCutout);
        return Shaders[shaderName];
    }

    public static Texture2D GetTexture(string path) {
        if (Textures.TryGetValue(path, out Texture2D value)) return value;
        
        Textures[path] = GetContent<Texture2D>(path);
        return Textures[path];
    }

    private static T GetAsset<T>(string name, T fallback = null) where T : Object {
        return _assets == null ? fallback : _assets.LoadAsset<T>(name);
    }

    private static T GetContent<T>(string path, T fallback = null) where T : Object {
        return _assets == null ? fallback : Verse.ContentFinder<T>.Get(path);
    }
}
