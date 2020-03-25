#ifndef __COMMON_POSITION_HLSL__
#define __COMMON_POSITION_HLSL__
#include "Common.hlsl"

struct PositionInputs
{
	float3 positionWS;  // World space position (could be camera-relative)
	float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
	uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
	//uint3  clusterIdx;   // Screen tile coordinates                             : [0, NumTiles)
	float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
	float  linearDepth; // View space Z coordinate                              : [Near, Far]
};


float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth)
{
	float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);

#if UNITY_UV_STARTS_AT_TOP
	// Our world space, view space, screen space and NDC space are Y-up.
	// Our clip space is flipped upside-down due to poor legacy Unity design.
	// The flip is baked into the projection matrix, so we only have to flip
	// manually when going from CS to NDC and back.
	positionCS.y = -positionCS.y;
#endif

	return positionCS;
}
float3 ComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)
{
	float4 positionCS = ComputeClipSpacePosition(positionNDC, deviceDepth);
	float4 hpositionWS = mul(invViewProjMatrix, positionCS);
	return hpositionWS.xyz / hpositionWS.w;
}

// Z buffer to linear depth.
// Correctly handles oblique view frustums.
// Does NOT work with orthographic projection.
// Ref: An Efficient Depth Linearization Method for Oblique View Frustums, Eq. 6.
float LinearEyeDepth(float2 positionNDC, float deviceDepth, float4 invProjParam)
{
	float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
	float  viewSpaceZ = rcp(dot(positionCS, invProjParam));

	// If the matrix is right-handed, we have to flip the Z axis to get a positive value.
	return abs(viewSpaceZ);
}

// Z buffer to linear depth.
// Does NOT correctly handle oblique view frustums.
// Does NOT work with orthographic projection.
// zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
float LinearEyeDepth(float depth, float4 zBufferParam)
{
	return 1.0 / (zBufferParam.z * depth + zBufferParam.w);
}

// Z buffer to linear depth.
// Works in all cases.
// Typically, this is the cheapest variant, provided you've already computed 'positionWS'.
// Assumes that the 'positionWS' is in front of the camera.
float LinearEyeDepth(float3 positionWS, float4x4 viewMatrix)
{
	float viewSpaceZ = mul(viewMatrix, float4(positionWS, 1.0)).z;

	// If the matrix is right-handed, we have to flip the Z axis to get a positive value.
	return abs(viewSpaceZ);
}
PositionInputs GetPositionInput(float2 positionSS, float2 invScreenSize, float deviceDepth,
	float4x4 invViewProjMatrix, float4x4 viewMatrix)
{
	PositionInputs posInput;
	ZERO_INITIALIZE(PositionInputs, posInput);
	posInput.positionNDC = positionSS;
	posInput.positionNDC *= invScreenSize;
	posInput.positionSS = uint2(positionSS);

	posInput.positionWS = ComputeWorldSpacePosition(posInput.positionNDC, deviceDepth, invViewProjMatrix);
	posInput.deviceDepth = deviceDepth;
	posInput.linearDepth = LinearEyeDepth(posInput.positionWS, viewMatrix);

	return posInput;
}
// From forward
// deviceDepth and linearDepth come directly from .zw of SV_Position
PositionInputs GetPositionInput(float2 positionSS, float4 screenSize,float deviceDepth, float linearDepth, float3 positionWS)
{
	PositionInputs posInput;
	ZERO_INITIALIZE(PositionInputs, posInput);
	posInput.positionNDC = positionSS;
	posInput.positionNDC *= screenSize.zw;
	posInput.positionSS = uint2(positionSS);
	posInput.positionSS.y = screenSize.y - posInput.positionSS;

	posInput.positionWS = positionWS;
	posInput.deviceDepth = deviceDepth;
	posInput.linearDepth = linearDepth;
	return posInput;
}

float4 GetScreenSize()
{
	return _IFScreenSize;
}

void InitilizeLightingInput(out lightingInput_t inputs)
{
	ZERO_INITIALIZE(lightingInput_t, inputs);
	inputs.normalTS = float3 (0.0, 0.0, 1.0);
	//inputs.normalSSS = float3 (0.0, 0.0, 1.0);
	inputs.alpha = 1;
	//inputs.ssdoDiffuseMul = 1;
	//inputs.invTS = float3x3 (
	//	1.0, 0.0, 0.0,
	//	0.0, 1.0, 0.0,
	//	0.0, 0.0, 1.0
	//	);
}



#endif