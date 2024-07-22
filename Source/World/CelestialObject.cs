using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Universum.Mod;
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace Universum.World;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
public abstract class CelestialObject(string celestialObjectDefName) {
    public enum OrbitDirection {
        Left = 0,
        Right = 1,
        Random = 2
    }
    
    public int seed;
    public int id = -1;
    public string name;
    public readonly Defs.CelestialObject DEF = Loader.Defs.CelestialObjects[celestialObjectDefName];

    public ObjectHolder objectHolder = null;

    protected Common.Random rand;

    protected bool blockRendering;
    protected bool generatingVisuals;
    protected bool dirty;
    protected bool positionChanged = true;
    protected bool rotationChanged = true;
    protected bool scaleChanged = true;

    public int? deathTick;
    public bool forceDeath = false;

    protected Common.Shape shape;
    protected bool icon;

    protected Texture2D iconTexture;
    protected Matrix4x4 transformationMatrix = Matrix4x4.identity;
    protected Quaternion rotation = Quaternion.identity;
    protected bool addBillboardRotation;
    protected Quaternion billboardRotation = Quaternion.identity;
    protected Quaternion axialRotation = Quaternion.identity;
    protected Quaternion spinRotation = Quaternion.identity;
    protected Quaternion inclinationRotation = Quaternion.identity;
    protected CelestialObject target;
    public int targetId = -1;

    protected Transform[] transforms = [];
    protected ObjectComponent[] components = [];

    public Vector3 transformedPosition;
    public Vector3 position;
    private Vector3 _localPosition;
    public Vector3 scale;
    protected float scalePercentage;
    public float extraScale;
    public double speed;
    protected double speedPercentage;
    protected int period;
    protected int timeOffset;
    protected float orbitPathOffsetPercentage;
    protected float orbitEccentricity;
    protected double orbitSpread;
    protected int orbitDirection;
    protected double spinRotationSpeed;
    protected double orbitRadius;
    protected float yOffset;

    public virtual void Destroy() {
        if (objectHolder != null) {
            objectHolder.keepAfterAbandon = false;
            objectHolder.Destroy();
        }

        if (components != null) for (int i = 0; i < components.Length; i++) components[i]?.Destroy();
        if (transforms != null) {
            for (int i = 0; i < transforms.Length; i++) {
                if (transforms[i] == null || transforms[i].gameObject == null) continue;
                
                UnityEngine.Object.Destroy(transforms[i].gameObject);
                transforms[i] = null;
            }
        }
    }

    protected virtual void SetActive(bool active) {
        for (int i = 0; i < transforms.Length; i++) transforms[i].gameObject.SetActive(active);
        for (int i = 0; i < components.Length; i++) components[i].SetActive(active);
    }

    public virtual void GetExposeData(List<string> defNames, List<int?> seeds, List<int?> ids, List<int?> targetIds, List<Vector3?> positions, List<int?> deathTicks) {
        if (objectHolder != null) return;

        defNames.Add(DEF.defName);
        seeds.Add(seed);
        ids.Add(id);
        targetIds.Add(targetId);
        positions.Add(position);
        deathTicks.Add(deathTick);
    }

    public virtual void Randomize() {
        Init(deathTick: deathTick);
        for (int i = 0; i < components.Length; i++) components[i].UpdateInfo();
    }

