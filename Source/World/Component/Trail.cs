using System;
using UnityEngine;

namespace Universum.World.Component;

public class Trail : ObjectComponent {
    private readonly TrailRenderer TRAIL_COMPONENT;
    private readonly int UTILITY_ID;

    private Verse.TimeSpeed _prevGameSpeed;
    private readonly float TRAIL_LENGTH;
    private float _speed;
    
    public Trail(CelestialObject celestialObject, Defs.Component def) : base(celestialObject, def) {
        gameObject.GetComponent<TMPro.TextMeshPro>().enabled = false;
        TRAIL_COMPONENT = gameObject.AddComponent<TrailRenderer>();
        
        Loader.Defs.UtilityId.TryGetValue(key: "universum.trails", out UTILITY_ID);

        _prevGameSpeed = Verse.TimeSpeed.Paused;
        TRAIL_LENGTH = def.trailLength;
        TRAIL_COMPONENT.startWidth = celestialObject.scale.x * def.trailWidth;
        TRAIL_COMPONENT.endWidth = 0.0f;
        TRAIL_COMPONENT.time = 0.0f;
        TRAIL_COMPONENT.material = Loader.Assets.Materials[def.materialDefName];
        Color color = def.color;
        TRAIL_COMPONENT.startColor = new Color(color.r, color.g, color.b, def.trailTransparency);
        TRAIL_COMPONENT.endColor = new Color(color.r, color.g, color.b, 0.0f);
        foreach (Material sharedMaterial in TRAIL_COMPONENT.sharedMaterials) {
            sharedMaterial.renderQueue = TRAIL_COMPONENT.material.renderQueue;
        }

        SetActive(false);
    }

    public override void Clear() {
        base.Clear();
        TRAIL_COMPONENT.Clear();
    }

    public override void OnWorldSceneActivated() {
        base.OnWorldSceneActivated();
        TRAIL_COMPONENT.Clear();
    }

    public override void OnWorldSceneDeactivated() {
        base.OnWorldSceneDeactivated();
        TRAIL_COMPONENT.Clear();
    }

    public override void Update() {
        SetBlock(!Cache.Settings.UtilityEnabled(UTILITY_ID));

        base.Update();
        Verse.TimeSpeed currentSpeed = Game.MainLoop.instance.timeSpeed;
        
        if (currentSpeed == _prevGameSpeed) return;
        
        _prevGameSpeed = currentSpeed;
        _speed = (float) Math.Pow(3.0, (double) currentSpeed - 1.0);
    }

    protected override void UpdateTransformationMatrix() {
        TRAIL_COMPONENT.transform.position = position;
        if (_speed <= 0) {
            TRAIL_COMPONENT.time = 0.0f;
        } else TRAIL_COMPONENT.time = TRAIL_LENGTH / _speed;
    }

    public override void SetActive(bool active) {
        if (this.active != active) TRAIL_COMPONENT.Clear();
        base.SetActive(active);
    }
}
