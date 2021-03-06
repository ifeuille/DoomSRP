﻿using System;
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

        public BetterList<ProjectorLight> lights = new BetterList<ProjectorLight>();
        private int m_NumVisibleLights = 0;

        public int VisibleLight { get { return m_NumVisibleLights; } set { m_NumVisibleLights = value; } }
        private NativeArray<SFiniteLightBound> NativeLightsBoundList;
        private int NumMaxLights = 0;
        public NativeArray<SFiniteLightBound> LigtsBoundList { get { return NativeLightsBoundList; } }
        public NativeArray<Sphere> LightsSphereList;

        private NativeArray<LightData> NativeLightsDataList;
        //unused
        public NativeArray<LightData> LightsDataList { get { return NativeLightsDataList; } }
        public BetterList<LightDataForShadow> LightsDataForShadow = new BetterList<LightDataForShadow>();

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
                LightsSphereList = new NativeArray<Sphere>(NumMaxLights, Allocator.Persistent);
                NativeLightsDataList = new NativeArray<LightData>(NumMaxLights, Allocator.Persistent);
                lights.Clear();
                culsterDataGenerator = new CulsterDataGenerator();
                tileAndClusterData = new TileAndClusterData();
                lightLoopLightsData = new LightLoopLightsData();
                culsterDataGenerator.Initilize(pipelineSettings);
                tileAndClusterData.Initilize(pipelineSettings.NumClusters, pipelineSettings.MaxItemsPerCluster);
                lightLoopLightsData.Initialize(pipelineSettings.MaxItemsOnScreen);

                LightsDataForShadow.Allocate(NumMaxLights);
            }
        }

        public void Dispose()
        {
            if (/*NativeLightsBoundList != null && */NativeLightsBoundList.IsCreated)
            {
                NativeLightsBoundList.Dispose();
            }
            if(LightsSphereList.IsCreated)
            {
                LightsSphereList.Dispose();
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

        public void RebuildLightsList(List<VisibleLight> visibleLights, CameraData cameraData,PipelineSettings pipelineSettings,ref RenderingData renderingData)
        {
            UnityEngine.Profiling.Profiler.BeginSample("RebuildLightsList");
            lights.Clear();
            var unityLights = visibleLights;
            VisibleLight = 0;
            LightsDataForShadow.Clear();
            bool softShadow = false;
            int j = 0;
            int currentShadowIndex = 0;
            int maxShadowNum = pipelineSettings.MaxShadowLightsNum;
            for (int i = 0; i < unityLights.Count; ++i)
            {
                var ul = unityLights[i];
                var pl = ul.light.GetComponent<ProjectorLight>();
                if (pl == null) continue;
                lights.Add(pl);
                var ifLight = pl;
                if (ifLight.spritesAtlas != spritesAtlas)
                {
                    spritesAtlas = ifLight.spritesAtlas;
                }
                //矩阵是从右向左乘的,view需要z取反
                Matrix4x4 c2w = /*Camera.main*/cameraData.camera.cameraToWorldMatrix * Matrix4x4.Scale(new Vector3(1, 1, -1));// Camera.main.transform.localToWorldMatrix;
                int shadowIndex = currentShadowIndex;
                if(currentShadowIndex >= maxShadowNum || !ifLight.lightParms_Shadow)
                {
                    shadowIndex = -1;
                }
                LightDataInAll lightDataInAll = ifLight.GetLightData(c2w, shadowIndex);
                NativeLightsBoundList[i] = lightDataInAll.sFiniteLightBound;
                NativeLightsDataList[i] = lightDataInAll.lightData;
                ++VisibleLight;

                //for shadow
                // TODO  frustum cull
                if (ifLight.lightParms_Shadow && currentShadowIndex < maxShadowNum)
                {
                    LightDataForShadow lightDataForShadow = new LightDataForShadow();
                    lightDataForShadow.lightIndex = j++;
                    lightDataForShadow.shadowData = lightDataInAll.shadowData;
                    lightDataForShadow.visibleLight = ul;
                    lightDataForShadow.projectorLight = ifLight;
                    lightDataForShadow.unityLightIndex = i;

                    //Matrix4x4 scaleMatrix = Matrix4x4.identity;
                    ////scaleMatrix.m22 = -1.0f;
                    //Matrix4x4 view = scaleMatrix * ul.localToWorld.inverse;
                    //lightDataForShadow.shadowData.viewMatrix = view;

                    LightsDataForShadow.Add(lightDataForShadow);

                    softShadow |= ifLight.softShadow;
                    ++currentShadowIndex;
                }
            }
            renderingData.shadowData.supportsSoftShadows = softShadow;
            culsterDataGenerator.Begin(pipelineSettings);
            culsterDataGenerator.Run(cameraData, pipelineSettings,this);
            lightLoopLightsData.LightsDatasBuf.SetData(LightsDataList);
            tileAndClusterData.itemsIDListBuf.SetData(culsterDataGenerator.ResultItemsIDList);
            tileAndClusterData.clusterNumItemsBuf.SetData(culsterDataGenerator.ResultClusterNumItems);
            culsterDataGenerator.End();
            UnityEngine.Profiling.Profiler.EndSample();
        }


        public static Matrix4x4 WorldToCamera(Camera camera)
        {
            // camera.worldToCameraMatrix is RHS and Unity's transforms are LHS
            // We need to flip it to work with transforms
            return Matrix4x4.Scale(new Vector3(1, 1, -1)) * camera.worldToCameraMatrix;
        }
    }

}