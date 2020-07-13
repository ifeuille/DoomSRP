using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DoomSRP
{

    public static class LightDefins
    {

        public static int MaxItemsPerCluster = 32;
        public static int NumClusterX = 16;
        public static int NumClusterY = 8;
        public static int NumClusterZ = 24;

    }

    //
    // 摘要:
    //     Representation of RGBA colors in 32 bit format.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Color16
    {
        public byte r;
        public byte g;


        public Color16(byte r, byte g)
        {
            this.r = r;
            this.g = g;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct clusternumlights_t
    {
        public int offset;
        public int numItems;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector4Int
    {
        public int x;
        public int y;
        public int z;
        public int w;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector4UInt
    {
        public uint x;
        public uint y;
        public uint z;
        public uint w;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AABB
    {
        public Vector4 Min;
        public Vector4 Max;
        public Vector3 m;
        public Vector3 extent;

        public void GenerateCenterAndExtent()
        {
            m = (Min + Max) * 0.5f;
            extent = new Vector3(Mathf.Abs(Min.x - Max.x) / 2.0f, Mathf.Abs(Min.y - Max.y) / 2.0f, Mathf.Abs(Min.z - Max.z) / 2.0f);
        }
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SPlanes
    {
        public Plane left;
        public Plane Right;
        public Plane Bottom;
        public Plane Top;
        public Plane Near;
        public Plane Far;

        public Plane this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return left;
                    case 1: return Right;
                    case 2: return Bottom;
                    case 3: return Top;
                    case 4: return Near;
                    case 5: return Far;
                    default: Debug.LogError("Out of index"); break;
                }
                return new Plane();
            }
            set
            {
                switch (index)
                {
                    case 0: left = value; break;
                    case 1: Right = value; break;
                    case 2: Bottom = value; break;
                    case 3: Top = value; break;
                    case 4: Near = value; break;
                    case 5: Far = value; break;
                    default: Debug.LogError("Out of index"); break;
                }
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Sphere
    {
        public Vector3 Center;
        public float R;
        public float R2;//pow(R,2)
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SFiniteLightBound
    {
        //public Vector3 center;
        //public float radius;

        public SPlanes planes;//解决了数组问题。。。
        public AABB aabb;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct shadowparms_t
    {
        public Matrix4x4 shadowLight;
        public Vector4 shadowAtlasScaleBias;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightData
    {
        public Vector3 pos;
        //[2] shadow?,[5] circle area light,[6] rect area light,[8] diffuse?
        //[17~21]:/63 shadow_fade
        //[31~22]:灯光索引
        public uint lightParms;
        public Vector4 posShadow;
        public Vector4 falloffR;
        public Vector4 projS;
        public Vector4 projT;
        public Vector4 projQ;
        //xy falloffScaleBias,zw projFilterScaleBias
        public Vector4UInt scaleBias;
        public Vector4 boxMin;
        public Vector4 boxMax;
        public Vector4 areaPlane;//worldspace中区域光所在平面xyz法向量,w距离
        public uint lightParms2;
        public uint colorPacked;//unpackRGBE
        public float specMultiplier;
        public float shadowBleedReduce;
        //todo 用colorPacked优化
        //Vector4 color;

        public bool shadow()
        {
            return ((lightParms & 0x10) != 0);
        }

    };
    public struct LightData_Shadow
    {
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projMatrix;

        public SPlanes planes;
    }

    public struct LightDataInAll
    {
        public LightData lightData;
        public SFiniteLightBound sFiniteLightBound;
        public LightData_Shadow shadowData;
    }

    public struct LightDataForShadow
    {
        public int lightIndex;
        public int unityLightIndex;
        public LightData_Shadow shadowData;

        public VisibleLight visibleLight;
        //public ProjectorLight projectorLight;
    }
}