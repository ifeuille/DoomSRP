using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering;
using UnityEngine.Experimental.GlobalIllumination;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;
using UnityEngine.Experimental.Rendering;
using UnityEngine;
using System;

namespace DoomSRP
{
    public interface IBeforeCameraRender
    {
        void ExecuteBeforeCameraRender(DoomSRPPipeline pipelineInstance, ScriptableRenderContext context, Camera camera);
    }

    public struct CameraData
    {
        public Camera camera;
        //public float renderScale;
        public int msaaSamples;
        public bool isSceneViewCamera;
        public bool isDefaultViewport;
        //public bool isOffscreenRender;
        public bool isHdrEnabled;
        public bool requiresDepthTexture;
        public bool requiresOpaqueTexture;
        public Downsampling opaqueTextureDownsampling;

        public SortFlags defaultOpaqueSortFlags;
        
        public float maxShadowDistance;


        public Vector4 screenSize;

        public float fieldOfViewY;
        public float zNear;
        public float zFar;
        public int clusterDimX;
        public int clusterDimY;
        public float sD;
        public float logDimY;
        public float logDepth;
        public int pixelWidth;
        public int pixelHeight;
        public Matrix4x4 WorldToCameraSpace;
        public Matrix4x4 projectMatrix;
        public Matrix4x4 GPUProjectMatrix;
        public Matrix4x4 cameraToWorldMatrix;
        public Matrix4x4 _CameraWorldMatrix;
        public Vector4 ClusterdLighting;
        public Vector4 _ClusterCB_Size;

        public float MyA;
        public float MyB;
        public Vector4 ClusterCameraParam;


        public void Refresh(Camera camera, PipelineSettings pipelineSettings)
        {
            fieldOfViewY = camera.fieldOfView * Mathf.Deg2Rad * 0.5f;//Degree 2 Radiance:  Param.CameraInfo.Property.Perspective.fFovAngleY * 0.5f;
            zNear = camera.nearClipPlane;// Param.CameraInfo.Property.Perspective.fMinVisibleDistance;
            zFar = camera.farClipPlane;// Param.CameraInfo.Property.Perspective.fMaxVisibleDistance;

            // Number of clusters in the screen X direction.
            clusterDimX = pipelineSettings.NumClusterX;/* Mathf.CeilToInt(Screen.width / (float)m_ClusterGridBlockSize);*/
            // Number of clusters in the screen Y direction.
            clusterDimY = pipelineSettings.NumClusterY;//Mathf.CeilToInt(Screen.height / (float)m_ClusterGridBlockSize);

            // The depth of the cluster grid during clustered rendering is dependent on the 
            // number of clusters subdivisions in the screen Y direction.
            // Source: Clustered Deferred and Forward Shading (2012) (Ola Olsson, Markus Billeter, Ulf Assarsson).
            sD = 2.0f * Mathf.Tan(fieldOfViewY) / (float)clusterDimY;
            logDimY = 1.0f / Mathf.Log(1.0f + sD);

            float logDepth = Mathf.Log(zFar / zNear);
            logDepth = Mathf.Log(zFar / zNear);
            pixelWidth = camera.pixelWidth;
            pixelHeight = camera.pixelHeight;
            projectMatrix = camera.projectionMatrix;
            GPUProjectMatrix = GL.GetGPUProjectionMatrix(projectMatrix, false);

            ClusterdLighting.z = (zFar - zNear) / (float)pipelineSettings.NumClusterZ;
            ClusterdLighting.w = 1.0f / zNear;

            _ClusterCB_Size.x = pixelWidth / (float)clusterDimX;
            _ClusterCB_Size.y = pixelHeight / (float)clusterDimY;
            _ClusterCB_Size.z = logDepth / logDimY;


            ClusterdLighting.x = zNear;
            ClusterdLighting.y = Mathf.Log(zFar / zNear, 2) / (float)pipelineSettings.NumClusterZ;
            //ClusterdLighting.y = zFar;
            WorldToCameraSpace = camera.worldToCameraMatrix;

            ClusterCameraParam.x = camera.farClipPlane;
            ClusterCameraParam.y = camera.nearClipPlane;

            cameraToWorldMatrix = camera.cameraToWorldMatrix;
            _CameraWorldMatrix = camera.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1, -1));// camera.transform.localToWorldMatrix;

