using System.Collections.Generic;
using UnityEngine;

namespace Universum.Common;

public class Random {
    public readonly int SEED;
    private readonly System.Random RAND;

    public Random(int seed) {
        SEED = seed;
        RAND = new System.Random(SEED);
    }

    public Quaternion GetRotation() => Quaternion.Euler(GetFloat() * 360, GetFloat() * 360, GetFloat() * 360);

    public bool GetBool() {
        return RAND.NextDouble() >= 0.5;
    }

    public float GetFloat() {
        return (float) RAND.NextDouble();
    }

    public float GetValueBetween(Vector2 range) {
        float min = range.x;
        float max = range.y;

        if (Mathf.Approximately(min, max)) return min;

        if (min > max) {
            (min, max) = (max, min);
        }

        return GetFloat() * (max - min) + min;
    }

    public int GetValueBetween(Vector2Int range) {
        int min = range.x;
        int max = range.y;

        if (min == max) return min;

        if (min > max) {
            (min, max) = (max, min);
        }

        return Mathf.Abs(RAND.Next(min, max + 1));
    }

    public T GetElement<T>(List<T> array) {
        return array[GetValueBetween(new Vector2Int(0, array.Count - 1))];
    }
}
