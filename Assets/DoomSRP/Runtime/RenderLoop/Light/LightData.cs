using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DoomSRP
{

    public static class LightDefins{

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

        public Plane this[int index] {
            get
            {
                switch(index)
                {
                    case 0:return left;
                    case 1: return Right; 
                    case 2: return Bottom;
                    case 3: return Top;
                    case 4: return Near; 
                    case 5: return Far;
                    default:Debug.LogError("Out of index");break;
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
    public struct SFiniteLightBound
    {
        //public Vector3 center;
        //public float radius;

#if UNITY_EDITOR
        public Matrix4x4 frustumMatrix;
#endif
        public SPlanes planes;//解决了数组问题。。。
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightData
    {
        public Vector3 pos;
        public uint lightParms;//[2] shadow?,[5] circle area light,[6] rect area light,[8] diffuse?
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
    };

    public struct LightDataInAll
    {
        public LightData lightData;
        public SFiniteLightBound sFiniteLightBound;
    }

}