﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int metallicId = Shader.PropertyToID("_Metallic");
    static int smoothnessId = Shader.PropertyToID("_Smoothness");

    [SerializeField]
    Mesh mesh = default;

    [SerializeField]
    Material material = default;

    [SerializeField]
    LightProbeProxyVolume lightProbeVolume = null;

    Matrix4x4[] matrices = new Matrix4x4[1023];
    Vector4[] baseColors = new Vector4[1023];

    float[] metallic = new float[1023];
    float[] smoothness = new float[1023];

    MaterialPropertyBlock block;

    private void Awake()
    {
        for (int i = 0; i < matrices.Length; ++i)
        {
            matrices[i] = Matrix4x4.TRS(transform.position + Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one);
            baseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
            metallic[i] = Random.value < 0.25f ? 1f : 0f;
            smoothness[i] = Random.Range(0.05f, 0.95f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
            block.SetVectorArray(baseColorId, baseColors);
            block.SetFloatArray(metallicId, metallic) ;
            block.SetFloatArray(smoothnessId, smoothness);

            if (lightProbeVolume != null)
            {
                var positions = new Vector3[1023];
                for (int i = 0; i < matrices.Length; ++i)
                {
                    positions[i] = matrices[i].GetColumn(3);
                }

                var lightProbes = new SphericalHarmonicsL2[1023];
                LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, lightProbes, null);
                block.CopySHCoefficientArraysFrom(lightProbes);
            }
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, 1023, block,
            ShadowCastingMode.On, true, 0, null,
            lightProbeVolume != null ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided,
            lightProbeVolume);
    }
}
