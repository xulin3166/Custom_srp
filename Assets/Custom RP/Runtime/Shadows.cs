﻿using UnityEngine.Rendering;
using UnityEngine;

public class Shadows
{
    const string bufferName = "Shadows";
    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    CommandBuffer buffer = new CommandBuffer { 
        name = bufferName
    };

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings settings;

    const int maxShadowDirectionalLightCount = 1;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowDirectionalLightCount];

    int shadowedDirectionalLightCount;

    public void Setup(ScriptableRenderContext context
        , CullingResults cullingResults
        , ShadowSettings shadowSettings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = shadowSettings;

        shadowedDirectionalLightCount = 0;
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowDirectionalLightCount
            && light.shadows != LightShadows.None
            && light.shadowStrength > 0f
            && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount++] =
                new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex};
        }
    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);
        ExecuteBuffer();
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
}
