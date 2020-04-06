using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DoomSRP
{

    public class CulsterDataGenerator //: MonoBehaviour
    {

        public NativeArray<AABB> ClustersAABBs;
#if UNITY_EDITOR
        public NativeArray<AABB> ClustersAABBsCache;
#endif
        public NativeArray<clusternumlights_t> ResultClusterNumItems;
        public NativeArray<uint> ResultItemsIDList;

        //PipelineSettings pipelineSettings;
        
        GenerateClusterJob generateClusterJob;
        //PointLightListGenJob pointLightListGenJob;
        PointLightListGenJobSingleLine pointLightListGenJobSingleLine;


        public void Initilize(PipelineSettings settings)
        {
            //pipelineSettings = settings;
            //pipelineSettings.Check();

            //CleanUp();
            ClustersAABBs = new NativeArray<AABB>(settings.NumClusters, Allocator.Persistent);
#if UNITY_EDITOR
            ClustersAABBsCache = new NativeArray<AABB>(settings.NumClusters, Allocator.Persistent);
#endif
            ResultClusterNumItems = new NativeArray<clusternumlights_t>(settings.NumClusters, Allocator.Persistent);
            ResultItemsIDList = new NativeArray<uint>(settings.MaxItemsPerCluster * settings.NumClusters, Allocator.Persistent);


            generateClusterJob = new GenerateClusterJob();
            //pointLightListGenJob = new PointLightListGenJob();
            pointLightListGenJobSingleLine = new PointLightListGenJobSingleLine();
        }

        private void Set(CameraData cameraData, PipelineSettings pipelineSettings,LightLoop lightLoop)
        {
            generateClusterJob.ClusterCB_GridDimX = pipelineSettings.NumClusterX;
            generateClusterJob.ClusterCB_GridDimY = pipelineSettings.NumClusterY;
            generateClusterJob.ClusterCB_GridDimZ = pipelineSettings.NumClusterZ;
            generateClusterJob.ClusterCB_ViewNear = cameraData.zNear;
            generateClusterJob.ClusterCB_SizeX = ((float)cameraData.pixelWidth / (float)pipelineSettings.NumClusterX);
            generateClusterJob.ClusterCB_SizeY = ((float)cameraData.pixelHeight / (float)pipelineSettings.NumClusterY);
            generateClusterJob.ClusterCB_NearK = 1.0f + cameraData.sD;
            generateClusterJob.ClusterCB_ScreenDimensions = new Vector4(
                cameraData.pixelWidth,
                cameraData.pixelHeight,
                1.0f / (float)cameraData.pixelWidth,
                1.0f / (float)cameraData.pixelHeight
                );
            generateClusterJob.ClusterCB_InverseProjectionMatrix = cameraData.GPUProjectMatrix.inverse;
            generateClusterJob.cameraToWorldMatrix = cameraData.cameraToWorldMatrix;
            generateClusterJob._CameraWorldMatrix = cameraData._CameraWorldMatrix;
            generateClusterJob.ScreenSizeX = cameraData.pixelWidth;
            generateClusterJob.ScreenSizeY = cameraData.pixelHeight;
            generateClusterJob.NearZ = cameraData.zNear;
            generateClusterJob.FarZ = cameraData.zFar;
            generateClusterJob.ClusterCG_ZDIV = cameraData.ClusterdLighting.z;
            generateClusterJob.ClusterdLighting = cameraData.ClusterdLighting;
            generateClusterJob.ResultClusterAABBS = ClustersAABBs;

            pointLightListGenJobSingleLine.NumClusters = pipelineSettings.NumClusters;
            pointLightListGenJobSingleLine.VisibleLightsCount = lightLoop.VisibleLight;
            pointLightListGenJobSingleLine.LightBounds = lightLoop.LigtsBoundList;
            pointLightListGenJobSingleLine.IsClusterEditorHelper = pipelineSettings.IsClusterEditorHelper;
            pointLightListGenJobSingleLine._CameraWorldMatrix = cameraData._CameraWorldMatrix;
            pointLightListGenJobSingleLine.NumItemsPerCluster = LightDefins.MaxItemsPerCluster;
            pointLightListGenJobSingleLine.InputClusterAABBS = ClustersAABBs;
            pointLightListGenJobSingleLine.ResultClusterNumItems = ResultClusterNumItems;
            pointLightListGenJobSingleLine.ResultItemsIDList = ResultItemsIDList;

        }

        public void CleanUp()
        {
            if(/*ClustersAABBs != null &&*/ ClustersAABBs.IsCreated)
                ClustersAABBs.Dispose();
            if (/*ResultClusterNumItems != null &&*/ ResultClusterNumItems.IsCreated)
                ResultClusterNumItems.Dispose();
            if (/*ResultItemsIDList != null &&*/ ResultItemsIDList.IsCreated)
                ResultItemsIDList.Dispose();
#if UNITY_EDITOR
            if (ClustersAABBsCache.IsCreated)
                ClustersAABBsCache.Dispose();
#endif
        }

        public void Run(/*Camera camera*/CameraData cameraData, PipelineSettings pipelineSettings, LightLoop lightLoop)
        {
            Set(cameraData, pipelineSettings, lightLoop);
            // Schedule the job with one Execute per index in the results array and only 1 item per processing batch
            generateClusterJob.Run(pipelineSettings.NumClusters);
            //JobHandle generateClusterJobHandle = generateClusterJob.Schedule(pipelineSettings.NumClusters, 1);

            pointLightListGenJobSingleLine.Run(pipelineSettings.NumClusters);

            //JobHandle pointLightListGenJobSingleLineHandle = pointLightListGenJobSingleLine.Schedule(pipelineSettings.NumClusters, 1, generateClusterJobHandle);
            //pointLightListGenJobSingleLineHandle.Complete();
#if UNITY_EDITOR
            if (ClusterDebug.selectCamera == cameraData.camera)
            {
                ClustersAABBsCache.CopyFrom(ClustersAABBs);
            }
#endif
        }

        public void ClearNumItems(PipelineSettings pipelineSettings)
        {
            int maxItemsPerCluster = pipelineSettings.MaxItemsPerCluster;
            for (int i = 0; i < pipelineSettings.NumClusters; ++i)
            {
                clusternumlights_t clusterInfo;
                clusterInfo.offset = maxItemsPerCluster * i;
                clusterInfo.numItems = 0;
                ResultClusterNumItems[i] = clusterInfo;
            }
        }
    }

}