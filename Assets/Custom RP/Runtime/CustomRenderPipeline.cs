﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();
    bool useDynamicBatching;
    bool useGPUInstancing;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatching)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;

        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach(Camera camera in cameras)
        {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
        }
    }
}
