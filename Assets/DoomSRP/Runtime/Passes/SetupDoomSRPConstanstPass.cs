using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DoomSRP
{
    /// <summary>
    /// Configure the shader constants needed by the render pipeline
    ///
    /// This pass configures constants that LWRP uses when rendering.
    /// For example, you can execute this pass before you render opaque
    /// objects, to make sure that lights are configured correctly.
    /// </summary>
    public class SetupDoomSRPConstanstPass : ScriptableRenderPass
    {
        static class LightConstantBuffer
        {
            public static int _lightsatlasmap;
            public static int _ClusterInfo;
            public static int _IFScreenSize;
            public static int _ClusterLighting;
            public static int _ClusterCB_Size;
            public static int LightsDataList;
            public static int ItemssIDLis;
            public static int ClusterNumItems;


        }

        const string k_SetupLightConstants = "Setup Light Constants";
        private ComputeBuffer perObjectLightIndices { get; set; }
        PipelineSettings settings;

        /// <summary>
        /// Create the pass
        /// </summary>
        public SetupDoomSRPConstanstPass()
        {
            LightConstantBuffer._lightsatlasmap = Shader.PropertyToID("_lightsatlasmap");
            LightConstantBuffer._ClusterInfo = Shader.PropertyToID("_ClusterInfo");
            LightConstantBuffer._IFScreenSize = Shader.PropertyToID("_IFScreenSize");
            LightConstantBuffer.LightsDataList = Shader.PropertyToID("_LightsDataList");
            LightConstantBuffer._ClusterLighting = Shader.PropertyToID("_ClusterLighting");
            LightConstantBuffer._ClusterCB_Size = Shader.PropertyToID("_ClusterCB_Size");
            LightConstantBuffer.ItemssIDLis = Shader.PropertyToID("_ItemsIDList");
            LightConstantBuffer.ClusterNumItems = Shader.PropertyToID("_ClusterNumItems");
            

        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="maxVisibleAdditionalLights">Maximum number of visible additional lights</param>
        /// <param name="perObjectLightIndices">Buffer holding per object light indicies</param>
        public void Setup(PipelineSettings set, ComputeBuffer perObjectLightIndices)
        {
            settings = set;
        }


        void SetupShaderLightConstants(CommandBuffer cmd, ref LightsData lightData, ref CameraData cameraData)
        {
            if(lightData.lightLoop.spritesAtlas != null)
               cmd.SetGlobalTexture(LightConstantBuffer._lightsatlasmap, lightData.lightLoop.spritesAtlas.texture);
            cmd.SetGlobalVector(LightConstantBuffer._ClusterInfo,
                new Vector4(settings.NumClusterX,
                settings.NumClusterY,
                settings.NumClusterZ,
                settings.MaxItemsPerCluster));

            cmd.SetGlobalVector(LightConstantBuffer._IFScreenSize, cameraData.screenSize);
            cmd.SetGlobalVector(LightConstantBuffer._ClusterLighting, cameraData.ClusterdLighting);
            cmd.SetGlobalVector(LightConstantBuffer._ClusterCB_Size, cameraData._ClusterCB_Size);
            cmd.SetGlobalVector(LightConstantBuffer._IFScreenSize, cameraData.screenSize);
            cmd.SetGlobalBuffer(LightConstantBuffer.LightsDataList, lightData.lightLoop.lightLoopLightsData.LightsDatasBuf);
            cmd.SetGlobalBuffer(LightConstantBuffer.ItemssIDLis, lightData.lightLoop.tileAndClusterData.itemsIDListBuf);
            cmd.SetGlobalBuffer(LightConstantBuffer.ClusterNumItems, lightData.lightLoop.tileAndClusterData.clusterNumItemsBuf);
            cmd.SetGlobalVector("_CameraClipDistance", new Vector4(
                cameraData.camera.nearClipPlane,
                cameraData.camera.farClipPlane - cameraData.camera.nearClipPlane, 0,0));
#if UNITY_EDITOR
            cmd.SetGlobalBuffer("ClusterAABBs", /*IFPipelineManager.*/lightData.lightLoop.tileAndClusterData.clusterAABBsBuf);
            cmd.SetGlobalMatrix("_CameraWorldMatrix", /*Camera.main*/cameraData.camera.transform.localToWorldMatrix);
            cmd.SetGlobalInt("NUM_CLUSTERS_X", /*IFPipelineManager.*/settings.NumClusterX);
            cmd.SetGlobalInt("NUM_CLUSTERS_Y", /*IFPipelineManager.*/settings.NumClusterY);
            cmd.SetGlobalInt("NUM_CLUSTERS_Z", /*IFPipelineManager.*/settings.NumClusterZ);
            cmd.SetGlobalInt("MAX_ITEMS_PERCUSTER", /*IFPipelineManager.*/settings.MaxItemsPerCluster);
#endif
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            CommandBuffer cmd = CommandBufferPool.Get(k_SetupLightConstants);
            SetupShaderLightConstants(cmd, ref renderingData.lightData, ref renderingData.cameraData);

            //CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.AdditionalLightsVertex, additionalLightsCount > 0 && additionalLightsPerVertex);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
