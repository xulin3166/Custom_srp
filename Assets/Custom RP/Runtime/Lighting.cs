using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting 
{
    const string bufferName = "Lighting";

    CommandBuffer buffer = new CommandBuffer() { name = bufferName};

    //static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
    //static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

    const int maxDirLightCount = 4;
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    CullingResults cullingResults;

    public void Setup (ScriptableRenderContext context, CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        //SetupDirectionalLight();
        SetupLights();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    //void SetupDirectionalLight()
    //{
    //    Light light = RenderSettings.sun;
    //    buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
    //    buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
    //}

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int nDirLightCount = 0;
        for (int i = 0; i < visibleLights.Length && nDirLightCount < maxDirLightCount; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
                SetupDirectionalLight(nDirLightCount++, ref visibleLight);
        }

        buffer.SetGlobalInt(dirLightCountId, nDirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}
