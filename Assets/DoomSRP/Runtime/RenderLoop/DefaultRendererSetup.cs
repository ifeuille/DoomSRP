using System;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;


namespace DoomSRP
{

    internal class DefaultRendererSetup : IRendererSetup
    {
        private DepthOnlyPass m_DepthOnlyPass;
        //private MainLightShadowCasterPass m_MainLightShadowCasterPass;
        //private AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;
        private SetupForwardRenderingPass m_SetupForwardRenderingPass;
        //private ScreenSpaceShadowResolvePass m_ScreenSpaceShadowResolvePass;
        private CreateDoomSRPRenderTexturesPass m_CreateRenderTexturesPass;
        private SetupDoomSRPConstanstPass m_SetupDoomSRPConstants;
        private OpaqueForwardPass m_RenderOpaqueForwardPass;
        private DrawSkyboxPass m_DrawSkyboxPass;
        private CopyDepthPass m_CopyDepthPass;
        private CopyColorPass m_CopyColorPass;
        //private RenderTransparentForwardPass m_RenderTransparentForwardPass;
        private FinalBlitPass m_FinalBlitPass;

#if UNITY_EDITOR
        private ClusterDebugPass m_ClusterDebugPass;
        private SceneViewDepthCopyPass m_SceneViewDepthCopyPass;
#endif

        private RenderTargetHandle ColorAttachment;
        private RenderTargetHandle DepthAttachment;
        private RenderTargetHandle DepthTexture;
        private RenderTargetHandle OpaqueColor;
        //private RenderTargetHandle MainLightShadowmap;
        //private RenderTargetHandle AdditionalLightsShadowmap;
        //private RenderTargetHandle ScreenSpaceShadowmap;

        private List<IBeforeRender> m_BeforeRenderPasses = new List<IBeforeRender>(10);

        [NonSerialized]
        private bool m_Initialized = false;

        private void Init()
        {
            if (m_Initialized)
                return;

            m_DepthOnlyPass = new DepthOnlyPass();
            //m_MainLightShadowCasterPass = new MainLightShadowCasterPass();
            //m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass();
            m_SetupForwardRenderingPass = new SetupForwardRenderingPass();
            //m_ScreenSpaceShadowResolvePass = new ScreenSpaceShadowResolvePass();
            m_CreateRenderTexturesPass = new CreateDoomSRPRenderTexturesPass();
            m_SetupDoomSRPConstants = new SetupDoomSRPConstanstPass();
            m_RenderOpaqueForwardPass = new OpaqueForwardPass();
            m_DrawSkyboxPass = new DrawSkyboxPass();
            m_CopyDepthPass = new CopyDepthPass();
            m_CopyColorPass = new CopyColorPass();
            //m_RenderTransparentForwardPass = new RenderTransparentForwardPass();
            m_FinalBlitPass = new FinalBlitPass();

#if UNITY_EDITOR
            m_ClusterDebugPass = new ClusterDebugPass();
            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass();
#endif

            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
            // Samples (MSAA) depend on camera and pipeline
            ColorAttachment.Init("_CameraColorTexture");
            DepthAttachment.Init("_CameraDepthAttachment");
            DepthTexture.Init("_CameraDepthTexture");
            OpaqueColor.Init("_CameraOpaqueTexture");
            //MainLightShadowmap.Init("_MainLightShadowmapTexture");
            //AdditionalLightsShadowmap.Init("_AdditionalLightsShadowmapTexture");
            //ScreenSpaceShadowmap.Init("_ScreenSpaceShadowmapTexture");

            m_Initialized = true;
        }

        public void Setup(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            Init();

            Camera camera = renderingData.cameraData.camera;
            camera.GetComponents(m_BeforeRenderPasses);

            renderer.SetupPerObjectLightIndices(ref renderingData.cullResults, ref renderingData.lightData);
            RenderTextureDescriptor baseDescriptor = ScriptableRenderer.CreateRenderTextureDescriptor(ref renderingData.cameraData);
            RenderTextureDescriptor shadowDescriptor = baseDescriptor;
            ClearFlag clearFlag = ScriptableRenderer.GetCameraClearFlag(renderingData.cameraData.camera);
            shadowDescriptor.dimension = TextureDimension.Tex2D;

            bool requiresRenderToTexture = ScriptableRenderer.RequiresIntermediateColorTexture(ref renderingData.cameraData, baseDescriptor)
                                           || m_BeforeRenderPasses.Count != 0;

            RenderTargetHandle colorHandle = RenderTargetHandle.CameraTarget;
            RenderTargetHandle depthHandle = RenderTargetHandle.CameraTarget;

            if (requiresRenderToTexture)
            {
                colorHandle = ColorAttachment;
                depthHandle = DepthAttachment;

                var sampleCount = (SampleCount)renderingData.cameraData.msaaSamples;
                m_CreateRenderTexturesPass.Setup(baseDescriptor, colorHandle, depthHandle, sampleCount);
                renderer.EnqueuePass(m_CreateRenderTexturesPass);
            }

            foreach (var pass in m_BeforeRenderPasses)
            {
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle, clearFlag));
            }

