using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace Universum.Cache;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public abstract class SubscriptionTracker {
    public int id = -1;
    public bool active;
    private bool _alwaysActive;
    private int _numSubscribers;
    private bool _settingsEnabled;
    private PatchClassProcessor[] _patches = [];
    private int _numPatches;

    public virtual void Init(string key, Type[] classesToPatch = null, bool alwaysActive = false) {
        Loader.Defs.UtilityId.TryGetValue(key, out id);
        
        _alwaysActive = alwaysActive;
        
        AddPatches(classesToPatch);
        
        if (_alwaysActive) Subscribe();
    }

    private void AddPatches(Type[] classesToPatch) {
        if (classesToPatch == null) return;
        
        _numPatches = classesToPatch.Length;
        _patches = new PatchClassProcessor[_numPatches];

        for (int i = 0; i < _numPatches; i++) {
            _patches[i] = new PatchClassProcessor(Mod.Manager.HARMONY, classesToPatch[i]);
        }
    }

    public void Subscribe() {
        _numSubscribers++;

        UpdateActiveState();
    }

    public void Unsubscribe() {
        if (_numSubscribers <= 0) return;
        
        _numSubscribers--;

        UpdateActiveState();
    }

    public abstract void Reset();

    protected void ResetTracker() {
        if (_numSubscribers == 0) return;
        
        _numSubscribers = _alwaysActive ? 1 : 0;

        UpdateActiveState();
    }
    
    public void SetSettingsEnabled(bool enabled) {
        if (_settingsEnabled == enabled) return;

        _settingsEnabled = enabled;

        UpdateActiveState();
    }
    
    private void UpdateActiveState() {
        bool shouldBeActive = _numSubscribers > 0 && _settingsEnabled;

        if (active == shouldBeActive) return;

        active = shouldBeActive;
        
        if (active) {
            Activate();
        } else {
            Deactivate();
        }
    }
    
    private void Activate() {
        for (int i = 0; i < _numPatches; i++) _patches[i].Patch();
        
        Verse.Log.Message("Activated: Harmony patches are now active.");
    }

    private void Deactivate() {
        for (int i = 0; i < _numPatches; i++) _patches[i].Unpatch();
        
        Verse.Log.Message("Deactivated: Harmony patches are now inactive.");
    }

    public void ResetPatching() {
        for (int i = 0; i < _numPatches; i++) _patches[i].Unpatch();

        if (!active) return;
        for (int i = 0; i < _numPatches; i++) _patches[i].Patch();
    }

    public bool CheckBiome(int biomeIndex) => Loader.Defs.BiomeProperties[biomeIndex].activeUtilities[id];

    public bool CheckTerrain(int terrainIndex) => Loader.Defs.TerrainProperties[terrainIndex].activeUtilities[id];
}
