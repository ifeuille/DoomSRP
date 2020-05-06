#ifndef DOOMSRP_ATLAS_SHADOWS_HLSL
#define DOOMSRP_ATLAS_SHADOWS_HLSL
#include "Common.hlsl"
#include "Assets/DoomSRP/Shaders/core/CommonMaterial.hlsl"

struct shadowparms_t
{
	float4x4 shadowLight;
	/*float4 shadowLightS;
	float4 shadowLightT;
	float4 shadowLightR;
	float4 shadowLightQ;*/
	float4 shadowAtlasScaleBias;
};

StructuredBuffer<shadowparms_t> _ShadowsParms;
//StructuredBuffer<float4x4> _LightsWorldToShadow;

TEXTURE2D_SHADOW (_LightsShadowmapTexture);
SAMPLER_CMP (sampler_LightsShadowmapTexture);
CBUFFER_START(_ClusterLightsShadowBuffer)
half4       _ClusterShadowOffset0;
half4       _ClusterShadowOffset1;
half4       _ClusterShadowOffset2;
half4       _ClusterShadowOffset3;
float4      _ClusterShadowmapSize; // (xy: 1/width and 1/height, zw: width and height)
CBUFFER_END

#if UNITY_REVERSED_Z
#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= UNITY_RAW_FAR_CLIP_VALUE
#else
#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z >= UNITY_RAW_FAR_CLIP_VALUE
#endif
struct ShadowSamplingData
{
	half4 shadowOffset0;
	half4 shadowOffset1;
	half4 shadowOffset2;
	half4 shadowOffset3;
	float4 shadowmapSize;
};
ShadowSamplingData GetClusterLightShadowSamplingData()
{
	ShadowSamplingData shadowSamplingData;
	shadowSamplingData.shadowOffset0 = _ClusterShadowOffset0;
	shadowSamplingData.shadowOffset1 = _ClusterShadowOffset1;
	shadowSamplingData.shadowOffset2 = _ClusterShadowOffset2;
	shadowSamplingData.shadowOffset3 = _ClusterShadowOffset3;
	shadowSamplingData.shadowmapSize = _ClusterShadowmapSize;
	return shadowSamplingData;
}

real SampleShadowmap(float4 shadowCoord, TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), ShadowSamplingData samplingData, half shadowStrength, bool isPerspectiveProjection = true)
{
	// Compiler will optimize this branch away as long as isPerspectiveProjection is known at compile time
	if (isPerspectiveProjection)
		shadowCoord.xyz /= shadowCoord.w;

	real attenuation;

#ifdef _SHADOWS_SOFT
#ifdef SHADER_API_MOBILE
	// 4-tap hardware comparison
	real4 attenuation4;
	attenuation4.x = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset0.xyz);
	attenuation4.y = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset1.xyz);
	attenuation4.z = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset2.xyz);
	attenuation4.w = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset3.xyz);
	attenuation = dot(attenuation4, 0.25);
#else
	float fetchesWeights[9];
	float2 fetchesUV[9];
	SampleShadow_ComputeSamples_Tent_5x5(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

	attenuation = fetchesWeights[0] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[0].xy, shadowCoord.z));
	attenuation += fetchesWeights[1] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[1].xy, shadowCoord.z));
	attenuation += fetchesWeights[2] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[2].xy, shadowCoord.z));
	attenuation += fetchesWeights[3] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[3].xy, shadowCoord.z));
	attenuation += fetchesWeights[4] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[4].xy, shadowCoord.z));
	attenuation += fetchesWeights[5] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[5].xy, shadowCoord.z));
	attenuation += fetchesWeights[6] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[6].xy, shadowCoord.z));
	attenuation += fetchesWeights[7] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[7].xy, shadowCoord.z));
	attenuation += fetchesWeights[8] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[8].xy, shadowCoord.z));
#endif
#else
	// 1-tap hardware comparison
	attenuation = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz);
#endif

	attenuation = LerpWhiteTo(attenuation, shadowStrength);

	// Shadow coords that fall out of the light frustum volume must always return attenuation 1.0
	real r = BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;
#if UNITY_REVERSED_Z
	r = 1 - r;
#endif
	return r;
}

real GetShadowMask(lightingInput_t inputs, uint light_parms)
{
	real normal_bias_scale = 0.5;// This is important
	float4 pos;
	pos.xyz = inputs.position +(inputs.normal * normal_bias_scale);
	pos.w = 1;
	shadowparms_t shadow_parms = _ShadowsParms[(light_parms >> uint(22))];
	float4 shadowCoord = mul(shadow_parms.shadowLight, pos);
	//shadowCoord = float4(shadowTC_1.xyz / shadowTC_1.w, shadowTC_1.w);
	ShadowSamplingData shadowSamplingData = GetClusterLightShadowSamplingData();
	half shadowStrength = 1;
	return SampleShadowmap(shadowCoord, TEXTURE2D_PARAM(_LightsShadowmapTexture, sampler_LightsShadowmapTexture),
		shadowSamplingData, shadowStrength, true);
}