            //if (renderingData.shadowData.supportsAdditionalLightShadows)
            {
                //bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(AdditionalLightsShadowmap, ref renderingData, renderer.maxVisibleAdditionalLights);
                //if (additionalLightShadows)
                //    renderer.EnqueuePass(m_AdditionalLightsShadowCasterPass);
            }

            bool resolveShadowsInScreenSpace = false;// renderingData.shadowData.requiresScreenSpaceShadowResolve;
            bool requiresDepthPrepass = resolveShadowsInScreenSpace || renderingData.cameraData.isSceneViewCamera ||
                                        (renderingData.cameraData.requiresDepthTexture && (!CanCopyDepth(ref renderingData.cameraData)));


            renderer.EnqueuePass(m_SetupForwardRenderingPass);

            if (requiresDepthPrepass)
            {
                m_DepthOnlyPass.Setup(baseDescriptor, DepthTexture, SampleCount.One);
                renderer.EnqueuePass(m_DepthOnlyPass);

                foreach (var pass in camera.GetComponents<IAfterDepthPrePass>())
                    renderer.EnqueuePass(pass.GetPassToEnqueue(m_DepthOnlyPass.descriptor, DepthTexture));
            }

            if (resolveShadowsInScreenSpace)
            {
                //m_ScreenSpaceShadowResolvePass.Setup(baseDescriptor, ScreenSpaceShadowmap);
                //renderer.EnqueuePass(m_ScreenSpaceShadowResolvePass);
            }


            RendererConfiguration rendererConfiguration = ScriptableRenderer.GetRendererConfiguration(0);

            m_SetupDoomSRPConstants.Setup(renderingData.settings, renderer.perObjectLightIndices);
            renderer.EnqueuePass(m_SetupDoomSRPConstants);

            // If a before all render pass executed we expect it to clear the color render target
            if (m_BeforeRenderPasses.Count != 0)
                clearFlag = ClearFlag.None;

            m_RenderOpaqueForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, clearFlag, camera.backgroundColor, rendererConfiguration);
            renderer.EnqueuePass(m_RenderOpaqueForwardPass);
            foreach (var pass in camera.GetComponents<IAfterOpaquePass>())
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

            //if (renderingData.cameraData.postProcessEnabled &&
            //    renderingData.cameraData.postProcessLayer.HasOpaqueOnlyEffects(renderer.postProcessingContext))
            //{
            //    m_OpaquePostProcessPass.Setup(baseDescriptor, colorHandle);
            //    renderer.EnqueuePass(m_OpaquePostProcessPass);

            //    foreach (var pass in camera.GetComponents<IAfterOpaquePostProcess>())
            //        renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));
            //}

            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                m_DrawSkyboxPass.Setup(colorHandle, depthHandle);
                renderer.EnqueuePass(m_DrawSkyboxPass);
            }

            foreach (var pass in camera.GetComponents<IAfterSkyboxPass>())
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

            if (renderingData.cameraData.requiresDepthTexture && !requiresDepthPrepass)
            {
                m_CopyDepthPass.Setup(depthHandle, DepthTexture);
                renderer.EnqueuePass(m_CopyDepthPass);
            }

            if (renderingData.cameraData.requiresOpaqueTexture)
            {
                m_CopyColorPass.Setup(colorHandle, OpaqueColor);
                renderer.EnqueuePass(m_CopyColorPass);
            }

            //m_RenderTransparentForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, rendererConfiguration);
            //renderer.EnqueuePass(m_RenderTransparentForwardPass);

            foreach (var pass in camera.GetComponents<IAfterTransparentPass>())
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

            if (colorHandle != RenderTargetHandle.CameraTarget)
            {
                m_FinalBlitPass.Setup(baseDescriptor, colorHandle);
                renderer.EnqueuePass(m_FinalBlitPass);
            }

            foreach (var pass in camera.GetComponents<IAfterRender>())
                renderer.EnqueuePass(pass.GetPassToEnqueue());


#if UNITY_EDITOR
            if (renderingData.cameraData.isSceneViewCamera)
            {
                m_ClusterDebugPass.Setup(renderer.GetMaterial(MaterialHandle.ClusterDebug), colorHandle, depthHandle);
                renderer.EnqueuePass(m_ClusterDebugPass);

                m_SceneViewDepthCopyPass.Setup(DepthTexture);
                renderer.EnqueuePass(m_SceneViewDepthCopyPass);
            }
#endif
        }

        bool CanCopyDepth(ref CameraData cameraData)
        {
            bool msaaEnabledForCamera = (int)cameraData.msaaSamples > 1;
            bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            bool supportsDepthTarget = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

            // TODO:  We don't have support to highp Texture2DMS currently and this breaks depth precision.
            // currently disabling it until shader changes kick in.
            //bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
            bool msaaDepthResolve = false;
            return supportsDepthCopy || msaaDepthResolve;
        }
    }
}
