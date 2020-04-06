#ifndef DOOMSRP_SHADERUTILITIES_H
#define DOOMSRP_SHADERUTILITIES_H
#include "Input.hlsl"
// Tranforms position from object to homogenous space
inline float4 UnityObjectToClipPos(in float3 pos)
{
#if defined(STEREO_CUBEMAP_RENDER_ON)
	return UnityObjectToClipPosODS(pos);
#else
	// More efficient than computing M*VP matrix product
	return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
#endif
}
inline float4 UnityObjectToClipPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
	return UnityObjectToClipPos(pos.xyz);
}

#endif
