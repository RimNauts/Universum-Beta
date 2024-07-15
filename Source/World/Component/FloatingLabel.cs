using UnityEngine;

namespace Universum.World.Component;

public class FloatingLabel : ObjectComponent {
    private readonly TMPro.TextMeshPro TEXT_COMPONENT;
    private readonly int UTILITY_ID;
    
    public FloatingLabel(CelestialObject celestialObject, Defs.Component def) : base(celestialObject, def) {
        TEXT_COMPONENT = _gameObject.GetComponent<TMPro.TextMeshPro>();
        
        Loader.Defs.UtilityId.TryGetValue(key: "universum.labels", out UTILITY_ID);

        TEXT_COMPONENT.text = def.overwriteText ?? celestialObject.name;
        TEXT_COMPONENT.color = def.color;
        TEXT_COMPONENT.fontSize = def.fontSize;
        TEXT_COMPONENT.outlineColor = def.outlineColor;
        TEXT_COMPONENT.outlineWidth = def.outlineWidth;
        TEXT_COMPONENT.overflowMode = TMPro.TextOverflowModes.Overflow;
        foreach (Material sharedMaterial in TEXT_COMPONENT.GetComponent<MeshRenderer>().sharedMaterials) {
            sharedMaterial.renderQueue = RimWorld.Planet.WorldMaterials.FeatureNameRenderQueue;
        }

        SetActive(false);
    }

    public override void UpdateInfo() {
        base.UpdateInfo();
        TEXT_COMPONENT.text = _def.overwriteText ?? _celestialObject.name;
    }

    public override void Update() {
        SetBlock(!Cache.Settings.UtilityEnabled(UTILITY_ID));

        base.Update();
        Hide();
    }

    public override void UpdatePosition() {
        Vector3 position = _celestialObject.transformedPosition + _offset;

        Vector3 directionFromObjectToCamera = Game.MainLoop.instance.cameraPosition - position;
        directionFromObjectToCamera.Normalize();

        _position = position + directionFromObjectToCamera * (_celestialObject.scale.y + _celestialObject.extraScale) * 1.2f;
        _position -= Game.MainLoop.instance.cameraUp * (_celestialObject.scale.y + _celestialObject.extraScale) * 1.2f;
    }

    public override void UpdateRotation() {
        _rotation = Quaternion.LookRotation(Game.MainLoop.instance.cameraForward, Vector3.up);
    }

    public override void UpdateTransformationMatrix() {
        TEXT_COMPONENT.transform.position = _position;
        TEXT_COMPONENT.transform.rotation = _rotation;
    }

    private void Hide() {
        float distanceFromCamera = Vector3.Distance(_position, Game.MainLoop.instance.cameraPosition);

        bool tooClose = distanceFromCamera < _hideAtMinAltitude;
        bool tooFar = distanceFromCamera > _hideAtMaxAltitude;
        bool outsideRange = tooClose || tooFar;

        SetBlock(outsideRange);
    }
}
