using System;
using HarmonyLib;

namespace Universum.Cache;

public class SubscriptionTracker(Harmony harmony) {
    public bool active;
    private int _numSubscribers;
    private bool _settingsEnabled;
    private PatchClassProcessor[] _patches = [];
    private int _numPatches;

    public void AddPatches(Type[] classesToPatch) {
        if (classesToPatch == null) return;
        
        _numPatches = classesToPatch.Length;
        _patches = new PatchClassProcessor[_numPatches];

        for (int i = 0; i < _numPatches; i++) {
            _patches[i] = new PatchClassProcessor(harmony, classesToPatch[i]);
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

    public void Reset() {
        if (_numSubscribers == 0) return;
        
        _numSubscribers = 0;

        UpdateActiveState();
    }
    
    public void SetSettingsEnabled(bool enabled) {
        if (_settingsEnabled == enabled) return;

        _settingsEnabled = enabled;

        UpdateActiveState();
    }
    
    public void UpdateActiveState() {
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
}