    public virtual void Init(int? seed = null, int? id = null, int? targetId = null, Vector3? position = null, int? deathTick = null) {
        this.seed = seed ?? Verse.Rand.Int;
        rand = new Common.Random(this.seed);

        this.id = id ?? Initialization.NextId++;

        this.targetId = targetId ?? -1;

        this.deathTick = deathTick;

        Defs.NamePack namePack = Loader.Defs.NamePacks[DEF.namePackDefName];
        name = $"{rand.GetElement(namePack.prefix)}-{rand.GetElement(namePack.postfix)}";

        period = (int) (36000.0f + (6000.0f * (rand.GetFloat() - 0.5f)));
        timeOffset = rand.GetValueBetween(new Vector2Int(0, period));

        orbitPathOffsetPercentage = DEF.orbitPathOffsetPercentage;
        orbitEccentricity = rand.GetValueBetween(DEF.orbitEccentricityBetween);
        scalePercentage = rand.GetValueBetween(DEF.scalePercentageBetween);
        speedPercentage = rand.GetValueBetween(DEF.speedPercentageBetween);
        orbitSpread = rand.GetValueBetween(DEF.orbitSpreadBetween);

        UpdateScale();
        UpdateSpeed();
        UpdateOrbitRadius();

        orbitDirection = DEF.orbitDirection switch {
            OrbitDirection.Left => -1,
            OrbitDirection.Right => 1,
            OrbitDirection.Random => rand.GetBool() ? 1 : -1,
            _ => 1
        };

        spinRotationSpeed = rand.GetValueBetween(DEF.spinRotationSpeedBetween);
        float axialAngle = rand.GetValueBetween(DEF.axialAngleBetween);
        if (DEF.shape == null && DEF.icon != null) {
            axialRotation = Quaternion.Euler(0, 0, axialAngle);
        } else axialRotation = Quaternion.Euler(0, axialAngle, 0);
        float inclinationAngle = rand.GetValueBetween(DEF.inclinationAngleBetween);
        if (inclinationAngle != 0) inclinationRotation = rand.GetRotation() * Quaternion.Euler(rand.GetValueBetween(DEF.inclinationAngleBetween), 0, 0);

        if (position != null) {
            this.position = (Vector3) position;
        } else {
            UpdatePosition(tick: 0);
            this.position.y = rand.GetValueBetween(DEF.yOffsetBetween);
        }

        Game.MainLoop.instance.forceUpdate = true;
    }

    public virtual void Tick() {
        if (ShouldDespawn()) Game.MainLoop.instance.dirtyCache = true;
    }

    public virtual void Update() {
        if (Game.MainLoop.instance.unpaused || Game.MainLoop.instance.forceUpdate) UpdatePosition(Game.MainLoop.instance.tick);
        UpdateRotation(Game.MainLoop.instance.tick);
        UpdateTransformationMatrix();

        for (int i = 0; i < components.Length; i++) {
            components[i].Update();
            if (Game.MainLoop.instance.worldSceneActivated) components[i].OnWorldSceneActivated();
            if (Game.MainLoop.instance.worldSceneDeactivated) components[i].OnWorldSceneDeactivated();
        }

        objectHolder?.CheckHideIcon();
    }

    protected virtual void UpdatePosition(int tick) {
        positionChanged = true;

        double time = speed * tick + timeOffset;
        double angularFrequencyTime = 6.28 / period * time;

        position.x = (float) (orbitDirection * orbitRadius * Math.Cos(angularFrequencyTime));
        position.z = (float) (orbitDirection * orbitRadius * Math.Sqrt(1 - orbitEccentricity * orbitEccentricity) * Math.Sin(angularFrequencyTime));

        _localPosition = (inclinationRotation * position) + GetTargetPosition();
    }

    protected virtual void UpdateRotation(int tick) {
        rotationChanged = true;

        if (addBillboardRotation) {
            billboardRotation = Quaternion.LookRotation(Game.MainLoop.instance.cameraForward, Vector3.up);
            rotation = billboardRotation * axialRotation;
        } else {
            double num1 = spinRotationSpeed * tick * orbitDirection * -1 * (Math.PI / 180);
            Vector3 euler = new Vector3(
                (float) (0.5 * num1),
                (float) num1,
                0.0f
            );
            spinRotation = Quaternion.Euler(euler);

            rotation = axialRotation * spinRotation;
        }
    }

    protected virtual void UpdateTransformationMatrix() {
        transformationMatrix.SetTRS(_localPosition, rotation, scale);
        // update real position
        transformedPosition.x = transformationMatrix.m03;
        transformedPosition.y = transformationMatrix.m13;
        transformedPosition.z = transformationMatrix.m23;
    }

