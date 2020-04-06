using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace DoomSRP
{
    public class LightLoop
    {
        [HideInInspector] [SerializeField] public SpritesAtlas spritesAtlas;

        BetterList<ProjectorLight> lights = new BetterList<ProjectorLight>();
        private int m_NumVisibleLights = 0;

        public int VisibleLight { get { return m_NumVisibleLights; } set { m_NumVisibleLights = value; } }
        private NativeArray<SFiniteLightBound> NativeLightsBoundList;
        private int NumMaxLights = 0;
        public NativeArray<SFiniteLightBound> LigtsBoundList { get { return NativeLightsBoundList; } }
        private NativeArray<LightData> NativeLightsDataList;
        //unused
        public NativeArray<LightData> LightsDataList { get { return NativeLightsDataList; } }

        public CulsterDataGenerator culsterDataGenerator;// = new CulsterDataGenerator();
        [HideInInspector]
        public TileAndClusterData tileAndClusterData;// = new TileAndClusterData();
        [HideInInspector]
        public LightLoopLightsData lightLoopLightsData;// = new LightLoopLightsData();

        public void Initilize(PipelineSettings pipelineSettings)
        {
            bool needRebuild = false;
            if(NumMaxLights != pipelineSettings.MaxItemsOnScreen)
            {
                needRebuild = true;
            }
            if(NativeLightsBoundList == null 
                || NativeLightsDataList == null)
                needRebuild = true;
            
            if (needRebuild)
            {
                Dispose();
                NumMaxLights = pipelineSettings.MaxItemsOnScreen;
                NativeLightsBoundList = new NativeArray<SFiniteLightBound>(NumMaxLights, Allocator.Persistent);
                NativeLightsDataList = new NativeArray<LightData>(NumMaxLights, Allocator.Persistent);
                lights.Clear();
                culsterDataGenerator = new CulsterDataGenerator();
                tileAndClusterData = new TileAndClusterData();
                lightLoopLightsData = new LightLoopLightsData();
                culsterDataGenerator.Initilize(pipelineSettings);
                tileAndClusterData.Initilize(pipelineSettings.NumClusters, pipelineSettings.MaxItemsPerCluster);
                lightLoopLightsData.Initialize(pipelineSettings.MaxItemsOnScreen);
            }
        }

        public void Dispose()
        {
            if (/*NativeLightsBoundList != null && */NativeLightsBoundList.IsCreated)
            {
                NativeLightsBoundList.Dispose();
            }
            if (/*NativeLightsDataList != null &&*/ NativeLightsDataList.IsCreated)
            {
                NativeLightsDataList.Dispose();
            }
            if (culsterDataGenerator != null)
            {
                culsterDataGenerator.CleanUp();
                culsterDataGenerator = null;
            }
            if (tileAndClusterData != null)
            {
                tileAndClusterData.CleanUp();
                tileAndClusterData = null;
            }
            if (lightLoopLightsData != null)
            {
                lightLoopLightsData.CleanUp();
                lightLoopLightsData = null;
            }
        }

        public void RebuildLightsList(List<VisibleLight> visibleLights, CameraData cameraData,PipelineSettings pipelineSettings)
        {
            lights.Clear();
            var unityLights = visibleLights;
            foreach(var ul in unityLights)
            {
                var pl = ul.light.GetComponent<ProjectorLight>();
                if(pl != null)
                    lights.Add(pl);
            }
            VisibleLight = 0;
            for (int i = 0; i < lights.size && i < NumMaxLights; ++i)
            {
                var ifLight = lights[i];
                if (ifLight.spritesAtlas != spritesAtlas)
                {
                    spritesAtlas = ifLight.spritesAtlas;
                }
                //矩阵是从右向左乘的,view需要z取反
                Matrix4x4 c2w = /*Camera.main*/cameraData.camera.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1, -1));// Camera.main.transform.localToWorldMatrix;

                LightDataInAll lightDataInAll = ifLight.GetLightData(c2w);
                NativeLightsBoundList[i] = lightDataInAll.sFiniteLightBound;
                NativeLightsDataList[i] = lightDataInAll.lightData;
                ++VisibleLight;
            }


            culsterDataGenerator.Run(cameraData, pipelineSettings,this);
            lightLoopLightsData.LightsDatasBuf.SetData(LightsDataList);
            tileAndClusterData.itemsIDListBuf.SetData(culsterDataGenerator.ResultItemsIDList);
            tileAndClusterData.clusterNumItemsBuf.SetData(culsterDataGenerator.ResultClusterNumItems);
#if UNITY_EDITOR
            tileAndClusterData.clusterAABBsBuf.SetData(culsterDataGenerator.ClustersAABBs);
#endif
        }


        public static Matrix4x4 WorldToCamera(Camera camera)
        {
            // camera.worldToCameraMatrix is RHS and Unity's transforms are LHS
            // We need to flip it to work with transforms
            return Matrix4x4.Scale(new Vector3(1, 1, -1)) * camera.worldToCameraMatrix;
        }
    }

}