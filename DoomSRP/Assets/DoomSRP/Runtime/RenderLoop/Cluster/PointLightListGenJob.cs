using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace DoomSRP
{
    public struct PointLightListGenJob : IJobParallelFor
    {
        //input
        public int VisibleLightsCount;
        public int NumClusters;
        public int NumItemsPerCluster;
        public bool IsClusterEditorHelper;
        public Matrix4x4 _CameraWorldMatrix;
        [ReadOnly]
        public NativeArray<AABB> InputClusterAABBS;
        [ReadOnly]
        public NativeArray<SFiniteLightBound> LightBounds;
        //more... decals

        //output
        public NativeArray<clusternumlights_t> ResultClusterNumItems;
        //public NativeArray<uint> ResultItemsIDList;
        //public NativeArray<NativeArray<uint>> ResultItemsIDVec;
        public NativeMultiHashMap<int, uint>.Concurrent ResultItemsIDVec;
        // for each cluster
        public void Execute(int i)
        {
            int clusterIndex1D = i;
            int lightOffs = 0;
            int decalsOffs = 0;
            AABB clusterAABB = InputClusterAABBS[clusterIndex1D];
            //1.light clulling test
            //todo more light type
            for (int index = 0; index < VisibleLightsCount && lightOffs < NumItemsPerCluster; ++index)
            {
                SFiniteLightBound bound = LightBounds[index];
                if(IntersectAABBAABB(clusterAABB, bound.aabb))
                {
                    if (IntersectAABBPlaneBounds(clusterAABB, bound.planes))
                    {
                        Item_Set_Light(clusterIndex1D, lightOffs, (uint)index);
                        ++lightOffs;
                    }
                }
            }
            //2.todo decals
            //3.set cluster info
            ClusterInfoSet(clusterIndex1D, lightOffs, decalsOffs);
        }
        #region utils
        void ClusterInfoSet(int clusterID, int lightOffs, int decalsOffs)
        {
            clusternumlights_t clusterInfo;
            clusterInfo.offset = /*NumItemsPerCluster * */clusterID;
            //number of lights
            clusterInfo.numItems = (lightOffs & 255);
            clusterInfo.numItems |= ((decalsOffs & 255) << 8);
            ResultClusterNumItems[clusterID] = clusterInfo;
        }
        void Item_Set_Light(int clusterID, int idInCluster, uint idOfListList)
        {
            if (idInCluster >= NumItemsPerCluster) return;
            ResultItemsIDVec.Add(clusterID, 4095u & idOfListList);
        }

        //bool SphereInsideAABB(SFiniteLightBound sphere, AABB aabb)
        //{
        //    float sqDistance = SqDistancePointAABB(sphere.center, aabb);

        //    return sqDistance <= sphere.radius * sphere.radius;
        //}
        float SqDistancePointAABB(Vector3 p, AABB b)
        {
            float sqDistance = 0.0f;

            for (int i = 0; i < 3; ++i)
            {
                float v = p[i];
                var min = b.Min[i] - v;
                if (v < b.Min[i]) sqDistance += min * min;
                var max = v - b.Max[i];
                if (v > b.Max[i]) sqDistance += max * max;
            }

            return sqDistance;
        }
        bool RightParallelepipedInFrustum(Vector4 Min, Vector4 Max, SPlanes frustum_planes)
        {

            bool inside = true;

            //试视锥体的6个平面。 
            for (int i = 0; i < 6; i++)
            {
                //从8点找到最靠近平面的那个点，然后测试它是否在平面之后。
                //如果是的话 - 那么对象不在视锥体的内部。

                float d = Mathf.Max(Min.x * frustum_planes[i].normal.x, Max.x * frustum_planes[i].normal.x) +
                        Mathf.Max(Min.y * frustum_planes[i].normal.y, Max.y * frustum_planes[i].normal.y) +
                        Mathf.Max(Min.z * frustum_planes[i].normal.z, Max.z * frustum_planes[i].normal.z) +
                        frustum_planes[i].distance;
                inside &= d > 0;
                //return false; //with flag works faster
            }
            return inside;
        }

        bool IntersectAABBPlaneBounds(AABB a, SPlanes ps)
        {
            Vector3 m = a.m;// GetCenter(a.Min, a.Max);// a.GetCenter();// center of AABB
            Vector3 extent = a.extent;// GetExtent(a.Min, a.Max);//a.GetExtent();// half-diagonal
            for (int i = 0; i < 6; ++i)
            {
                Plane p = ps[i];
                Vector3 normal = p.normal;
                float dist = p.GetDistanceToPoint(m);
                float radius = Vector3.Dot(extent, new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z)));
                if (dist + radius <= 0) return false; // behind clip plane
            }
            return true; // AABB intersects space bounded by planes
        }

        bool IntersectAABBAABB(AABB a, AABB b)
        {
            if (Mathf.Abs(a.m.x - b.m.x) > a.extent.x + b.extent.x
              || Mathf.Abs(a.m.y - b.m.y) > a.extent.y + b.extent.y
              || Mathf.Abs(a.m.z - b.m.z) > a.extent.z + b.extent.z)
                return false;
            return true;
        }

        #endregion
    }
}