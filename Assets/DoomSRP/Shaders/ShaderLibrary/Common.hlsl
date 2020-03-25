#ifndef IFPIPELINE_COMMON_HLSL_
#define IFPIPELINE_COMMON_HLSL_
#define ZERO_INITIALIZE(type, name) name = (type)0;
#include "MathTools.hlsl"
//最大32
#define MaxItemsPerCluster 32
#define MAXITEMSONSCREEN 512


CBUFFER_START(DoomSRPCommonBuffer)
	float4 _IFScreenSize;// { w, h, 1 / w, 1 / h }
CBUFFER_END

//用于光照计算的中间结构体
struct lightingInput_t
{
	half3 debugColor;
	float3 albedo;
	//float3 colorMask;
	float3 specular;
	float smoothness;
	//float maskSSS;
	//float thickness;
	float3 normal;//WS
	float3 normalTS;
	float3 normalSSS;
	//float3 lightmap;//?
	half3 emissive;
	float alpha;
	//float ssdoDiffuseMul;
	half3 view;
	float3 position;
	float4 texCoords;
	float4 fragCoord;
	//float3x3 invTS;
	half3 vertexNormal;
	float3 ambient_lighting;
	float3 diffuse_lighting;
	float3 specular_lighting;
	float3 output_lighting;
	uint albedo_packed;
	uint specular_packed;
	uint diffuse_lighting_packed;
	uint specular_lighting_packed;
	//uint normal_packed;
	//uint4 ticksDecals;
	//uint4 ticksProbes;
	//uint4 ticksLights;
};

float dot2 (float2 a, float2 b)
{
	return dot (a, b);
}


float SRGBlinear (float value)
{
	if (value <= 0.040449999272823333740234375)
	{
		return value / 12.9200000762939453125;
	}
	else
	{
		return pow ((value / 1.05499994754791259765625) + 0.0521326996386051177978515625, 2.400000095367431640625);
	}
}

float DeGamma (float value)
{
	float param = value;
	return SRGBlinear (param);
}

//float saturate (float v)
//{
//	return clamp (v, 0.0, 1.0);
//}

//float2 screenPosToTexcoord (float2 pos, float4 bias_scale)
//{
//	return (pos * bias_scale.zw) + bias_scale.xy;
//}

//float asfloat (uint x)
//{
//	return uintBitsToFloat (x);
//}


float3 ReconstructNormal (float3 normalTS, bool isFrontFacing)
{
	float3 N = (normalTS * 2.0) - float3 (1.0,1.0,1.0);
	float param = (1.0 - (N.x * N.x)) - (N.y * N.y);
	N.z = sqrt (saturate (param));
	if (isFrontFacing == false)
	{
		N.z = -N.z;
	}
	return N;
}

float3 TransformNormal (float3 normal, float3x3 mat)
{
	return normalize (float3 (dot (normal, mat[0]), dot (normal, mat[1]), dot (normal, mat[2])));
}

float3 GetWorldSpaceNormal (float3 normalTS, float3x3 invTS, bool isFrontFacing)
{
	float3 param = normalTS;
	bool param_1 = isFrontFacing;
	float3 N = ReconstructNormal (param, param_1);
	float3 param_2 = N;
	float3x3 param_3 = invTS;
	return TransformNormal (param_2, param_3);
}

//uint asuint (float x)
//{
//	return floatBitsToUint (x);
//}

float fmin3 (float f1, float f2, float f3)
{
	return min (f1, min (f2, f3));
}

float fmax3 (float f1, float f2, float f3)
{
	return max (f1, max (f2, f3));
}

uint packR10G10B10(inout float3 value)
{
	float3 param = value;
	value = saturate(param);
	return ((uint(value.x * 1023.0) << uint(20)) | (uint(value.y * 1023.0) << uint(10))) | uint(value.z * 1023.0);
}

float4 unpackR15G15B15A15 (uint2 value)
{
	return float4 (
		float ((value.x >> uint(16)) & 65535u),
		float (value.x & 65535u),
		float ((value.y >> uint(16)) & 65535u),
		float (value.y & 65535u))
		* 3.0518509447574615478515625e-05;
}