    public virtual void Render(bool blockRendering) {
        switch (blockRendering) {
            case true when !this.blockRendering:
                this.blockRendering = true;
                SetActive(false);
                break;
            case false when this.blockRendering:
                this.blockRendering = false;
                SetActive(true);
                break;
        }

        if (dirty) Recache();

        bool updatePosition = positionChanged || Game.MainLoop.instance.forceUpdate;
        bool updateRotation = rotationChanged || Game.MainLoop.instance.forceUpdate;
        bool updateScale = scaleChanged || Game.MainLoop.instance.forceUpdate;

        positionChanged = false;
        rotationChanged = false;
        scaleChanged = false;

        foreach (ObjectComponent component in components) component.Render();

        int totalTransforms = transforms.Length;
        if (totalTransforms <= 0) return;

        for (int i = 0; i < totalTransforms; i++) {
            if (updatePosition) transforms[i].position = _localPosition;
            if (updateRotation) transforms[i].rotation = rotation;
            if (updateScale) transforms[i].localScale = scale;
        }
    }

    protected virtual void Recache() {
        dirty = false;
        if ((generatingVisuals || shape == null) && (generatingVisuals || !icon)) return;
        // generate game object for shape
        if (shape != null) {
            Mesh[] meshes = (Mesh[]) shape.GetMeshes().Clone();
            Material[] materials = (Material[]) shape.GetMaterials().Clone();
            extraScale = shape.HighestElevation;
            shape = null;

            transforms = new Transform[meshes.Length];
            for (int i = 0; i < meshes.Length; i++) {
                GameObject newGameObject = new GameObject {
                    layer = RimWorld.Planet.WorldCameraManager.WorldLayer
                };
                UnityEngine.Object.DontDestroyOnLoad(newGameObject);

                newGameObject.SetActive(false);

                MeshFilter meshFilter = newGameObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = newGameObject.AddComponent<MeshRenderer>();

                meshFilter.mesh = meshes[i];
                meshRenderer.material = materials[i];

                transforms[i] = newGameObject.transform;
            }
        }
        // generate game object for icon
        if (icon) {
            icon = false;

            GameObject newGameObject = new GameObject {
                layer = RimWorld.Planet.WorldCameraManager.WorldLayer
            };
            UnityEngine.Object.DontDestroyOnLoad(newGameObject);

            SpriteRenderer spriteRenderer = newGameObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));

            spriteRenderer.material.renderQueue = RimWorld.Planet.WorldMaterials.WorldObjectRenderQueue;

