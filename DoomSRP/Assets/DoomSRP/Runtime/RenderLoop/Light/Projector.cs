﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoomSRP
{
    public struct ProjectorSettings
    {
        public Vector4 projectMatrixX;
        public Vector4 projectMatrixY;
        public Vector4 projectMatrixW;
        public Vector4 falloffR;

        public Matrix4x4 projection;
        public Matrix4x4 distance;
        public Matrix4x4 clipping;
        public Matrix4x4 frustumMatrix;

        public Matrix4x4 projMatrix;
        public Matrix4x4 viewMatrix;
    }

    [Serializable]
    public class Projector
    {
        [SerializeField]
        [HideInInspector]
        public float nearClipPlane = 0.1f;
        [SerializeField]
        [HideInInspector]
        public float farClipPlane = 100.0f;
        [SerializeField]
        [HideInInspector]
        public float fieldOfView = 60.0f;
        [SerializeField]
        [HideInInspector]
        public float aspectRatio = 1;
        [SerializeField]
        [HideInInspector]
        public bool orthographic = false;
        [SerializeField]
        [HideInInspector]
        public float orthoGraphicSize = 10;

        private bool isDirty = true;

        public Transform transform;

        public void SetDirty()
        {
            isDirty = true;
        }

        private ProjectorSettings projectSettings;
        public ProjectorSettings GetProjectorSettings
        {
            get
            {
                if (isDirty)
                {
                    projectSettings = SetupProjectorSettings();
                }
                return projectSettings;
            }
        }

        // Start is called before the first frame update
        public void Awake(Transform trans)
        {
            transform = trans;
            SetDirty();
        }

        // Update is called once per frame
        public void Update()
        {
#if UNITY_EDITOR
            SetDirty();
#endif

        }

        public void Reset(Transform trans)
        {
            Awake(trans);
        }

        const float kSmallValue = 1.0e-8f;
        const float kSmallestNearClipMargin = 1.0e-2f;
        public void CheckConsistency()
        {
            if (orthographic)
            {
                float clipPlaneDiff = farClipPlane - nearClipPlane;
                if (Mathf.Abs(clipPlaneDiff) < kSmallestNearClipMargin)
                    farClipPlane = nearClipPlane + Mathf.Sign(clipPlaneDiff) * kSmallestNearClipMargin;
            }
            else
            {
                if (nearClipPlane < kSmallestNearClipMargin)
                    nearClipPlane = kSmallestNearClipMargin;

                if (farClipPlane < nearClipPlane + kSmallestNearClipMargin)
                    farClipPlane = nearClipPlane + kSmallestNearClipMargin;
            }

            if (Mathf.Abs(fieldOfView) < kSmallValue)
                fieldOfView = kSmallValue * Mathf.Sign(fieldOfView);

            if (Mathf.Abs(aspectRatio) < kSmallValue)
                aspectRatio = kSmallValue * Mathf.Sign(aspectRatio);

            if (Mathf.Abs(orthoGraphicSize) < kSmallValue)
                orthoGraphicSize = kSmallValue * Mathf.Sign(orthoGraphicSize);
        }

        public Matrix4x4 GetProjectorToPerspectiveMatrix()
        {
            Matrix4x4 zscale = new Matrix4x4();
            zscale = Matrix4x4.Scale(new Vector3(1, 1, -1));
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Matrix4x4 w2l = GetWorldToLocalMatrixNoScale();//Matrix4x4.TRS(-pos, rot, new Vector3(1, 1, 1));//SetTRInverse
            Matrix4x4 projectorToWorld = w2l;
            // CalculateProjectionMatrix() * zscale * projectorToWorld
            Matrix4x4 proj, temp, res;
            proj = CalculateProjectionMatrix();
            temp = proj * zscale;
            res = temp * projectorToWorld;
            return res;
        }

        private Matrix4x4 CalculateProjectionMatrix()
        {
            if (orthographic)
            {
                return Matrix4x4.Ortho(
                    -orthoGraphicSize * aspectRatio,
                    orthoGraphicSize * aspectRatio,
                    -orthoGraphicSize,
                    orthoGraphicSize,
                    nearClipPlane,
                    farClipPlane
                    );
            }
            else
            {
                return Matrix4x4.Perspective(
                    fieldOfView,
                    aspectRatio,
                    nearClipPlane,
                    farClipPlane);
            }
        }


        public Matrix4x4 GetWorldToLocalMatrixNoScale()
        {            
            Vector3 pos = new Vector3(-transform.position.x,-transform.position.y,-transform.position.z);
            Quaternion rot = Quaternion.Inverse(transform.rotation);
            //return Matrix4x4.TRS(pos, rot, new Vector3(1, 1, 1));
            return Matrix4x4.Rotate(rot) * Matrix4x4.Translate(pos);
        }


        public ProjectorSettings SetupProjectorSettings()
        {
            ProjectorSettings projectSettings = new ProjectorSettings();
            Matrix4x4 projectionMatrix = CalculateProjectionMatrix();

            Matrix4x4 zscale = Matrix4x4.Scale(new Vector3(1, 1, -1));
            //Vector3 pos = transform.position;
            //Quaternion rot = transform.rotation;
            //Matrix4x4 w2l = Matrix4x4.TRS(pos, rot, new Vector3(1, 1, 1));
            //Matrix4x4 projectorToWorld = w2l;
            Matrix4x4 projectorToWorld = GetWorldToLocalMatrixNoScale();
            // Setup the functor
            // projection matrix
            Matrix4x4 temp1, temp2, temp3, temp4;
            temp1 = Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 1.0f));
            temp2 = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0.0f));
            temp3 = temp2 * projectionMatrix;
            temp4 = temp3 * zscale;
            temp2 = temp4 * temp1;
            projectSettings.projection = temp2 * projectorToWorld;

            // X-axis fadeout matrix
            float scale = 1.0f / farClipPlane;
            temp1 = Matrix4x4.Scale(new Vector3(scale, scale, scale));
            temp2 = Matrix4x4.identity;
            temp2.m00 = 0; temp2.m01 = 0; temp2.m02 = 1; temp2.m00 = 0;
            // functor.distance = temp2 * temp1 * projectorToWorld
            temp3 = temp2 * temp1;
            projectSettings.distance = temp3 * projectorToWorld;

            // X-axis texture cull (use with an alpha map to do alpha-tested clip planes)
            scale = 1.0f / (farClipPlane - nearClipPlane);
            temp1 = Matrix4x4.Scale(new Vector3(scale, scale, scale));
            temp2 = Matrix4x4.identity;
            temp3 = Matrix4x4.Translate(new Vector3(-nearClipPlane, -nearClipPlane, -nearClipPlane));
            temp2.m00 = 0; temp2.m01 = 0; temp2.m02 = 1; temp2.m00 = 0;
            // functor.clipping = temp2 * temp1 * temp3 * projectorToWorld
            temp4 = temp2 * temp1;
            temp1 = temp4 * temp3;
            projectSettings.clipping = temp1 * projectorToWorld;

            temp1 = projectionMatrix * zscale;
            projectSettings.frustumMatrix = temp1 * projectorToWorld;

            //xy/w
            Matrix4x4 gpumatrix = GL.GetGPUProjectionMatrix(projectSettings.projection, false);
            //projectSettings.projectMatrixX = projectSettings.projection.GetColumn(0);
            //projectSettings.projectMatrixY = projectSettings.projection.GetColumn(1);
            //projectSettings.projectMatrixW = projectSettings.projection.GetColumn(3);
            projectSettings.projectMatrixX = projectSettings.projection.GetRow(0);
            projectSettings.projectMatrixY = projectSettings.projection.GetRow(1);
            projectSettings.projectMatrixW = projectSettings.projection.GetRow(3);
            gpumatrix = GL.GetGPUProjectionMatrix(projectSettings.clipping, false);
            //projectSettings.falloffR = projectSettings.clipping.GetRow(2);//z/w
            projectSettings.falloffR = gpumatrix.GetRow(2);//z/w

            {
                //view
                projectSettings.projMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, false);
                projectSettings.viewMatrix = zscale * transform.localToWorldMatrix.inverse;
            }

            return projectSettings;
        }
        static void InvertPerspective(ref Matrix4x4 proj, ref Matrix4x4 view, out Matrix4x4 vpinv)
        {
            Matrix4x4 invview;
            InvertView(ref view, out invview);

            Matrix4x4 invproj = Matrix4x4.zero;
            invproj.m00 = 1.0f / proj.m00;
            invproj.m03 = proj.m02 * invproj.m00;
            invproj.m11 = 1.0f / proj.m11;
            invproj.m13 = proj.m12 * invproj.m11;
            invproj.m22 = 0.0f;
            invproj.m23 = -1.0f;
            invproj.m33 = proj.m22 / proj.m23;
            invproj.m32 = invproj.m33 / proj.m22;

            vpinv = invview * invproj;
        }
        static void InvertView(ref Matrix4x4 view, out Matrix4x4 invview)
        {
            invview = Matrix4x4.zero;
            invview.m00 = view.m00; invview.m01 = view.m10; invview.m02 = view.m20;
            invview.m10 = view.m01; invview.m11 = view.m11; invview.m12 = view.m21;
            invview.m20 = view.m02; invview.m21 = view.m12; invview.m22 = view.m22;
            invview.m33 = 1.0f;
            invview.m03 = -(invview.m00 * view.m03 + invview.m01 * view.m13 + invview.m02 * view.m23);
            invview.m13 = -(invview.m10 * view.m03 + invview.m11 * view.m13 + invview.m12 * view.m23);
            invview.m23 = -(invview.m20 * view.m03 + invview.m21 * view.m13 + invview.m22 * view.m23);
        }


        public static SPlanes GetCullingPlanes(Matrix4x4 frustumMatrix,Matrix4x4 objTrans)
        {
            // From the objects that we are rendering from this camera.
            // Cull it further to the objects that overlap with the Projector frustum
            //Matrix4x4 zscale = Matrix4x4.Scale(new Vector3(1, 1, -1));
            Matrix4x4 finalMatrix = frustumMatrix * objTrans;
            return ExtractProjectionPlanes(finalMatrix);
        }
        public static void GetCullingPlanesAndSphere(out SPlanes planes,out AABB aabb,Matrix4x4 frustumMatrix, Matrix4x4 objTrans)
        {
            // From the objects that we are rendering from this camera.
            // Cull it further to the objects that overlap with the Projector frustum
            Matrix4x4 finalMatrix = frustumMatrix * objTrans;
            planes = ExtractProjectionPlanes(finalMatrix);
            aabb = ExtracetProjectionAABB(finalMatrix);
        }
        static Vector4[] PosInPSForAABB = new Vector4[8]
        {
            new Vector4(-1,-1,-1, 1),
            new Vector4(-1,-1,1, 1),
            new Vector4(-1,1,-1, 1),
            new Vector4(-1,1,1, 1),
            new Vector4(1,-1,-1, 1),
            new Vector4(1,-1,1, 1),
            new Vector4(1,1,-1, 1),
            new Vector4(1,1,1, 1)
        };
        unsafe static AABB ExtracetProjectionAABB(Matrix4x4 finalMatrix)
        {
            /*
             为了获取视锥在world space里的AABB，我们需要先获取其8个顶点在world space里的坐标。即project space中的八个顶点乘以(view*project)的逆矩阵
             八个顶点为：
             (-1,-1,-1,1),(-1,-1,1,1),
             (-1,1,-1,1),(-1,1,1,1),
             (1,-1,-1,1),(1,-1,1,1),
             (1,1,-1,1),(1,1,1,1)
             */
            Vector3* vectors = stackalloc Vector3[8];//unsafe
            vectors[0] = finalMatrix * PosInPSForAABB[0];
            vectors[1] = finalMatrix * PosInPSForAABB[1];
            vectors[2] = finalMatrix * PosInPSForAABB[2];
            vectors[3] = finalMatrix * PosInPSForAABB[3];
            vectors[4] = finalMatrix * PosInPSForAABB[4];
            vectors[5] = finalMatrix * PosInPSForAABB[5];
            vectors[6] = finalMatrix * PosInPSForAABB[6];
            vectors[7] = finalMatrix * PosInPSForAABB[7];

            Vector3 aabbMin = vectors[0];
            Vector3 aabbMax = vectors[0];
            for (int i = 1; i < 8; ++i)
            {
                aabbMin = Vector3.Min(aabbMin, vectors[i]);
                aabbMax = Vector3.Max(aabbMax, vectors[i]);
            }
            AABB aabb = new AABB();
            aabb.Min = new Vector4(aabbMin.x, aabbMin.y, aabbMin.z, 1.0f);
            aabb.Max = new Vector4(aabbMax.x, aabbMax.y, aabbMax.z, 1.0f);
            aabb.GenerateCenterAndExtent();
            return aabb;
        }

        static SPlanes ExtractProjectionPlanes( Matrix4x4 finalMatrix )
        {
            //https://www.zhihu.com/question/46377273/answer/101213293
            SPlanes outPlanes = new SPlanes();
            const int kPlaneFrustumLeft = 0;
            const int kPlaneFrustumRight = 1;
            const int kPlaneFrustumBottom = 2;
            const int kPlaneFrustumTop = 3;
            const int kPlaneFrustumNear = 4;
            const int kPlaneFrustumFar = 5;
            //const int kPlaneFrustumNum = 6;

            Vector4 tmpVec = new Vector4();
            Vector4 otherVec = new Vector4();
            tmpVec[0] = finalMatrix[3, 0];
            tmpVec[1] = finalMatrix[3, 1];
            tmpVec[2] = finalMatrix[3, 2];
            tmpVec[3] = finalMatrix[3, 3];

            otherVec[0] = finalMatrix[0, 0];
            otherVec[1] = finalMatrix[0, 1];
            otherVec[2] = finalMatrix[0, 2];
            otherVec[3] = finalMatrix[0, 3];
            // left & right
            outPlanes[kPlaneFrustumLeft] = NormalizeUnsafe(new Vector3(otherVec[0] + tmpVec[0], otherVec[1] + tmpVec[1], otherVec[2] + tmpVec[2]), otherVec[3] + tmpVec[3]);
            outPlanes[kPlaneFrustumRight] = NormalizeUnsafe(new Vector3(-otherVec[0] + tmpVec[0], -otherVec[1] + tmpVec[1], -otherVec[2] + tmpVec[2]), -otherVec[3] + tmpVec[3]);

            // bottom & top
            otherVec[0] = finalMatrix[1, 0];
	        otherVec[1] = finalMatrix[1, 1];
	        otherVec[2] = finalMatrix[1, 2];
	        otherVec[3] = finalMatrix[1, 3];

            outPlanes[kPlaneFrustumBottom] = NormalizeUnsafe(new Vector3(otherVec[0] + tmpVec[0], otherVec[1] + tmpVec[1], otherVec[2] + tmpVec[2]), otherVec[3] + tmpVec[3]);
            outPlanes[kPlaneFrustumTop] = NormalizeUnsafe(new Vector3(-otherVec[0] + tmpVec[0], -otherVec[1] + tmpVec[1], -otherVec[2] + tmpVec[2]), -otherVec[3] + tmpVec[3]);

            otherVec[0] = finalMatrix[2, 0];
	        otherVec[1] = finalMatrix[2, 1];
	        otherVec[2] = finalMatrix[2, 2];
	        otherVec[3] = finalMatrix[2, 3];
	
	        // near & far
            outPlanes[kPlaneFrustumNear] = NormalizeUnsafe(new Vector3(otherVec[0] + tmpVec[0], otherVec[1] + tmpVec[1], otherVec[2] + tmpVec[2]), otherVec[3] + tmpVec[3]);
            outPlanes[kPlaneFrustumFar] = NormalizeUnsafe(new Vector3(-otherVec[0] + tmpVec[0], -otherVec[1] + tmpVec[1], -otherVec[2] + tmpVec[2]), -otherVec[3] + tmpVec[3]);
            return outPlanes;
        }

        //Plane NormalizeUnsafe(Plane plane)
        //{
        //    float invMag = 1.0f / plane.normal.magnitude;// Magnitude(P);
        //    plane.normal *= invMag;
        //    plane.distance *= invMag;
        //    return plane;
        //}

        static Plane NormalizeUnsafe(Vector3 normal,float distance)
        {
            Plane plane = new Plane();
            float invMag = 1.0f / normal.magnitude;// Magnitude(P);
            plane.normal = normal * invMag;
            plane.distance = distance * invMag;
            return plane;
        }
