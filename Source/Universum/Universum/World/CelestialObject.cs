﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Universum.World {
    public abstract class CelestialObject {
        public int seed;
        public int id = -1;
        public string name;
        public Defs.CelestialObject def;

        public ObjectHolder objectHolder = null;

        protected Functionality.Random _rand;

        protected bool _blockRendering = false;
        protected bool _generatingVisuals = false;
        protected bool _dirty = false;
        protected bool _positionChanged = true;
        protected bool _rotationChanged = true;
        protected bool _scaleChanged = true;

        public int? deathTick = null;
        public bool forceDeath = false;

        protected Shape _shape = null;
        protected bool _icon = false;

        protected Texture2D _iconTexture = null;
        protected Matrix4x4 _transformationMatrix = Matrix4x4.identity;
        protected Quaternion _rotation = Quaternion.identity;
        protected bool _addBillboardRotation = false;
        protected Quaternion _billboardRotation = Quaternion.identity;
        protected Quaternion _axialRotation = Quaternion.identity;
        protected Quaternion _spinRotation = Quaternion.identity;
        protected Quaternion _inclinatioRotation = Quaternion.identity;
        protected CelestialObject _target;
        public int targetId = -1;

        protected Transform[] _transforms = new Transform[0];
        protected ObjectComponent[] _components = new ObjectComponent[0];

        public Vector3 transformedPosition;
        public Vector3 position;
        private Vector3 _localPosition;
        public Vector3 scale;
        protected float _scalePercentage;
        public float extraScale;
        public double speed;
        protected double _speedPercentage;
        protected int _period;
        protected int _timeOffset;
        protected float _orbitPathOffsetPercentage;
        protected float _orbitEccentricity;
        protected double _orbitSpread;
        protected int _orbitDirection;
        protected double _spinRotationSpeed;
        protected double _orbitRadius;
        protected float _yOffset;

        public CelestialObject(string celestialObjectDefName) {
            def = Defs.Loader.celestialObjects[celestialObjectDefName];
        }

        public virtual void Destroy() {
            if (objectHolder != null) {
                objectHolder.keepAfterAbandon = false;
                objectHolder.Destroy();
            }

            if (_components != null) for (int i = 0; i < _components.Length; i++) _components[i]?.Destroy();
            if (_transforms != null) {
                for (int i = 0; i < _transforms.Length; i++) {
                    if (_transforms[i] != null && _transforms[i].gameObject != null) {
                        UnityEngine.Object.Destroy(_transforms[i].gameObject);
                        _transforms[i] = null;
                    }
                }
            }
        }

        public virtual void SetActive(bool active) {
            for (int i = 0; i < _transforms.Length; i++) _transforms[i].gameObject.SetActive(active);
            for (int i = 0; i < _components.Length; i++) _components[i].SetActive(active);
        }

        public virtual void GetExposeData(List<string> defNames, List<int?> seeds, List<int?> ids, List<int?> targetIds, List<Vector3?> positions, List<int?> deathTicks) {
            if (objectHolder != null) return;

            defNames.Add(def.defName);
            seeds.Add(seed);
            ids.Add(id);
            targetIds.Add(targetId);
            positions.Add(position);
            deathTicks.Add(deathTick);
        }

        public virtual void Randomize() {
            Init(deathTick: deathTick);
            for (int i = 0; i < _components.Length; i++) _components[i].UpdateInfo();
        }

        public virtual void Init(int? seed = null, int? id = null, int? targetId = null, Vector3? position = null, int? deathTick = null) {
            this.seed = seed ?? Rand.Int;
            _rand = new Functionality.Random(this.seed);

            this.id = id ?? Generator.nextId++;

            this.targetId = targetId ?? -1;

            this.deathTick = deathTick;

            Defs.NamePack namePack = Defs.Loader.namePacks[def.namePackDefName];
            name = $"{_rand.GetElement(namePack.prefix)}-{_rand.GetElement(namePack.postfix)}";

            _period = (int) (36000.0f + (6000.0f * (_rand.GetFloat() - 0.5f)));
            _timeOffset = _rand.GetValueBetween(new Vector2Int(0, _period));

            _orbitPathOffsetPercentage = def.orbitPathOffsetPercentage;
            _orbitEccentricity = _rand.GetValueBetween(def.orbitEccentricityBetween);
            _scalePercentage = _rand.GetValueBetween(def.scalePercentageBetween);
            _speedPercentage = _rand.GetValueBetween(def.speedPercentageBetween);
            _orbitSpread = _rand.GetValueBetween(def.orbitSpreadBetween);

            UpdateScale();
            UpdateSpeed();
            UpdateOrbitRadius();

            switch (def.orbitDirection) {
                case Defs.OrbitDirection.LEFT:
                    _orbitDirection = -1;
                    break;
                case Defs.OrbitDirection.RIGHT:
                    _orbitDirection = 1;
                    break;
                case Defs.OrbitDirection.RANDOM:
                    _orbitDirection = _rand.GetBool() ? 1 : -1;
                    break;
                default:
                    _orbitDirection = 1;
                    break;
            }

            _spinRotationSpeed = _rand.GetValueBetween(def.spinRotationSpeedBetween);
            float axialAngle = _rand.GetValueBetween(def.axialAngleBetween);
            if (def.shape == null && def.icon != null) {
                _axialRotation = Quaternion.Euler(0, 0, axialAngle);
            } else _axialRotation = Quaternion.Euler(0, axialAngle, 0);
            float inclinationAngle = _rand.GetValueBetween(def.inclinationAngleBetween);
            if (inclinationAngle != 0) _inclinatioRotation = _rand.GetRotation() * Quaternion.Euler(_rand.GetValueBetween(def.inclinationAngleBetween), 0, 0);

            if (position != null) {
                this.position = (Vector3) position;
            } else {
                UpdatePosition(tick: 0);
                this.position.y = _rand.GetValueBetween(def.yOffsetBetween);
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

            for (int i = 0; i < _components.Length; i++) {
                _components[i].Update();
                if (Game.MainLoop.instance.worldSceneActivated) _components[i].OnWorldSceneActivated();
                if (Game.MainLoop.instance.worldSceneDeactivated) _components[i].OnWorldSceneDeactivated();
            }

            objectHolder?.CheckHideIcon();
        }

        public virtual void UpdatePosition(int tick) {
            _positionChanged = true;

            double time = speed * tick + _timeOffset;
            double angularFrequencyTime = 6.28 / _period * time;

            position.x = (float) (_orbitDirection * _orbitRadius * Math.Cos(angularFrequencyTime));
            position.z = (float) (_orbitDirection * _orbitRadius * Math.Sqrt(1 - _orbitEccentricity * _orbitEccentricity) * Math.Sin(angularFrequencyTime));

            _localPosition = (_inclinatioRotation * position) + GetTargetPosition();
        }

        public virtual void UpdateRotation(int tick) {
            _rotationChanged = true;

            if (_addBillboardRotation) {
                _billboardRotation = Utils.billboardRotation();
                _rotation = _billboardRotation * _axialRotation;
            } else {
                double num1 = _spinRotationSpeed * tick * _orbitDirection * -1 * (Math.PI / 180);
                Vector3 euler = new Vector3(
                    (float) (0.5 * num1),
                    (float) num1,
                    0.0f
                );
                _spinRotation = Quaternion.Internal_FromEulerRad(euler);

                _rotation = _axialRotation * _spinRotation;
            }
        }

        public virtual void UpdateTransformationMatrix() {
            _transformationMatrix.SetTRS(_localPosition, _rotation, scale);
            // update real position
            transformedPosition.x = _transformationMatrix.m03;
            transformedPosition.y = _transformationMatrix.m13;
            transformedPosition.z = _transformationMatrix.m23;
        }

        public virtual void Render(bool blockRendering) {
            if (blockRendering && !_blockRendering) {
                _blockRendering = true;
                SetActive(false);
            }
            if (!blockRendering && _blockRendering) {
                _blockRendering = false;
                SetActive(true);
            }
            if (_dirty) _Recache();

            bool updatePosition = _positionChanged || Game.MainLoop.instance.forceUpdate;
            bool updateRotation = _rotationChanged || Game.MainLoop.instance.forceUpdate;
            bool updateScale = _scaleChanged || Game.MainLoop.instance.forceUpdate;

            _positionChanged = false;
            _rotationChanged = false;
            _scaleChanged = false;

            foreach (ObjectComponent component in _components) component.Render();

            int totalTransforms = _transforms.Length;
            if (totalTransforms <= 0) return;

            for (int i = 0; i < totalTransforms; i++) {
                if (updatePosition) _transforms[i].position = _localPosition;
                if (updateRotation) _transforms[i].rotation = _rotation;
                if (updateScale) _transforms[i].localScale = scale;
            }
        }

        protected virtual void _Recache() {
            _dirty = false;
            if ((_generatingVisuals || _shape == null) && (_generatingVisuals || !_icon)) return;
            // generate game object for shape
            if (_shape != null) {
                Mesh[] meshes = (Mesh[]) _shape.GetMeshes().Clone();
                Material[] materials = (Material[]) _shape.GetMaterials().Clone();
                extraScale = _shape.highestElevation;
                _shape = null;

                _transforms = new Transform[meshes.Length];
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

                    _transforms[i] = newGameObject.transform;
                }
            }
            // generate game object for icon
            if (_icon) {
                _icon = false;

                GameObject newGameObject = new GameObject {
                    layer = RimWorld.Planet.WorldCameraManager.WorldLayer
                };
                UnityEngine.Object.DontDestroyOnLoad(newGameObject);

                SpriteRenderer spriteRenderer = newGameObject.AddComponent<SpriteRenderer>();

                spriteRenderer.sprite = Sprite.Create(_iconTexture, new Rect(0, 0, _iconTexture.width, _iconTexture.height), new Vector2(0.5f, 0.5f));

                spriteRenderer.material.renderQueue = RimWorld.Planet.WorldMaterials.WorldObjectRenderQueue;

                _transforms = new Transform[1];
                _transforms[0] = newGameObject.transform;

            }
            // generate components
            List<ObjectComponent> objectComponents = new List<ObjectComponent>();
            foreach (var componentDef in def.components) {
                ObjectComponent component = (ObjectComponent) Activator.CreateInstance(
                    componentDef.componentClass,
                    new object[] { this, componentDef }
                );
                objectComponents.Add(component);
            }

            _components = objectComponents.ToArray();

            if (!_blockRendering) for (int i = 0; i < _transforms.Length; i++) _transforms[i].gameObject.SetActive(true);

            Game.MainLoop.instance.forceUpdate = true;
        }

        public virtual void SetTarget(CelestialObject target) {
            _target = target;

            if (_target != null) {
                targetId = _target.id;
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
            bool foundTarget;
            for (int i = 0; i < numCelestialObjects; i++) {
                foundTarget = celestialObjects[i].id == targetId;
                if (foundTarget) {
                    SetTarget(celestialObjects[i]);

                    return;
                }
            }
        }

        public virtual void UpdateOrbitRadius() {
            Vector3 scaledOrbitOffset = GetTargetScale() * _orbitPathOffsetPercentage;
            _orbitRadius = scaledOrbitOffset.x + ((_rand.GetFloat() - 0.5) * (scaledOrbitOffset.x * _orbitSpread));
        }

        public virtual void UpdateScale() {
            _scaleChanged = true;

            scale = GetTargetScale() * _scalePercentage;

            if (def.minSize != null && scale.x < (float) def.minSize) scale = new Vector3((float) def.minSize, (float) def.minSize, (float) def.minSize);
        }

        public virtual void UpdateSpeed() {
            speed = GetTargetSpeed() * _speedPercentage;
        }

        public virtual Vector3 GetTargetPosition() {
            return _target?.transformedPosition ?? Vector3.zero;
        }

        public virtual double GetTargetSpeed() {
            return _target?.speed ?? 0.8;
        }

        public virtual Vector3 GetTargetScale() {
            return _target?.scale ?? new Vector3(100.0f, 100.0f, 100.0f);
        }

        public virtual bool ShouldDespawn() {
            if (forceDeath) return true;
            if (objectHolder != null && !objectHolder.SafeDespawn()) return false;
            return deathTick != null && Game.MainLoop.instance.tick > deathTick;
        }

        public virtual void GenerateVisuals() {
            if (def.shape != null) {
                _GenerateShape();
            } else if (def.icon != null) {
                _GenerateIcon();
            } else {
                Logger.print(
                    Logger.Importance.Error,
                    key: "Universum.Error.no_shape_icon",
                    prefix: Style.name_prefix,
                    args: new NamedArgument[] { def.defName }
                );
                return;
            }
        }

        protected virtual void _GenerateShape() {
            _generatingVisuals = true;

            _shape = new Shape(_rand);

            foreach (Defs.Mesh mesh in def.shape.meshes) {
                List<bool> isMask = new List<bool>();
                List<bool> useMask = new List<bool>();
                List<float> noiseStrength = new List<float>();
                List<float> noiseRoughness = new List<float>();
                List<int> noiseIterations = new List<int>();
                List<float> noisePersistence = new List<float>();
                List<float> noiseBaseRoughness = new List<float>();
                List<float> noiseMinValue = new List<float>();

                foreach (Defs.Noise noiseLayer in mesh.noiseLayers) {
                    isMask.Add(noiseLayer.isMask);
                    useMask.Add(noiseLayer.useMask);
                    noiseStrength.Add(_rand.GetValueBetween(noiseLayer.strenghtBetween));
                    noiseRoughness.Add(_rand.GetValueBetween(noiseLayer.roughnessBetween));
                    noiseIterations.Add(_rand.GetValueBetween(new Vector2Int((int) Math.Abs(noiseLayer.iterationsBetween.x), (int) Math.Abs(noiseLayer.iterationsBetween.y))));
                    noisePersistence.Add(_rand.GetValueBetween(noiseLayer.persistenceBetween));
                    noiseBaseRoughness.Add(_rand.GetValueBetween(noiseLayer.baseRoughnessBetween));
                    noiseMinValue.Add(_rand.GetValueBetween(noiseLayer.minNoiseValueBetween));
                }

                int detail = _rand.GetValueBetween(new Vector2Int((int) Math.Abs(mesh.detailBetween.x), (int) Math.Abs(mesh.detailBetween.y)));

                _shape.Add(
                    Assets.materials[mesh.materialDefName],
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

            _shape.CompressData();

            _generatingVisuals = false;
            _dirty = true;
        }

        protected virtual void _GenerateIcon() {
            _generatingVisuals = true;
            _icon = true;

            _iconTexture = Assets.GetTexture(def.icon.texturePath);
            _addBillboardRotation = true;

            _generatingVisuals = false;
            _dirty = true;
        }
    }
}
