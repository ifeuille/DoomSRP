using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DoomSRP
{
    public class AtlasShadowsPass : ScriptableRenderPass
    {
        private static class ShadowsConstantBuffer
        {
            public static int _ShadowAtlasResolution;
            public static int _ShadowsParms;
            public static int _LightsShadowmapTexture;
        }

        const int k_ShadowmapBufferBits = 16;
        //private RenderTargetHandle m_LightsShadowmap;
        RenderTexture m_LightsShadowmapTexture;
        RenderTextureFormat m_ShadowmapFormat;
        int m_ShadowmapWidth;
        int m_ShadowmapHeight;

        const string m_ProfilerTag = "Render Shadows";
        bool m_SupportsBoxFilterForShadows;
        private RenderTargetHandle destination { get; set; }

        BetterList<int> m_ShadowCastingLightIndices = new BetterList<int>();
        
        public AtlasShadowsPass()
        {
            ShadowsConstantBuffer._ShadowAtlasResolution = Shader.PropertyToID("_ShadowAtlasResolution");
            ShadowsConstantBuffer._LightsShadowmapTexture = Shader.PropertyToID("_LightsShadowmapTexture");
            ShadowsConstantBuffer._ShadowsParms = Shader.PropertyToID("_ShadowsParms");

            RegisterShaderPassName("ShadowCaster");
                       
            m_ShadowmapFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap)
            ? RenderTextureFormat.Shadowmap
            : RenderTextureFormat.Depth;

            m_SupportsBoxFilterForShadows = Application.isMobilePlatform || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Switch;


        }

        public bool Setup(RenderTargetHandle destination, ref RenderingData renderingData, int maxVisibleShaodwLights)
        {
            Clear();
            this.destination = destination;

            m_ShadowmapWidth = renderingData.shadowData.lightsShadowmapWidth;
            m_ShadowmapHeight = renderingData.shadowData.lightsShadowmapHeight;
            var visibleLights = renderingData.lightData.visibleLights;
            m_ShadowCastingLightIndices.Clear();

            int lightsCount = renderingData.lightData.lightLoop.VisibleLight;
            int maxShadowLightsNum = renderingData.settings.MaxShadowLightsNum;
            Bounds bounds;
            for(int i = 0; i < visibleLights.Count 
                && m_ShadowCastingLightIndices.size < lightsCount
                && m_ShadowCastingLightIndices.size < maxShadowLightsNum; ++i)
            {

            }



            return false;
        }

        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            throw new NotImplementedException();
        }
        
        void Clear()
        {
            m_LightsShadowmapTexture = null;

            for (int i = 0; i < m_LightShadowMatrices.Length; ++i)
                m_LightShadowMatrices[i] = Matrix4x4.identity;

            for (int i = 0; i < m_LightSlices.Length; ++i)
                m_LightSlices[i].Clear();

            for (int i = 0; i < m_LightsShadowStrength.Length; ++i)
                m_LightsShadowStrength[i] = 0.0f;
        }
    }
}