            transforms = new Transform[1];
            transforms[0] = newGameObject.transform;

        }
        // generate components
        List<ObjectComponent> objectComponents = new List<ObjectComponent>();
        foreach (var componentDef in DEF.components) {
            ObjectComponent component = (ObjectComponent) Activator.CreateInstance(
                componentDef.componentClass,
                [this, componentDef]
            );
            objectComponents.Add(component);
        }

        components = objectComponents.ToArray();

        if (!blockRendering) for (int i = 0; i < transforms.Length; i++) transforms[i].gameObject.SetActive(true);

        Game.MainLoop.instance.forceUpdate = true;
    }

    public virtual void SetTarget(CelestialObject target) {
        this.target = target;

        if (this.target != null) {
            targetId = this.target.id;
        } else {
            targetId = -1;
        }

        UpdateScale();
        UpdateOrbitRadius();
        UpdateSpeed();
    }

    public virtual void FindTarget(List<CelestialObject> celestialObjects) {
        bool notTargeting = targetId == -1;
        if (notTargeting) return;

        int numCelestialObjects = celestialObjects.Count;
        for (int i = 0; i < numCelestialObjects; i++) {
            bool foundTarget = celestialObjects[i].id == targetId;
            if (!foundTarget) continue;
            
            SetTarget(celestialObjects[i]);
            return;
        }
    }

    protected virtual void UpdateOrbitRadius() {
        Vector3 scaledOrbitOffset = GetTargetScale() * orbitPathOffsetPercentage;
        orbitRadius = scaledOrbitOffset.x + ((rand.GetFloat() - 0.5) * (scaledOrbitOffset.x * orbitSpread));
    }

    protected virtual void UpdateScale() {
        scaleChanged = true;

        scale = GetTargetScale() * scalePercentage;

        if (DEF.minSize != null && scale.x < (float) DEF.minSize) scale = new Vector3((float) DEF.minSize, (float) DEF.minSize, (float) DEF.minSize);
    }

    protected virtual void UpdateSpeed() {
        speed = GetTargetSpeed() * speedPercentage;
    }

    protected virtual Vector3 GetTargetPosition() {
        return target?.transformedPosition ?? Vector3.zero;
    }

    protected virtual double GetTargetSpeed() {
        return target?.speed ?? 0.8;
    }

    protected virtual Vector3 GetTargetScale() {
        return target?.scale ?? new Vector3(100.0f, 100.0f, 100.0f);
    }

    public virtual bool ShouldDespawn() {
        if (forceDeath) return true;
        if (objectHolder != null && !objectHolder.SafeDespawn()) return false;
        return deathTick != null && Game.MainLoop.instance.tick > deathTick;
    }

    public virtual void GenerateVisuals() {
        if (DEF.shape != null) {
            GenerateShape();
        } else if (DEF.icon != null) {
            GenerateIcon();
        } else {
            Debugger.Log(
                key: "Universum.Error.no_shape_icon",
                prefix: $"{Manager.METADATA.Name}: ",
                args: [DEF.defName],
                severity: Debugger.Severity.Error
            );
        }
    }

    protected virtual void GenerateShape() {
        generatingVisuals = true;

        shape = new Common.Shape(rand);

        foreach (Defs.Mesh mesh in DEF.shape.meshes) {
            List<bool> isMask = new List<bool>();
            List<bool> useMask = new List<bool>();
            List<float> noiseStrength = new List<float>();
            List<float> noiseRoughness = new List<float>();
            List<int> noiseIterations = new List<int>();
            List<float> noisePersistence = new List<float>();
            List<float> noiseBaseRoughness = new List<float>();
            List<float> noiseMinValue = new List<float>();

            foreach (Defs.Mesh.Noise noiseLayer in mesh.noiseLayers) {
                isMask.Add(noiseLayer.isMask);
                useMask.Add(noiseLayer.useMask);
                noiseStrength.Add(rand.GetValueBetween(noiseLayer.strengthBetween));
                noiseRoughness.Add(rand.GetValueBetween(noiseLayer.roughnessBetween));
                noiseIterations.Add(rand.GetValueBetween(new Vector2Int((int) Math.Abs(noiseLayer.iterationsBetween.x), (int) Math.Abs(noiseLayer.iterationsBetween.y))));
                noisePersistence.Add(rand.GetValueBetween(noiseLayer.persistenceBetween));
                noiseBaseRoughness.Add(rand.GetValueBetween(noiseLayer.baseRoughnessBetween));
                noiseMinValue.Add(rand.GetValueBetween(noiseLayer.minNoiseValueBetween));
            }

            int detail = rand.GetValueBetween(new Vector2Int((int) Math.Abs(mesh.detailBetween.x), (int) Math.Abs(mesh.detailBetween.y)));

            shape.Add(
                Loader.Assets.Materials[mesh.materialDefName],
                mesh.type,
                mesh.subdivisionIterations,
                detail,
                mesh.radius,
                mesh.dimensions,
                mesh.minElevationColor,
                mesh.maxElevationColor,
                mesh.craterDepth,
                mesh.craterRimHeight,
                isMask,
                useMask,
                noiseStrength,
                noiseRoughness,
                noiseIterations,
                noisePersistence,
                noiseBaseRoughness,
                noiseMinValue
            );
        }

        shape.CompressData();

        generatingVisuals = false;
        dirty = true;
    }

    protected virtual void GenerateIcon() {
        generatingVisuals = true;
        icon = true;

        iconTexture = Loader.Assets.GetTexture(DEF.icon.texturePath);
        addBillboardRotation = true;

        generatingVisuals = false;
        dirty = true;
    }
}
