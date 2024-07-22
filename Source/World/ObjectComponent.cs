using System.Diagnostics.CodeAnalysis;
using UnityEngine;
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Universum.World;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public abstract class ObjectComponent {
    protected readonly CelestialObject CELESTIAL_OBJECT;
    protected readonly Defs.Component DEF;
    protected GameObject gameObject;
    protected bool active = true;
    private bool _block;

    protected Vector3 position = Vector3.zero;
    protected Quaternion rotation = Quaternion.identity;

    protected Vector3 offset;
    protected readonly float HIDE_AT_MIN_ALTITUDE;
    protected readonly float HIDE_AT_MAX_ALTITUDE;

    protected ObjectComponent(CelestialObject celestialObject, Defs.Component def) {
        CELESTIAL_OBJECT = celestialObject;
        DEF = def;

        gameObject = Object.Instantiate(Loader.Assets.GameObjectWorldText);
        Object.DontDestroyOnLoad(gameObject);

        offset = def.offSet;
        HIDE_AT_MIN_ALTITUDE = def.hideAtMinAltitude;
        HIDE_AT_MAX_ALTITUDE = def.hideAtMaxAltitude;
    }

    public virtual void Destroy() {
        if (gameObject == null) return;
        
        Object.Destroy(gameObject);
        gameObject = null;
    }

    public virtual void UpdateInfo() { }

    public virtual void Clear() { }

    public virtual void OnWorldSceneActivated() { }

    public virtual void OnWorldSceneDeactivated() { }

    public virtual void Update() {
        UpdatePosition();
        UpdateRotation();
    }

    protected virtual void UpdatePosition() {
        position = CELESTIAL_OBJECT.transformedPosition + offset;
    }

    protected virtual void UpdateRotation() { }

    public virtual void Render() {
        SetActive(!_block);
        if (!active) return;
        UpdateTransformationMatrix();
    }

    protected virtual void UpdateTransformationMatrix() { }

    protected virtual void SetBlock(bool block) => _block = block;

    public virtual void SetActive(bool active) {
        if (this.active == active) return;
        this.active = active;
        gameObject.SetActive(active);
    }
}
