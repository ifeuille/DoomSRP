using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace DoomSRP
{
    public struct GenerateClusterJob : IJobParallelFor
    {
        //input
        public int ClusterCB_GridDimX;
        public int ClusterCB_GridDimY;
        public int ClusterCB_GridDimZ;
        public float ClusterCB_ViewNear;     // The distance to the near clipping plane. (Used for computing the index in the cluster grid)
        //uint2 ClusterCB_Size;         // The size of a cluster in screen space (pixels).
        public float ClusterCB_SizeX;
        public float ClusterCB_SizeY;
        public float ClusterCB_NearK;        // ( 1 + ( 2 * tan( fov * 0.5 ) / ClusterGridDim.y ) ) 
        // Used to compute the near plane for clusters at depth k.
        public Vector4 ClusterCB_ScreenDimensions;
        public Matrix4x4 ClusterCB_InverseProjectionMatrix;
        public Matrix4x4 cameraToWorldMatrix;
        public Matrix4x4 _CameraWorldMatrix;

        public float ScreenSizeX;
        public float ScreenSizeY;
        public float NearZ;
        public float FarZ;
        public float ClusterCG_ZDIV;

        public Vector4 ClusterdLighting;
        

        //output
        public NativeArray<AABB> ResultClusterAABBS;


        //doom6
        public void Execute(int i)
        {
            Vector3Int clusterIndex3D = ComputeClusterIndex3D(i);
            // Compute the near and far planes for cluster K.
            float z_i = GetZ2(clusterIndex3D.z);
            float z_ip1 = GetZ2(clusterIndex3D.z + 1);
            
            //z_i = Mathf.Min(FarZ, z_i);
            //z_ip1 = Mathf.Min(FarZ, z_ip1);

            Plane nearPlane = new Plane(new Vector3(0, 0, 1), z_i);
            Plane farPlane = new Plane(new Vector3(0, 0, 1), z_ip1);

            // The top-left point of cluster K in screen space.
            Vector4 pMin = new Vector4(clusterIndex3D.x * ClusterCB_SizeX, clusterIndex3D.y * ClusterCB_SizeY, 0.0f, 1.0f);
            // The bottom-right point of cluster K in screen space.
            Vector4 pMax = new Vector4((clusterIndex3D.x + 1) * ClusterCB_SizeX, (clusterIndex3D.y + 1) * ClusterCB_SizeY, 0.0f, 1.0f);

            // Transform the screen space points to view space.
            pMin = ScreenToView(pMin);
            pMax = ScreenToView(pMax);

            pMin.z *= -1;
            pMax.z *= -1;



            // Find the min and max points on the near and far planes.
        
            Vector3 farMin, farMax;
#if true
            Vector3 nearMin, nearMax;
            // Origin (camera eye position)
            Vector3 eye = Vector3.zero;
            IntersectLinePlane0(eye, new Vector3(pMin.x, pMin.y, pMin.z), nearPlane, out nearMin);
            IntersectLinePlane0(eye, new Vector3(pMax.x, pMax.y, pMax.z), nearPlane, out nearMax);
            IntersectLinePlane0(eye, new Vector3(pMin.x, pMin.y, pMin.z), farPlane, out farMin);
            IntersectLinePlane0(eye, new Vector3(pMax.x, pMax.y, pMax.z), farPlane, out farMax);

            Vector3 aabbMin = Vector3.Min(nearMin, Vector3.Min(nearMax, Vector3.Min(farMin, farMax)));
            Vector3 aabbMax = Vector3.Max(nearMin, Vector3.Max(nearMax, Vector3.Max(farMin, farMax)));
#else
            IntersectLinePlane(new Vector3(pMin.x, pMin.y, pMin.z), farPlane, out farMin);
            IntersectLinePlane(new Vector3(pMax.x, pMax.y, pMax.z), farPlane, out farMax);

            Vector3 aabbMin = new Vector3(farMin.x, farMin.y, z_i);
            Vector3 aabbMax = new Vector3(farMax.x, farMax.y, farPlane.distance);
#endif
            
            AABB aabb = new AABB();
            aabb.Min = new Vector4(aabbMin.x, aabbMin.y, aabbMin.z, 1.0f);
            aabb.Max = new Vector4(aabbMax.x, aabbMax.y, aabbMax.z, 1.0f);
            aabb.GenerateCenterAndExtent();
            ResultClusterAABBS[i] = aabb;
        }

#region utils
        Vector3Int ComputeClusterIndex3D(int clusterIndex1D)
        {
            int i = clusterIndex1D % ClusterCB_GridDimX;
            int j = clusterIndex1D % (ClusterCB_GridDimX * ClusterCB_GridDimY) / ClusterCB_GridDimX;
            int k = clusterIndex1D / (ClusterCB_GridDimX * ClusterCB_GridDimY);

            return new Vector3Int(i, j, k);
        }

        /**
         * Find the intersection of a line segment with a plane.
         * This function will return true if an intersection point
         * was found or false if no intersection could be found.
         * Source: Real-time collision detection, Christer Ericson (2005)
         */
        bool IntersectLinePlane0(Vector3 a, Vector3 b, Plane p, out Vector3 q)
        {
            Vector3 ab = b - a;
#if true
            float t = (p.distance - Vector3.Dot(p.normal, a)) / Vector3.Dot(p.normal, ab);

            bool intersect = (t >= 0.0f && t <= 1.0f);

            q = Vector3.zero;
            q = a + t * ab;
            //if (intersect)
            //{
            //    q = a + t * ab;
            //}
#else
            float rate = p.distance / Mathf.Abs(b.z);
            q = new Vector3(b.x * rate, b.y * rate, p.distance);
#endif
            return intersect;
        }
        bool IntersectLinePlane( Vector3 b, Plane p, out Vector3 q)
        {
            Vector3 ab = b;
            //float t = p.distance / Vector3.Dot(p.normal, ab);
            //t = Mathf.Clamp01(t);
            //q = t * ab;

            float rate = p.distance / Mathf.Abs(b.z);
            q = new Vector3(b.x * rate, b.y * rate, p.distance);

            //float rate = (p.distance - Vector3.Dot(b, p.normal)) / Vector3.Dot(ab, p.normal);
            //q = new Vector3(b.x * rate, b.y * rate, p.distance);
            return true;
        }

        /// Functions.hlsli
        // Convert clip space coordinates to view space
        Vector4 ClipToView(Vector4 clip)
        {
            // View space position.
            Vector4 view = ClusterCB_InverseProjectionMatrix * clip;// ClusterCB_InverseProjectionMatrix.MultiplyVector(clip);
            // Perspecitive projection.
            if (view.w == 0) view.w = 0.000001f;//?
             view = view / view.w;

            return view;
        }

        // Convert screen space coordinates to view space.
        Vector4 ScreenToView(Vector4 screen)
        {
            // Convert to normalized texture coordinates in the range [0 .. 1].
            Vector2 texCoord = new Vector2(screen.x * ClusterCB_ScreenDimensions.z, screen.y * ClusterCB_ScreenDimensions.w);

            // Convert to clip space
            Vector4 clip = new Vector4(texCoord.x * 2.0f - 1.0f, (1.0f - texCoord.y) * 2.0f - 1.0f, screen.z, screen.w);
            //Vector4 clip = new Vector4(texCoord.x * 2.0f - 1.0f, texCoord.y * 2.0f - 1.0f, screen.z, screen.w);
            return ClipToView(clip);
        }

        Vector4 ScreenToViewportPoint(Vector4 pos)
        {
            pos.x = (pos.x - 0) / ScreenSizeX;
            pos.y = (pos.y - 0) / ScreenSizeY;
            return pos;
        }

        Vector4 ViewToWorld(Vector4 viewPos)
        {
            return _CameraWorldMatrix.MultiplyPoint(viewPos);
        }

#endregion


#region utils for 
        float GetZ0(int slice)
        {
            return ClusterCB_ViewNear * Mathf.Pow(Mathf.Abs(ClusterCB_NearK), slice);
        }
        float GetZ1(int slice)
        {
            return ClusterCG_ZDIV * Mathf.Pow(2, NearZ * slice);
        }
        float GetZ2(int slice)
        {
            return ClusterdLighting.x * Mathf.Pow(2, ClusterdLighting.y * slice);
            //float zNear = ClusterdLighting.x;
            //float zFar = ClusterdLighting.y;
            //return zNear * Mathf.Pow(zFar / zNear, slice / (float)(ClusterCB_GridDimZ - 1));
        }

        #endregion
    }
}