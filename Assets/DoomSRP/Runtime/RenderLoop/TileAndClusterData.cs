using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DoomSRP
{
    public class TileAndClusterData
    {

        public ComputeBuffer clusterNumItemsBuf { get; private set; }
        public ComputeBuffer itemsIDListBuf { get; private set; }
#if UNITY_EDITOR
        public ComputeBuffer clusterAABBsBuf { get; private set; }
#endif
        public void Initilize(int numClusters, int numItemsPerCluster)
        {
            clusterNumItemsBuf = new ComputeBuffer(numClusters, Marshal.SizeOf(typeof(clusternumlights_t)));
            itemsIDListBuf = new ComputeBuffer(numItemsPerCluster * numClusters, sizeof(uint));
#if UNITY_EDITOR
            clusterAABBsBuf = new ComputeBuffer(numClusters, Marshal.SizeOf(typeof(AABB)));
#endif
        }

        public void CleanUp()
        {
            CoreUtils.SafeRelease(clusterNumItemsBuf);
            CoreUtils.SafeRelease(itemsIDListBuf);
            //CoreUtils.SafeRelease(clusterNumItemsBuf);
            //CoreUtils.SafeRelease(itemsIDListBuf);
#if UNITY_EDITOR
            CoreUtils.SafeRelease(clusterAABBsBuf);
#endif
        }
    }

    public class LightLoopLightsData
    {
        // decals
        //public ComputeBuffer decalparms1 { get; private set; }//uniform ,public Vector4,k_MaxDecalsOnScreen
        //public ComputeBuffer decalparms2 { get; private set; }//uniform 
        //public ComputeBuffer decalparms3 { get; private set; }//uniform 

        public ComputeBuffer LightsDatasBuf { get; private set; }//uniform,k_MaxLightsOnScreen

        public void Initialize(int numMaxLights)
        {
            LightsDatasBuf = new ComputeBuffer(numMaxLights, Marshal.SizeOf<LightData>()/*, ComputeBufferType.Constant*/);
        }

        public void CleanUp()
        {
            CoreUtils.SafeRelease(LightsDatasBuf);
        }
    }

}