inline float4 unpackR8G8B8A8 (uint value)
{
	/*float4 ret;
	ret.r = float(int_bitfieldExtract (value, 24, 8)) * 0.00392156862745;
	ret.g = float(int_bitfieldExtract (value, 16, 8)) * 0.00392156862745;
	ret.b = float(int_bitfieldExtract (value, 8, 8)) * 0.00392156862745;
	ret.a = float(value & 255u) * 0.00392156862745;
	return ret;*/
	//int v = int (value);
	//return float4 (
	//	float ((uint(value) >> 24) & 255),
	//	float ((uint (value) >> 16) & 255),
	//	float ((uint (value) >> 8) & 255),
	//	float (value & 255u)) * 0.00392156862745;
	return float4 (float ((value >> uint(24)) & 255u), float ((value >> uint(16)) & 255u), float ((value >> uint(8)) & 255u), float (value & 255u)) / float4 (255.0, 255.0, 255.0, 255.0);
}

float3 unpackR10G10B10(uint value)
{
	/*float3 ret;
	ret.r = ((float)(int_bitfieldExtract(value, 20, 10))) * 0.00097751710654936;
	ret.g = ((float)(int_bitfieldExtract(value, 10, 10))) * 0.00097751710654936;
	ret.b = ((float)(value & 1023)) * 0.00097751710654936;
	return ret;*/
	//return float3 (
	//	float ((uint (value) >> 20) & 1023),
	//	float ((uint (value) >> 10) & 1023),
	//	float (value & 1023u))* 0.00097751710654936;
	return float3 (float((value >> uint(20)) & 1023u), float((value >> uint(10)) & 1023u), float(value & 1023u)) / float3 (1023.0, 1023.0, 1023.0);
}

//uint R8G8B8A8touint(float4 color)
//{
//	return color.r * 255 << 24 + color.g * 255 << 16 + color.b * 255 << 8 + color.a * 255;
//}

//int asint (float x)
//{
//	return floatBitsToInt (x);
//}
//
//float asfloat (int x)
//{
//	return intBitsToFloat (x);
//}

float sqrtIEEEIntApproximation (float inX, int inSqrtConst)
{
	float param = inX;
	int x = asint (param);
	x = inSqrtConst + (x >> 1);
	int param_1 = x;
	return asfloat (param_1);
}

float fastSqrtNR0(float inX)
{
	float param = inX;
	int param_1 = 532487669;
	return sqrtIEEEIntApproximation(param, param_1);
}

float GetLuma (float3 c)
{
	return dot (c, float3 (0.2125999927520751953125, 0.715200006961822509765625, 0.072200000286102294921875));
}

float ApproxLog2 (float f)
{
	float param = f;
	return (float (asuint (param)) / 8388608.0) - 127.0;
}

float ApproxExp2 (float f)
{
	uint param = uint((f + 127.0) * 8388608.0);
	return asfloat (param);
}

uint packR8G8B8A8 (inout float4 value)
{
	//float4 param = value;
	//value = saturate (param);
	//return ((
	//	(uint(value.x * 255.0 + 0.5) << uint(24)) 
	//	| (uint(value.y * 255.0 + 0.5) << uint(16)))
	//	| (uint(value.z * 255.0 + 0.5) << uint(8)))
	//	| uint(value.w * 255.0 + 0.5);
	return (((uint(value.x * 255.0 ) << uint(24)) | (uint(value.y * 255.0) << uint(16))) | (uint(value.z * 255.0) << uint(8))) | uint(value.w * 255.0);
}

uint packRGBE (float3 value)
{
	float param = max (max (value.x, value.y), value.z);
	float sharedExp = ceil (ApproxLog2 (param));
	float exp2value = ApproxExp2 (sharedExp);
	float4 param_2 = float4 (value.x / exp2value, value.y / exp2value, value.z / exp2value, (sharedExp + 128.0) / 255.0);
	float4 param_3 = saturate (param_2);
	uint _635 = packR8G8B8A8 (param_3);
	return _635;
}

