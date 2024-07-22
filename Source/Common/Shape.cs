using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Universum.Common;

public class Shape(Random rand) {
    public enum Type {
        Prev = 0,
        Sphere = 1,
        QuadSphere = 2,
        Box = 3,
        Voronoi = 4,
        Torus = 5,
        Plane = 6
    }

    public float HighestElevation { get; private set; }
    private List<Mesh> _meshes = [];
    private List<Material> _materials = [];

    public void CompressData() {
        if (_meshes.Count <= 1) return;

        Dictionary<Material, Mesh> meshMaterialMap = new Dictionary<Material, Mesh>();
        for (int i = 0; i < _meshes.Count; i++) {
            if (meshMaterialMap.TryGetValue(_materials[i], out var mesh)) {
                mesh.Merge(_meshes[i]);
            } else meshMaterialMap[_materials[i]] = _meshes[i];
        }

        _materials = meshMaterialMap.Keys.ToList();
        _meshes = meshMaterialMap.Values.ToList();
    }

    public UnityEngine.Mesh[] GetMeshes() {
        UnityEngine.Mesh[] meshes = new UnityEngine.Mesh[_meshes.Count];
        for (int i = 0; i < meshes.Length; i++) meshes[i] = _meshes[i].GetUnityMesh();
        return meshes;
    }

    public Material[] GetMaterials() {
        return _materials.ToArray();
    }

    public void Add(
        Material material,
        Type type,
        int subdivisionIterations,
        int detail,
        float radius,
        Vector3 dimensions,
        Color? minElevationColor,
        Color? maxElevationColor,
        float craterDepth,
        float craterRimHeight,
        List<bool> isMask,
        List<bool> useMask,
        List<float> noiseStrength,
        List<float> noiseRoughness,
        List<int> noiseIterations,
        List<float> noisePersistence,
        List<float> noiseBaseRoughness,
        List<float> noiseMinValue
    ) {
        Mesh mesh = new Mesh(rand);
        
        switch (type) {
            case Type.Prev:
                mesh = _meshes[_meshes.Count - 1];
                mesh.Subdivide(subdivisionIterations);
                _materials[_materials.Count - 1] = material;
                break;
            case Type.Sphere:
                mesh.GenerateIcoSphere(radius, detail);
                _meshes.Add(mesh);
                _materials.Add(material);
                break;
            case Type.QuadSphere:
                mesh.GenerateQuadSphere(radius, detail);
                _meshes.Add(mesh);
                _materials.Add(material);
                break;
            case Type.Box:
                mesh.GenerateBox(dimensions, detail);
                _meshes.Add(mesh);
                _materials.Add(material);
                break;
            case Type.Voronoi:
                minElevationColor ??= Color.white;
                maxElevationColor ??= Color.white;
                
                mesh = _meshes[_meshes.Count - 1];
                mesh.ApplyVoronoiPattern(
                    siteCount: detail,
                    craterDepth,
                    craterRimHeight,
                    (Color) minElevationColor,
                    (Color) maxElevationColor
                );
                _materials[_materials.Count - 1] = material;
                break;
            case Type.Torus:
                minElevationColor ??= Color.white;
                
                mesh.GenerateTorus(
                    radius: 1.0f,
                    tubeRadius: radius,
                    thickness: 0.001f,
                    radialSegments: detail,
                    tubularSegments: detail,
                    color: (Color) minElevationColor
                );
                _meshes.Add(mesh);
                _materials.Add(material);
                break;
            case Type.Plane:
                mesh.GeneratePlane(dimensions, subdivisions: subdivisionIterations);
                _meshes.Add(mesh);
                _materials.Add(material);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        mesh.ApplyNoise(
            rand.Seed,
            isMask,
            useMask,
            noiseStrength,
            noiseRoughness,
            noiseIterations,
            noisePersistence,
            noiseBaseRoughness,
            noiseMinValue
        );

        if (minElevationColor != null && maxElevationColor != null && type != Type.Voronoi) {
            mesh.GenerateColors((Color) minElevationColor, (Color) maxElevationColor);
        }

        if (HighestElevation < mesh.MaxElevation) HighestElevation = mesh.MaxElevation;
    }
}
