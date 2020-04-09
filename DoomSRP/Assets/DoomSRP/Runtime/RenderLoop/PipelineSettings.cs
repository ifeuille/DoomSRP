using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DoomSRP
{
    [Serializable]
    public class PipelineSettings
    {
        //[Range(0,512)]
        [SerializeField]
        public int MaxItemsOnScreen = 512;//同步修改#define MAXITEMSONSCREEN 256
        [SerializeField]
        public int MaxShadowLightsNum = 64;//阴影灯光数量最大值8x8

        [HideInInspector]
        [SerializeField]
        public int MaxItemsPerCluster = LightDefins.MaxItemsPerCluster;
        [HideInInspector]
        [SerializeField]
        public int NumClusterX = LightDefins.NumClusterX;
        [HideInInspector]
        [SerializeField]
        public int NumClusterY = LightDefins.NumClusterY;
        [HideInInspector]
        [SerializeField]
        public int NumClusterZ = LightDefins.NumClusterZ;

        [HideInInspector]
        public int NumClusters { get { return NumClusterX * NumClusterY * NumClusterZ; } }

        [HideInInspector]
        public bool IsClusterEditorHelper = false;

        public int msaaSampleCount = 1;

        public void Check()
        {
            Debug.Assert(NumClusterX % 4 == 0);
            Debug.Assert(NumClusterY % 4 == 0);
            Debug.Assert(NumClusterZ % 4 == 0);
            Debug.Assert(MaxItemsPerCluster > 0);
            Debug.Assert(MaxItemsOnScreen > 0);
        }

    }

}