uint getCubeMapFaceID (float x, float y, float z)
{
	float _334 = z;
	float _336 = x;
	bool _338 = abs (_334) >= abs (_336);
	bool _346;
	if (_338)
	{
		_346 = abs (z) >= abs (y);
	}
	else
	{
		_346 = _338;
	}
	if (_346)
	{
		int _349;
		if (z < 0.0)
		{
			_349 = 5;
		}
		else
		{
			_349 = 4;
		}
		return uint(_349);
	}
	if (abs (y) >= abs (x))
	{
		int _367;
		if (y < 0.0)
		{
			_367 = 3;
		}
		else
		{
			_367 = 2;
		}
		return uint(_367);
	}
	int _378;
	if (x < 0.0)
	{
		_378 = 1;
	}
	else
	{
		_378 = 0;
	}
	return uint(_378);
}

//float shadow2Ddepth (sampler2DShadow image, float3 texcoord)
//{
//	return textureLod (image, float3 (texcoord.xy, texcoord.z), 0.0);
//}

float3 unpackRGBE (uint value)
{
	float4 rgbe = unpackR8G8B8A8 (value);
	float param_1 = (rgbe.w * 255.0) - 128.0;
	return rgbe.xyz * ApproxExp2 (param_1);
}


float ApproxPow (float fBase, float fPower)
{
	uint param_1 = uint((fPower * float (asuint (fBase))) - (((fPower - 1.0) * 127.0) * 8388608.0));
	return asfloat (param_1);
}

float3 fresnelSchlick (float3 f0, float costheta)
{
	float param = 50.0 * dot (f0, float3 (0.33329999446868896484375, 0.33329999446868896484375, 0.33329999446868896484375));
	float baked_spec_occl = saturate (param);
	float param_1 = 1.0 - costheta;
	float param_2 = saturate (param_1);
	float param_3 = 5.0;
	float3 param_4 = f0 + ((float3 (baked_spec_occl, baked_spec_occl, baked_spec_occl) - f0) * ApproxPow (param_2, param_3));
	return saturate (param_4);
}

float3 specBRDF (float3 N, float3 V, float3 L, float3 f0, float smoothness)
{
	float3 H = normalize (V + L);
	float m = 1.0 - (smoothness * 0.800000011920928955078125);
	m *= m;
	m *= m;
	float m2 = m * m;
	float param = dot (N, H);
	float NdotH = saturate (param);
	float spec = ((NdotH * NdotH) * (m2 - 1.0)) + 1.0;
	spec = m2 / ((spec * spec) + 9.9999999392252902907785028219223e-09);
	float param_1 = dot (N, V);
	float Gv = (saturate (param_1) * (1.0 - m)) + m;
	float param_2 = dot (N, L);
	float Gl = (saturate (param_2) * (1.0 - m)) + m;
	spec /= (((4.0 * Gv) * Gl) + 9.9999999392252902907785028219223e-09);
	float3 param_3 = f0;
	float param_4 = dot (L, H);
	return fresnelSchlick (param_3, param_4) * spec;
}

float2 OctWrap (float2 v)
{
	float _913;
	if (v.x >= 0.0)
	{
		_913 = 1.0;
	}
	else
	{
		_913 = -1.0;
	}
	float _921 = _913;
	float _922;
	if (v.y >= 0.0)
	{
		_922 = 1.0;
	}
	else
	{
		_922 = -1.0;
	}
	return (float2 (1.0, 1.0) - float2(abs (v.y), abs (v.x))) * float2 (_921, _922);
}

float2 NormalOctEncode (inout float3 n, bool compress_range)
{
	float value = (abs (n.x) + abs (n.y)) + abs (n.z);
	n /= float3 (value,value,value);
	float2 _948;
	if (n.z >= 0.0)
	{
		_948 = n.xy;
	}
	else
	{
		float2 param = n.xy;
		_948 = OctWrap (param);
	}
	n = float3 (_948.x, _948.y, n.z);
	if (compress_range)
	{
		float2 _972 = (n.xy * 0.5) + float2 (0.5, 0.5);
		n = float3 (_972.x, _972.y, n.z);
	}
	return n.xy;
}

float SmoothnessEncode (float s)
{
	float param = abs (s);
	float s1 = fastSqrtNR0 (param);
	float _984;
	if (s > 0.0)
	{
		_984 = s1;
	}
	else
	{
		_984 = -s1;
	}
	return (_984 * 0.5) + 0.5;
}

half3 LerpWhiteTo(half3 b, half t)
{
	half oneMinusT = 1 - t;
	return half3(oneMinusT, oneMinusT, oneMinusT) + b * t;
}


#endif