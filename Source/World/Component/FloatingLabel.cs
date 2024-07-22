using UnityEngine;

namespace Universum.World.Component;

public class FloatingLabel : ObjectComponent {
    private readonly TMPro.TextMeshPro TEXT_COMPONENT;
    private readonly int UTILITY_ID;
    
    public FloatingLabel(CelestialObject celestialObject, Defs.Component def) : base(celestialObject, def) {
        TEXT_COMPONENT = gameObject.GetComponent<TMPro.TextMeshPro>();
        
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
        TEXT_COMPONENT.text = DEF.overwriteText ?? CELESTIAL_OBJECT.name;
    }

    public override void Update() {
        SetBlock(!Cache.Settings.UtilityEnabled(UTILITY_ID));

        base.Update();
        Hide();
    }

    protected override void UpdatePosition() {
        Vector3 newPosition = CELESTIAL_OBJECT.transformedPosition + offset;

        Vector3 directionFromObjectToCamera = Game.MainLoop.instance.cameraPosition - newPosition;
        directionFromObjectToCamera.Normalize();

        position = newPosition + directionFromObjectToCamera * (CELESTIAL_OBJECT.scale.y + CELESTIAL_OBJECT.extraScale) * 1.2f;
        position -= Game.MainLoop.instance.cameraUp * (CELESTIAL_OBJECT.scale.y + CELESTIAL_OBJECT.extraScale) * 1.2f;
    }

    protected override void UpdateRotation() {
        rotation = Quaternion.LookRotation(Game.MainLoop.instance.cameraForward, Vector3.up);
    }

    protected override void UpdateTransformationMatrix() {
        TEXT_COMPONENT.transform.position = position;
        TEXT_COMPONENT.transform.rotation = rotation;
    }

    private void Hide() {
        float distanceFromCamera = Vector3.Distance(position, Game.MainLoop.instance.cameraPosition);

        bool tooClose = distanceFromCamera < HIDE_AT_MIN_ALTITUDE;
        bool tooFar = distanceFromCamera > HIDE_AT_MAX_ALTITUDE;
        bool outsideRange = tooClose || tooFar;

        SetBlock(outsideRange);
    }
}
