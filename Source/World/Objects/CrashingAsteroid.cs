using System.Diagnostics.CodeAnalysis;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Universum.World.Objects;

public class CrashingAsteroid(string celestialObjectDefName) : CelestialObject(celestialObjectDefName) {
    private double _baseOrbitRadius;
    private double _radiusChangePerTick;
    private double _targetOrbitRadius;

    public override void Init(int? seed = null, int? id = null, int? targetId = null, Vector3? position = null, int? deathTick = null) {
        base.Init(seed, id, targetId, position, deathTick);

        _baseOrbitRadius = orbitRadius;
        const double minOrbitRadius = 0.0;
        double maxOrbitRadius = _baseOrbitRadius * 2;

        _targetOrbitRadius = rand.GetBool() ? minOrbitRadius : maxOrbitRadius;

        double minSpeed = DEF.speedPercentageBetween[0];
        double maxSpeed = DEF.speedPercentageBetween[1];

        double normalizedSpeed = (speed - minSpeed) / (maxSpeed - minSpeed);

        const double minRadiusChange = 0.02;
        const double maxRadiusChange = 0.05;
        _radiusChangePerTick = (1 - normalizedSpeed) * minRadiusChange + normalizedSpeed * maxRadiusChange;
    }

    protected override void UpdatePosition(int tick) {
        _AdjustOrbitRadius();

        base.UpdatePosition(tick);
    }

    [SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
    private void _AdjustOrbitRadius() {
        double direction = orbitRadius < _targetOrbitRadius ? 1.0 : -1.0;

        orbitRadius += direction * _radiusChangePerTick;

        if ((direction == 1.0 && orbitRadius > _targetOrbitRadius) ||
            (direction == -1.0 && orbitRadius < _targetOrbitRadius)) {
            _ResetOrbitRadiusAndClearComponents();
        }
    }

    private void _ResetOrbitRadiusAndClearComponents() {
        orbitRadius = _baseOrbitRadius;
        for (int i = 0; i < components.Length; i++) components[i].Clear();
    }
}
