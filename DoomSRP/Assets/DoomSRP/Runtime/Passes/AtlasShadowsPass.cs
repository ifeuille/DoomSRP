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
            public static int _LightsWorldToShadow;
            public static int _ShadowStrength;
            public static int _ShadowmapSize;
            public static int _ShadowOffset0;
            public static int _ShadowOffset1;
            public static int _ShadowOffset2;
            public static int _ShadowOffset3;
        }

        const int k_ShadowmapBufferBits = 16;
        //private RenderTargetHandle m_LightsShadowmap;
        RenderTexture m_LightsShadowmapTexture;
        RenderTextureFormat m_ShadowmapFormat;
        int m_ShadowmapWidth;
        int m_ShadowmapHeight;

        const string m_ProfilerTag = "Render Shadows";
        const string k_RenderLightShadows = "Render Projector Shadows";
        bool m_SupportsBoxFilterForShadows;
        private RenderTargetHandle destination { get; set; }

        List<int> m_ShadowCastingLightIndices = new List<int>();
        ShadowSliceData[] m_LightSlices;
        float[] m_LightsShadowStrength;
        Matrix4x4[] m_LightShadowMatrices;
        private ComputeBuffer lightShadowParams;

        public AtlasShadowsPass()
        {
            m_LightShadowMatrices = new Matrix4x4[0];
            m_LightSlices = new ShadowSliceData[0];
            m_LightsShadowStrength = new float[0];

            ShadowsConstantBuffer._ShadowAtlasResolution = Shader.PropertyToID("_ShadowAtlasResolution");
            ShadowsConstantBuffer._LightsShadowmapTexture = Shader.PropertyToID("_LightsShadowmapTexture");
            ShadowsConstantBuffer._ShadowsParms = Shader.PropertyToID("_ShadowsParms");
            ShadowsConstantBuffer._LightsWorldToShadow = Shader.PropertyToID("_LightsWorldToShadow");
            ShadowsConstantBuffer._ShadowStrength = Shader.PropertyToID("_ShadowStrength");
            ShadowsConstantBuffer._ShadowmapSize = Shader.PropertyToID("_ShadowmapSize");
            ShadowsConstantBuffer._ShadowOffset0 = Shader.PropertyToID("_ShadowOffset0");
            ShadowsConstantBuffer._ShadowOffset1 = Shader.PropertyToID("_ShadowOffset1");
            ShadowsConstantBuffer._ShadowOffset2 = Shader.PropertyToID("_ShadowOffset2");
            ShadowsConstantBuffer._ShadowOffset3 = Shader.PropertyToID("_ShadowOffset3");

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
            if (m_LightShadowMatrices.Length != maxVisibleShaodwLights)
            {
                m_LightShadowMatrices = new Matrix4x4[maxVisibleShaodwLights];
                m_LightSlices = new ShadowSliceData[maxVisibleShaodwLights];
                m_LightsShadowStrength = new float[maxVisibleShaodwLights];
            }
            m_ShadowCastingLightIndices.Clear();


            m_ShadowmapWidth = renderingData.shadowData.lightsShadowmapWidth;
            m_ShadowmapHeight = renderingData.shadowData.lightsShadowmapHeight;
            //var visibleLights = renderingData.lightData.visibleLights;
            m_ShadowCastingLightIndices.Clear();

            int lightsCount = renderingData.lightData.lightLoop.VisibleLight;
            int maxShadowLightsNum = renderingData.settings.MaxShadowLightsNum;
            var shadowLightDataList = renderingData.lightData.lightLoop.LightsDataForShadow;
            var lightDataList = renderingData.lightData.lightLoop.LightsDataList;


            for(int i = 0; i < shadowLightDataList.size && m_ShadowCastingLightIndices.Count < maxShadowLightsNum; ++i)
            {
                m_ShadowCastingLightIndices.Add(shadowLightDataList[i].lightIndex);
            }

            int shadowCastingLightsCount = m_ShadowCastingLightIndices.Count;
            if (shadowCastingLightsCount == 0) return false;
            int sliceResolution = ShadowUtils.GetMaxTileResolutionInAtlas(m_ShadowmapWidth, m_ShadowmapHeight, shadowCastingLightsCount);
            bool anyShadows = false;
            int shadowSlicesPerRow = (m_ShadowmapWidth / sliceResolution);
            for (int i = 0; i < shadowCastingLightsCount; ++i)
            {
                int shadowLightIndex = m_ShadowCastingLightIndices[i];
                var shadowData = shadowLightDataList[shadowLightIndex];
                
                Matrix4x4 shadowTransform;
                bool success = ShadowUtils.ExtractProjectorLightMatrix(ref shadowData.projectorLight, ref shadowData.shadowData,
                    out shadowTransform);

                if (success)
                {
                    // TODO: We need to pass bias and scale list to shader to be able to support multiple
                    // shadow casting additional lights.
                    m_LightSlices[i].offsetX = (i % shadowSlicesPerRow) * sliceResolution;
                    m_LightSlices[i].offsetY = (i / shadowSlicesPerRow) * sliceResolution;
                    m_LightSlices[i].resolution = sliceResolution;
                    m_LightSlices[i].shadowTransform = shadowTransform;

                    m_LightsShadowStrength[i] = 1;//TODO Shadow strength
                    anyShadows = true;
                }
                else
                {
                    m_ShadowCastingLightIndices.RemoveAt(i--);
                }
            }

            return anyShadows;
        }

        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            lightShadowParams = renderer.lightShadowParams;

            if (renderingData.shadowData.supportsLightShadows)
                RenderProjectorShadowmapAtlas(ref context, ref renderingData.cullResults, ref renderingData.lightData, ref renderingData.shadowData);
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

        void RenderProjectorShadowmapAtlas(ref ScriptableRenderContext context, ref CullResults cullResults, ref LightsData lightData, ref ShadowData shadowData)
        {
            List<VisibleLight> visibleLights = lightData.visibleLights;
            var shadowLightDataList = lightData.lightLoop.LightsDataForShadow;
            var lightDataList = lightData.lightLoop.LightsDataList;

            bool LightHasSoftShadows = false;
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderLightShadows);
            using (new ProfilingSample(cmd, k_RenderLightShadows))
            {
                int shadowmapWidth = shadowData.lightsShadowmapWidth;
                int shadowmapHeight = shadowData.lightsShadowmapHeight;

                m_LightsShadowmapTexture = RenderTexture.GetTemporary(shadowmapWidth, shadowmapHeight,
                    k_ShadowmapBufferBits, m_ShadowmapFormat);
                m_LightsShadowmapTexture.filterMode = FilterMode.Bilinear;
                m_LightsShadowmapTexture.wrapMode = TextureWrapMode.Clamp;

                SetRenderTarget(cmd, m_LightsShadowmapTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                    ClearFlag.Depth, Color.black, TextureDimension.Tex2D);

                for (int i = 0; i < m_ShadowCastingLightIndices.Count; ++i)
                {
                    int shadowLightIndex = m_ShadowCastingLightIndices[i];
                    LightDataForShadow shadowLightData = shadowLightDataList[shadowLightIndex];
                    
                    if (m_ShadowCastingLightIndices.Count > 1)
                        ShadowUtils.ApplySliceTransform(ref m_LightSlices[i], shadowmapWidth, shadowmapHeight);
                    //cullResults 这个cullResults导致shadow必须每帧都算。。
                    //有没有办法静态灯光只算一次，也就是对灯光进行cull,这还涉及到场景管理了
                    // from HDSRP : TODO remove DrawShadowSettings, lightIndex and splitData when scriptable culling is available
                    // lightindex must be right otherwise unity will not render the light's shadow.
                    var settings = new DrawShadowsSettings(cullResults, shadowLightData.unityLightIndex);
                    var pos = shadowLightData.projectorLight.cacheTransform.position;
                    var dis = shadowLightData.projectorLight.iFPipelineProjector.farClipPlane;
                    settings.splitData.cullingSphere = new Vector4(pos.x, pos.y, pos.z, dis);
                    Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLightData.shadowData, shadowLightIndex,
                            ref shadowData, out m_LightSlices[i].projectionMatrix, out m_LightSlices[i].viewMatrix, m_LightSlices[i].resolution);
                    ShadowUtils.SetupShadowCasterConstantBuffer(cmd, ref shadowLightData.visibleLight, shadowBias);//?
                    ShadowUtils.RenderShadowSlice(cmd, ref context, ref m_LightSlices[i],
                        ref settings, m_LightSlices[i].projectionMatrix, m_LightSlices[i].viewMatrix);
                    LightHasSoftShadows |= false;//TODO soft shadow
                }

                SetupLightsShadowReceiverConstants(cmd, ref shadowData);
            }


            bool softShadows = shadowData.supportsSoftShadows;
            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.AdditionalLightShadows, true);
            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, softShadows);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        void SetupLightsShadowReceiverConstants(CommandBuffer cmd, ref ShadowData shadowData)
        {
            for (int i = 0; i < m_LightSlices.Length; ++i)
                m_LightShadowMatrices[i] = m_LightSlices[i].shadowTransform;
            if(lightShadowParams != null)
            {
                lightShadowParams.SetData(m_LightShadowMatrices);
                cmd.SetGlobalBuffer(ShadowsConstantBuffer._LightsWorldToShadow, lightShadowParams);
            }
            float invShadowAtlasWidth = 1.0f / shadowData.lightsShadowmapWidth;
            float invShadowAtlasHeight = 1.0f / shadowData.lightsShadowmapHeight;
            float invHalfShadowAtlasWidth = 0.5f * invShadowAtlasWidth;
            float invHalfShadowAtlasHeight = 0.5f * invShadowAtlasHeight;
            
            cmd.SetGlobalTexture(destination.id, m_LightsShadowmapTexture);
            //cmd.SetGlobalMatrixArray(ShadowsConstantBuffer._LightsWorldToShadow, m_LightShadowMatrices);
            cmd.SetGlobalFloatArray(ShadowsConstantBuffer._ShadowStrength, m_LightsShadowStrength);
            cmd.SetGlobalVector(ShadowsConstantBuffer._ShadowOffset0, new Vector4(-invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(ShadowsConstantBuffer._ShadowOffset1, new Vector4(invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(ShadowsConstantBuffer._ShadowOffset2, new Vector4(-invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(ShadowsConstantBuffer._ShadowOffset3, new Vector4(invHalfShadowAtlasWidth, invHalfShadowAtlasHeight, 0.0f, 0.0f));
            cmd.SetGlobalVector(ShadowsConstantBuffer._ShadowmapSize, 
                new Vector4(invShadowAtlasWidth, invShadowAtlasHeight, shadowData.lightsShadowmapWidth, shadowData.lightsShadowmapHeight));
        }
    }
}