#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            //Camera camera = Camera.current;
            //if((camera.cullingMask & gameObject.layer) == 0)
            //{
            //    return;// current camera does not render our layer - exit
            //}
            var defColor = Gizmos.color;
            Gizmos.color = Color.yellow;

            Matrix4x4 inverse = Matrix4x4.Inverse(GetProjectorToPerspectiveMatrix());

            float xmin = -1.0F;
            float xmax = 1.0F;
            float ymin = -1.0F;
            float ymax = 1.0F;

            Vector3 lbn, rbn, ltn, rtn, lbf, rbf, ltf, rtf;
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmin, ymax, -1.0f), out lbn);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmax, ymax, -1.0f), out rbn);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmin, ymin, -1.0f), out ltn);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmax, ymin, -1.0f), out rtn);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmin, ymax, 1.0f), out lbf);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmax, ymax, 1.0f), out rbf);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmin, ymin, 1.0f), out ltf);
            PerspectiveMultiplyPoint3(inverse, new Vector3(xmax, ymin, 1.0f), out rtf);

            // Front rectangle
            Gizmos.DrawLine(lbn, rbn);
            Gizmos.DrawLine(ltn, rtn);
            Gizmos.DrawLine(lbn, ltn);
            Gizmos.DrawLine(rbn, rtn);
            // back rectangle
            Gizmos.DrawLine(lbf, rbf);
            Gizmos.DrawLine(ltf, rtf);
            Gizmos.DrawLine(lbf, ltf);
            Gizmos.DrawLine(rbf, rtf);
            // near->far DrawLines
            Gizmos.DrawLine(lbn, lbf);
            Gizmos.DrawLine(ltn, ltf);
            Gizmos.DrawLine(rbn, rbf);
            Gizmos.DrawLine(rtn, rtf);

            Gizmos.color = defColor;
        }
        bool PerspectiveMultiplyPoint3(Matrix4x4 mat, Vector3 v, out Vector3 output)
        {
            Vector3 res;
            float w;
            res.x = mat[0, 0] * v.x + mat[0, 1] * v.y + mat[0, 2] * v.z + mat[0, 3];
            res.y = mat[1, 0] * v.x + mat[1, 1] * v.y + mat[1, 2] * v.z + mat[1, 3];
            res.z = mat[2, 0] * v.x + mat[2, 1] * v.y + mat[2, 2] * v.z + mat[2, 3];
            w = mat[3, 0] * v.x + mat[3, 1] * v.y + mat[3, 2] * v.z + mat[3, 3];
            if (Mathf.Abs(w) > 1.0e-7f)
            {
                float invW = 1.0f / w;
                output.x = res.x * invW;
                output.y = res.y * invW;
                output.z = res.z * invW;
                return true;
            }
            else
            {
                output.x = 0.0f;
                output.y = 0.0f;
                output.z = 0.0f;
                return false;
            }
        }
#endif
    }

}
