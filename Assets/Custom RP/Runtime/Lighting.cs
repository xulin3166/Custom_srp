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
    static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];

    const int maxOtherLightCount = 64;
    static int otherLightCountId = Shader.PropertyToID("_OtherLightCount");
    static int otherLightColorsId = Shader.PropertyToID("_OtherLightColors");
    static int otherLightPositionsId = Shader.PropertyToID("_OtherLightPositions");
    static int otherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections");
    static int otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles");

    static Vector4[] otherLightColors = new Vector4[maxOtherLightCount];
    static Vector4[] otherLightPositions = new Vector4[maxOtherLightCount];
    static Vector4[] otherLightDirections = new Vector4[maxOtherLightCount];
    static Vector4[] otherLightSpotAngles = new Vector4[maxOtherLightCount];

    CullingResults cullingResults;
    Shadows shadows = new Shadows();

    public void Setup (ScriptableRenderContext context
        , CullingResults cullingResults
        , ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        //SetupDirectionalLight();
        shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        shadows.Render();
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
        int nOtherLightCount = 0;
        for (int i = 0; i < visibleLights.Length; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];
            switch(visibleLight.lightType)
            {
                case LightType.Directional:
                    if (nDirLightCount < maxDirLightCount)
                        SetupDirectionalLight(nDirLightCount++, ref visibleLight);
                    break;
                case LightType.Point:
                    if (nOtherLightCount < maxOtherLightCount)
                        SetupPointLight(nOtherLightCount++, ref visibleLight);
                    break;
                case LightType.Spot:
                    if (nOtherLightCount < maxOtherLightCount)
                        SetupSpotLight(nOtherLightCount++, ref visibleLight);
                    break;
            }
        }

        buffer.SetGlobalInt(dirLightCountId, nDirLightCount);
        if (nDirLightCount > 0)
        {
            buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
            buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }

        buffer.SetGlobalInt(otherLightCountId, nOtherLightCount);
        if (nOtherLightCount > 0)
        {
            buffer.SetGlobalVectorArray(otherLightColorsId, otherLightColors);
            buffer.SetGlobalVectorArray(otherLightPositionsId, otherLightPositions);
            buffer.SetGlobalVectorArray(otherLightDirectionsId, otherLightDirections);
            buffer.SetGlobalVectorArray(otherLightSpotAnglesId, otherLightSpotAngles);
        }
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    void SetupPointLight(int index, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;

        otherLightSpotAngles[index] = new Vector4(0f, 1f);
    }

    void SetupSpotLight(int index, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor;
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;
        otherLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);

        Light light = visibleLight.light;
        float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
        float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
        float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
        otherLightSpotAngles[index] = new Vector4(angleRangeInv, -outerCos * angleRangeInv);
    }

    public void Cleanup()
    {
        shadows.Cleanup();
    }
}