/*uint light_parms
&4:是否shadow
&3 cube map? 2：
17~21:/63  shadow_fade
31~22 灯光索引
*/
float GetShadowMask1 (lightingInput_t inputs, uint light_parms)
{
	float shadow = 1.0;
	float normal_bias_scale = 0.5;
	float4 pos;
	pos.xyz = inputs.position + (inputs.normal * normal_bias_scale);
	pos.w = 1;
#if 0

	shadowparms_t shadow_parms = _ShadowsParms[(light_parms >> uint(22))];
	float4 shadowTC_1 = mul (shadow_parms.shadowLight, pos);
	shadowTC_1 = float4(shadowTC_1.xyz / shadowTC_1.w, shadowTC_1.w);
#else
	float4 shadowTC_1 = mul (_ShadowsParms[(light_parms >> uint(22))].shadowLight, pos);
	shadowTC_1 = float4(shadowTC_1.xyz / shadowTC_1.w, shadowTC_1.w);
#endif
	if (shadowTC_1.x > 0 && shadowTC_1.x < 1 && shadowTC_1.y > 0 && shadowTC_1.y < 1)
	{ 
		//shadowTC_1.xy = (shadowTC_1.xy * shadow_parms.shadowAtlasScaleBias.xy) + shadow_parms.shadowAtlasScaleBias.zw;
#ifndef _SHADOWS_SOFT
		real4 attenuation4;
		attenuation4.x = SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, shadowTC_1.xyz);
		attenuation4.y = SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, shadowTC_1.xyz);
		attenuation4.z = SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, shadowTC_1.xyz);
		attenuation4.w = SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, shadowTC_1.xyz);
		shadow = dot (attenuation4, 0.25);
#else
		float2 uv = shadowTC_1.xy * _ShadowAtlasResolution.xy;
		float2 shadowMapSizeInv = _ShadowAtlasResolution.zw;
		float2 base_uv = float2 (floor (uv.x + 0.5), floor (uv.y + 0.5));
		float s = (uv.x + 0.5) - base_uv.x;
		float t = (uv.y + 0.5) - base_uv.y;
		base_uv -= float2 (0.5);
		base_uv *= shadowMapSizeInv;
		float uw0 = 4.0 - (3.0 * s);
		float uw1 = 7.0;
		float uw2 = 1.0 + (3.0 * s);
		float u0 = ((3.0 - (2.0 * s)) / uw0) - 2.0;
		float u1 = (3.0 + s) / uw1;
		float u2 = (s / uw2) + 2.0;
		float vw0 = 4.0 - (3.0 * t);
		float v0 = ((3.0 - (2.0 * t)) / vw0) - 2.0;
		float3 param_149 = float3 (base_uv + (float2 (u0, v0) * shadowMapSizeInv), shadowTC_1.z);
		float s0 = (uw0 * vw0) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_149.xyz); ;
		float3 param_150 = float3 (base_uv + (float2 (u1, v0) * shadowMapSizeInv), shadowTC_1.z);
		float s1 = (uw1 * vw0) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_150);
		float3 param_151 = float3 (base_uv + (float2 (u2, v0) * shadowMapSizeInv), shadowTC_1.z);
		float s2 = (uw2 * vw0) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_151);
		shadow = (s0 + s1) + s2;
		float vw1 = 7.0;
		float v1 = (3.0 + t) / vw1;
		float3 param_152 = float3 (base_uv + (vec2 (u0, v1) * shadowMapSizeInv), shadowTC_1.z);
		float s0_1 = (uw0 * vw1) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_152);
		float3 param_153 = float3 (base_uv + (vec2 (u1, v1) * shadowMapSizeInv), shadowTC_1.z);
		float s1_1 = (uw1 * vw1) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_153);
		float3 param_154 = float3 (base_uv + (vec2 (u2, v1) * shadowMapSizeInv), shadowTC_1.z);
		float s2_1 = (uw2 * vw1) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_154);
		shadow += ((s0_1 + s1_1) + s2_1);
		float vw2 = 1.0 + (3.0 * t);
		float v2 = (t / vw2) + 2.0;
		float3 param_155 = float3 (base_uv + (vec2 (u0, v2) * shadowMapSizeInv), shadowTC_1.z);
		float s0_2 = (uw0 * vw2) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_155);
		float3 param_156 = float3 (base_uv + (vec2 (u1, v2) * shadowMapSizeInv), shadowTC_1.z);
		float s1_2 = (uw1 * vw2) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_156);
		float3 param_157 = float3 (base_uv + (vec2 (u2, v2) * shadowMapSizeInv), shadowTC_1.z);
		float s2_2 = (uw2 * vw2) * SAMPLE_TEXTURE2D_SHADOW (_LightsShadowmapTexture, sampler_LightsShadowmapTexture, param_157);
		shadow += ((s0_2 + s1_2) + s2_2);
		shadow /= 144.0;
#endif
	}
	else
	{
		shadow = 1;// light_parms & 3u == 2u ? 1 : 0;//是否
	}


	return shadow;
}

#endif