            screenSize = new Vector4(pixelWidth, pixelHeight, 1.0f / pixelWidth, 1.0f / pixelWidth);
        }
    }

    public static class ShaderKeywordStrings
    {
        public static readonly string MainLightShadows = "_MAIN_LIGHT_SHADOWS";
        public static readonly string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";
        public static readonly string AdditionalLightsVertex = "_ADDITIONAL_LIGHTS_VERTEX";
        public static readonly string AdditionalLightsPixel = "_ADDITIONAL_LIGHTS";
        public static readonly string AdditionalLightShadows = "_ADDITIONAL_LIGHT_SHADOWS";
        public static readonly string SoftShadows = "_SHADOWS_SOFT";
        public static readonly string MixedLightingSubtractive = "_MIXED_LIGHTING_SUBTRACTIVE";

        public static readonly string DepthNoMsaa = "_DEPTH_NO_MSAA";
        public static readonly string DepthMsaa2 = "_DEPTH_MSAA_2";
        public static readonly string DepthMsaa4 = "_DEPTH_MSAA_4";
    }

    public class DoomSRPPipeline : RenderPipeline
    {
        [SerializeField]
        public PipelineSettings settings = new PipelineSettings();// { get; set; }
        static class PerFrameBuffer
        {
            public static int _GlossyEnvironmentColor;
            public static int _SubtractiveShadowColor;
        }
        static class PerCameraBuffer
        {
            // TODO: This needs to account for stereo rendering
            public static int _InvCameraViewProj;
            public static int _ScaledScreenParams;
        }
        const string k_RenderCameraTag = "Render Camera";
        public const string k_ShaderTagName = "DoomSRP";
        CullResults m_CullResults;
        // private static IRendererSetup s_DefaultRendererSetup;
        CameraData cameraData;
        private static IRendererSetup s_DefaultRendererSetup;

        public LightLoop lightLoop = new LightLoop();
        public static DoomSRPPipeline doomSRPPipeline;

        static List<Vector4> m_ShadowBiasData = new List<Vector4>();

        public LightLoop GetLightLoop()
        {
            if(doomSRPPipeline != null) { return doomSRPPipeline.lightLoop; }
            return null;
        }

        private static IRendererSetup defaultRendererSetup
        {
            get
            {
                if (s_DefaultRendererSetup == null)
                    s_DefaultRendererSetup = new DefaultRendererSetup();

                return s_DefaultRendererSetup;
            }
        }

        public ScriptableRenderer renderer { get; private set; }

        public DoomSRPPipeline(DoomSRPAsset asset)
        {
            Shader.globalRenderPipeline = "DoomSRP";
            renderer = new ScriptableRenderer(asset);
            lightLoop.Initilize(settings);
            doomSRPPipeline = this;
        }

        public override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            base.Render(context, cameras);
            GraphicsSettings.lightsUseLinearIntensity = true;
            SortCameras(cameras);
            foreach (Camera camera in cameras)
            {
                CommandBuffer cmd = CommandBufferPool.Get(k_RenderCameraTag);
                using (new ProfilingSample(cmd, k_RenderCameraTag))
                {
                    BeginCameraRendering(camera);
                    foreach (var beforeCamera in camera.GetComponents<IBeforeCameraRender>())
                        beforeCamera.ExecuteBeforeCameraRender(this, context, camera);

                    RenderSingleCamera(this, context, camera, ref m_CullResults, camera.GetComponent<IRendererSetup>());

                }
            }
        }

        public static void RenderSingleCamera(DoomSRPPipeline pipelineInstance, ScriptableRenderContext context, Camera camera, ref CullResults cullResults, IRendererSetup setup = null)
        {
            if (pipelineInstance == null)
            {
                Debug.LogError("Trying to render a camera with an invalid render pipeline instance.");
                return;
            }
 
            ScriptableCullingParameters cullingParameters;
            if (!CullResults.GetCullingParameters(camera, out cullingParameters))
                return;

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderCameraTag);
            using (new ProfilingSample(cmd, k_RenderCameraTag))
            {
                CameraData cameraData;
                PipelineSettings settings = pipelineInstance.settings;
                ScriptableRenderer renderer = pipelineInstance.renderer;
                LightLoop lightloop = pipelineInstance.GetLightLoop();
                InitializeCameraData(pipelineInstance.settings, camera, out cameraData);
                SetupPerCameraShaderConstants(cameraData);

                cullingParameters.shadowDistance = Mathf.Min(cameraData.maxShadowDistance, camera.farClipPlane);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

#if UNITY_EDITOR
                // Emit scene view UI
                if (cameraData.isSceneViewCamera)
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
#endif
                CullResults.Cull(ref cullingParameters, context, ref cullResults);

                RenderingData renderingData;
                InitializeRenderingData(settings, ref cameraData, ref cullResults, ref lightloop, out renderingData);
                var setupToUse = setup;
                if (setupToUse == null)
                    setupToUse = defaultRendererSetup;

                renderer.Clear();
                setupToUse.Setup(renderer, ref renderingData);
                renderer.Execute(context, ref renderingData);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
#if UNITY_EDITOR
            Handles.DrawGizmos(camera);
#endif
        }

        static void InitializeRenderingData(PipelineSettings settings, ref CameraData cameraData, ref CullResults cullResults,
            ref LightLoop lightloop, out RenderingData renderingData)
        {
            List<VisibleLight> visibleLights = cullResults.visibleLights;
                        
            renderingData.cullResults = cullResults;
            renderingData.cameraData = cameraData;
            renderingData.settings = settings;
            //todo 自定义光源裁剪，因为这个光源是projector光源
            InitializeLightData(settings, visibleLights, lightloop, out renderingData.lightData);
            renderingData.supportsDynamicBatching = false;// settings.supportsDynamicBatching;

            //todo: settings
            renderingData.shadowData = new ShadowData();
            renderingData.shadowData.lightsShadowmapHeight = 1024;
            renderingData.shadowData.lightsShadowmapWidth = 1024;
            renderingData.shadowData.supportsLightShadows = true;
            renderingData.shadowData.supportsSoftShadows = false;
            renderingData.shadowData.shadowmapDepthBufferBits = 24;

            lightloop.RebuildLightsList(cullResults.visibleLights, cameraData, settings, ref renderingData);
            InitializeShadowData(settings, lightloop, ref renderingData.shadowData);
        }
        static void InitializeLightData(PipelineSettings settings, List<VisibleLight> visibleLights, LightLoop lightloop, out LightsData lightData)
        {
        //        public int mainLightIndex;
        //public int additionalLightsCount;
        //public int maxPerObjectAdditionalLightsCount;
        //public List<VisibleLight> visibleLights;
        //public bool shadeAdditionalLightsPerVertex;
        //public bool supportsMixedLighting;
            
            
            lightData.visibleLights = visibleLights;
            lightData.lightLoop = lightloop;
            //lightData.mainLightIndex = 0;
        }

        static void InitializeShadowData(PipelineSettings settings, LightLoop lightLoop, ref ShadowData shadowData)
        {
            m_ShadowBiasData.Clear();
            var lights = lightLoop.lights;
            for(int i = 0; i < lights.size; ++i)
            {
                var light = lights[i];
                m_ShadowBiasData.Add(new Vector4(light.shadowBias, light.shadowNormalBias, 0.0f, 0.0f));
            }
            
            shadowData.bias = m_ShadowBiasData;

            // Until we can have keyword stripping forcing single cascade hard shadows on gles2
            bool supportsScreenSpaceShadows = SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;

            // we resolve shadows in screenspace when cascades are enabled to save ALU as computing cascade index + shadowCoord on fragment is expensive
            shadowData.requiresScreenSpaceShadowResolve = supportsScreenSpaceShadows;
            shadowData.supportsSoftShadows = settings.supportsSoftShadows && shadowData.supportsSoftShadows;
            shadowData.shadowmapDepthBufferBits = 16;
        }

        static void SetupPerCameraShaderConstants(CameraData cameraData)
        {            
            Camera camera = cameraData.camera;
            float cameraWidth = (float)cameraData.camera.pixelWidth;// * cameraData.renderScale;
            float cameraHeight = (float)cameraData.camera.pixelHeight;// * cameraData.renderScale;
            Shader.SetGlobalVector(PerCameraBuffer._ScaledScreenParams, new Vector4(cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));

            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
            Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
            //Shader.SetGlobalMatrix(PerCameraBuffer._InvCameraViewProj, invViewProjMatrix);

        }

        void SortCameras(Camera[] cameras)
        {
            Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));
        }

        private static void InitializeCameraData(PipelineSettings settings, Camera camera, out CameraData cameraData)
        {
            cameraData = new CameraData();
            cameraData.camera = camera;
            bool msaaEnabled = camera.allowMSAA && settings.msaaSampleCount > 1;
            if (msaaEnabled)
                cameraData.msaaSamples = (camera.targetTexture != null) ? camera.targetTexture.antiAliasing : settings.msaaSampleCount;
            else
                cameraData.msaaSamples = 1;

            cameraData.isSceneViewCamera = camera.cameraType == CameraType.SceneView;
            //cameraData.isOffscreenRender = camera.targetTexture != null && !cameraData.isSceneViewCamera;
            //cameraData.isHdrEnabled = camera.allowHDR;

            Rect cameraRect = camera.rect;

            var commonOpaqueFlags = SortFlags.CommonOpaque;
            var noFrontToBackOpaqueFlags = SortFlags.SortingLayer | SortFlags.RenderQueue | SortFlags.OptimizeStateChanges | SortFlags.CanvasOrder;
            bool hasHSRGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
            bool canSkipFrontToBackSorting = (camera.opaqueSortMode == OpaqueSortMode.Default && hasHSRGPU) || camera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;

            cameraData.defaultOpaqueSortFlags = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : commonOpaqueFlags;
            cameraData.Refresh(camera, settings);
        }

        private static void SetupPerFrameCameraConstants(CameraData cameraData)
        {

        }

        public override void Dispose()
        {
            renderer.Dispose();
            lightLoop.Dispose();
        }
        
        

    }

}