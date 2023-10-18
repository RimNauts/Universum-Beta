﻿using UnityEngine;

namespace Universum.World {
    public abstract class ObjectComponent {
        protected CelestialObject _celestialObject;
        protected GameObject _gameObject;
        protected bool _active = true;
        protected bool _block = false;

        protected Vector3 _position = Vector3.zero;
        protected Quaternion _rotation = Quaternion.identity;

        protected Vector3 _offset;
        protected float _hideAtMinAltitude;
        protected float _hideAtMaxAltitude;

        public ObjectComponent(CelestialObject celestialObject, Defs.Component def) {
            _celestialObject = celestialObject;

            _gameObject = Object.Instantiate(Assets.gameObjectWorldText);

            _offset = def.offSet;
            _hideAtMinAltitude = def.hideAtMinAltitude;
            _hideAtMaxAltitude = def.hideAtMaxAltitude;
        }

        ~ObjectComponent() {
            Object.Destroy(_gameObject);
        }

        public virtual void OnWorldSceneActivated() {
            
        }

        public virtual void OnWorldSceneDeactivated() {
            
        }

        public virtual void Update() {
            UpdatePosition();
            UpdateRotation();
        }

        public virtual void UpdatePosition() {
            _position = _celestialObject.transformedPosition + _offset;
        }

        public virtual void UpdateRotation() {
            
        }

        public virtual void Render() {
            SetActive(!_block);
            if (!_active) return;
            UpdateTransformationMatrix();
        }

        public virtual void UpdateTransformationMatrix() {
            
        }

        public virtual void SetBlock(bool block) {
            if (_block == block) return;
            _block = block;
        }

        public virtual void SetActive(bool active) {
            if (_active == active) return;
            _active = active;
            _gameObject.SetActive(active);
        }
    }
}
