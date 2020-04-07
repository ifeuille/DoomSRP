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

        }

        const int k_ShadowmapBufferBits = 16;
        RenderTexture m_LightsShadowmapTexture;
        RenderTextureFormat m_ShadowmapFormat;

        private RenderTargetHandle destination { get; set; }

        public AtlasShadowsPass()
        {
            RegisterShaderPassName("ShadowCaster");

            m_ShadowmapFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap)
            ? RenderTextureFormat.Shadowmap
            : RenderTextureFormat.Depth;
        }
        public bool Setup(RenderTargetHandle destination, ref RenderingData renderingData, int maxVisibleShaodwLights)
        {
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
