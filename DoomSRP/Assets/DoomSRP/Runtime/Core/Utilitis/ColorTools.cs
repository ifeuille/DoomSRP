using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoomSRP
{
    public static class ColorTools
    {
		public static float ToTemperature(Color color)
        {
            Matrix4x4 mat = new Matrix4x4(
                new Vector4(3.2404542f, -1.5371385f, -0.4985314f, 0.0f),
                new Vector4(-0.9692660f, 1.8760108f, 0.0415560f, 0.0f),
                new Vector4(0.0556434f, -0.2040259f, 1.0572252f, 0.0f),
                new Vector4(0, 0, 0, 1.0f)
            );
            var invMat = mat.inverse;

            Vector4 XYZ = invMat * color.ToVector4();
            float Y = XYZ.y;
            float X = XYZ.x/ Y;
            float Z = XYZ.z/ Y;

            float y = 1.0f / (X + Y + 1);
            float x = y * X;

            float v = 6.0f * y / (3.0f - 2.0f * x + 12.0f * y);
            float u = (8.0f * x * v - 4.0f * x) / (2.0f * x - 3.0f);

            float a = u * 7.08145163e-7f - 1.28641212e-7f;
            float b = u * 8.42420235e-4f - 1.54118254e-4f;
            float c = u - 0.860117757f;
            float delta = b * b - 4.0f * a * c;
            float sqrtDelta = Mathf.Sqrt(Mathf.Abs(delta));
            float t1 = (-b + sqrtDelta) / 2.0f * a;
            if (t1 > 0) return t1;
            float t2 = (-b - sqrtDelta) / 2.0f * a;
            return t2;
        }
        public static Color MakeFromColorTemperature(float Temp)
        {
            Temp = Mathf.Clamp(Temp, 1000.0f, 15000.0f);

            // Approximate Planckian locus in CIE 1960 UCS
            float u = (0.860117757f + 1.54118254e-4f * Temp + 1.28641212e-7f * Temp * Temp) / (1.0f + 8.42420235e-4f * Temp + 7.08145163e-7f * Temp * Temp);
            float v = (0.317398726f + 4.22806245e-5f * Temp + 4.20481691e-8f * Temp * Temp) / (1.0f - 2.89741816e-5f * Temp + 1.61456053e-7f * Temp * Temp);

            float x = 3.0f * u / (2.0f * u - 8.0f * v + 4.0f);
            float y = 2.0f * v / (2.0f * u - 8.0f * v + 4.0f);
            float z = 1.0f - x - y;

            float Y = 1.0f;
            float X = Y / y * x;
            float Z = Y / y * z;

            // XYZ to RGB with BT.709 primaries
            float R = 3.2404542f * X + -1.5371385f * Y + -0.4985314f * Z;
            float G = -0.9692660f * X + 1.8760108f * Y + 0.0415560f * Z;
            float B = 0.0556434f * X + -0.2040259f * Y + 1.0572252f * Z;

            return new Color(R, G, B);
        }

        public static Color GetColorFromIntensityAndTemperature(Color c,float intensity,float temperature,bool useTemperature)
        {
            /*
             finalColor	= Color*(Intensity/(4*Π))\*Π  
                        = Color*Intensity/4
                        = 1,1,1)*3.14/4 =(0.78,0.78,0.78)
             if use temperature then
                finalColor *= MakeFromColorTemperature(temperature)
             end
             */
            Color finalColor = c;
            finalColor *= (intensity * 0.25f);
            if (useTemperature)
            {
                finalColor *= ColorTools.MakeFromColorTemperature(temperature);
            }
            return finalColor;
        }

        //public static uint PackColorRGB(Color c)
        //{
        //    uint packed = 0;


        //    return packed;
        //}

        public static Vector4 ToRGBA(uint hex)
        {
            return new Vector4(
                ((hex >> 16) & 0xff) / 255f, 
                ((hex >> 8) & 0xff) / 255f, 
                (hex & 0xff) / 255f, 
                ((hex >> 24) & 0xff) / 255f);
        }

        public static uint packR8G8B8A8(Vector4 rgba)
        {
            rgba = MathTools.Clamp01(rgba);
            uint r = (uint)(rgba.x * 255.0) << 24;
            uint g = (uint)(rgba.y * 255.0) << 16;
            uint b = (uint)(rgba.z * 255.0) << 8;
            uint a = (uint)(rgba.w * 255.0) ;
            return r | g | b | a;
        }

        public static uint packRGBE(Vector3 value)
        {
            float param = Mathf.Max(Mathf.Max(value.x, value.y), value.z);
            float sharedExp = Mathf.Ceil(MathTools.ApproxLog2(param));
            float param_1 = sharedExp;
            float exp2 = MathTools.ApproxExp2(param_1);
            Vector4 param_2 = new Vector4(value.x/ exp2,value.y/ exp2,value.z/ exp2, (sharedExp + 128.0f) / 255.0f);
            Vector4 param_3 = MathTools.Clamp01(param_2);
            uint _635 = packR8G8B8A8(param_3);
            return _635;
        }

        public static Vector4 unpackR8G8B8A8(uint value)
        {
            float r = ((value >> 24) & 255u);
            float g = ((value >> 16) & 255u);
            float b = ((value >> 8) & 255u);
            float a = (value & 255u);
            return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public static Color32 unpackR8G8B8A8_Color32(uint value)
        {
            byte r = (byte)((value >> 24) & 255u);
            byte g = (byte)((value >> 16) & 255u);
            byte b = (byte)((value >> 8) & 255u);
            byte a = (byte)(value & 255u);
            return new Color32(r,g,b,a);
        }

        public static Vector3 unpackRGBE(uint value)
        {
            uint param = value;
            Vector4 rgbe = unpackR8G8B8A8(param);
            float param_1 = (rgbe.w * 255.0f) - 128.0f;
            float exp2 = MathTools.ApproxExp2(param_1);
            return new Vector3(exp2 * rgbe.x, exp2 * rgbe.y, exp2 * rgbe.z);
        }

        public static Vector3 Color2Vec3(Color c)
        {
            return new Vector3(c.r, c.g, c.b);
        }

        public static Vector4 Color2Vec4(Color c)
        {
            return new Vector4(c.r, c.g, c.b, c.a);
        }

        public static Color Vec2Color(Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }

        public static Color Vec2Color(Vector3 v)
        {
            return new Color(v.x, v.y, v.z, 1);
        }

        public static Vector4 unpackR15G15B15A15(uint a, uint b)
        {
            return new Vector4(
                (float)((a >> 16) & 65535u),
                (float)(a & 65535u),
                (float)((b >> 16) & 65535u),
                (float)(b & 65535u)) * 3.0518509447e-05f;
        }

        public static void packR15G15B15A15(out uint a,out uint b,Vector4 value)
        {
            a = ((uint)(value.x / (3.0518509447e-05f)) & 65535u) << 16;
            a |= ((uint)(value.y / (3.0518509447e-05f)) & 65535u);
            b = ((uint)(value.z / (3.0518509447e-05f)) & 65535u) << 16;
            b |= ((uint)(value.w / (3.0518509447e-05f)) & 65535u);
        }

        public static void packR15G15B15A15(out int a, out int b, Vector4 value)
        {
            a = ((int)(value.x / (3.0518509447e-05f)) & 65535) << 16;
            a |= ((int)(value.y / (3.0518509447e-05f)) & 65535);
            b = ((int)(value.z / (3.0518509447e-05f)) & 65535) << 16;
            b |= ((int)(value.w / (3.0518509447e-05f)) & 65535);
        }
    }
}
