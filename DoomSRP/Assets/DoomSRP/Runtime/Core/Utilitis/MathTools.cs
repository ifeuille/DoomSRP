using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoomSRP
{
    public static class MathTools
    {
        public static Vector4 Clamp(Vector4 v,float min,float max)
        {
            v.x = Mathf.Clamp(v.x, min, max);
            v.y = Mathf.Clamp(v.y, min, max);
            v.z = Mathf.Clamp(v.z, min, max);
            v.w = Mathf.Clamp(v.w, min, max);
            return v;
        }

        public static Vector4 Clamp01(Vector4 v)
        {
            v.x = Mathf.Clamp01(v.x);
            v.y = Mathf.Clamp01(v.y);
            v.z = Mathf.Clamp01(v.z);
            v.w = Mathf.Clamp01(v.w);
            return v;
        }

        //log2(x)
        public static float ApproxLog2(float param)
        {
            return (((float)TypeTools.asuint(param)) / 8388608.0f) - 127.0f;
        }

        //pow(x,2)
        public static float ApproxExp2(float f)
        {
            uint param = (uint)((f + 127.0f) * 8388608.0f);
            return TypeTools.asfloat(param);
        }

        public static Vector3 GetCenter(Vector3 min, Vector3 max)
        {
            return (min + max) * 0.5f;
        }
        public static Vector3 GetExtent(Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Abs(min.x - max.x) / 2.0f, Mathf.Abs(min.y - max.y) / 2.0f, Mathf.Abs(min.z - max.z) / 2.0f);
        }
    }
}
