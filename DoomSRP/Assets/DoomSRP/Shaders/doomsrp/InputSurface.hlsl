#ifndef __IFINPUTSURFACELIT_HLSL__
#define __IFINPUTSURFACELIT_HLSL__
#include "Common.hlsl"
#include "Packing.hlsl"
CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
half4 _Color;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Glossiness;
half _GlossMapScale;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;
CBUFFER_END
TEXTURE2D(_MainTex);
TEXTURE2D(_BumpMap);
TEXTURE2D(_EmissionMap);
TEXTURE2D(_OcclusionMap);
TEXTURE2D(_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);

SAMPLER(sampler_MainTex);
SAMPLER(sampler_BumpMap);
SAMPLER(sampler_OcclusionMap);
SAMPLER(sampler_MetallicGlossMap);
SAMPLER(sampler_SpecGlossMap);

//TEXTURE2D_ARGS(_MainTex, _MainTexSamp);
//TEXTURE2D_ARGS(_BumpMap, _BumpMapSamp);
//TEXTURE2D_ARGS(_EmissionMap, _EmissionMapSamp);
//TEXTURE2D_ARGS(_OcclusionMap, _OcclusionMapSamp);
//TEXTURE2D_ARGS(_MetallicGlossMap, _MetallicGlossMapSamp);
//TEXTURE2D_ARGS(_SpecGlossMap, _SpecGlossMapSamp);



struct SurfaceData
{
	half3 albedo;
	half3 specular;
	half  metallic;
	half  smoothness;
	half3 normalTS;
	half3 emission;
	half  occlusion;
	half  alpha;
};

#ifdef _SPECULAR_SETUP
//#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap,sampler_SpecGlossMap, uv)
#else
//#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap,sampler_MetallicGlossMap, uv)
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
	half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
	specGloss = SAMPLE_METALLICSPECULAR(uv);
#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
	specGloss.a = albedoAlpha * _GlossMapScale;
#else
	specGloss.a *= _GlossMapScale;
#endif
#else // _METALLICSPECGLOSSMAP
#if _SPECULAR_SETUP
	specGloss.rgb = _SpecColor.rgb;
#else
	specGloss.rgb = _Metallic.rrr;
#endif

#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
	specGloss.a = albedoAlpha * _GlossMapScale;
#else
	specGloss.a = _Glossiness;
#endif
#endif

	return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
	// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
#if defined(SHADER_API_GLES)
	return SAMPLE_TEXTURE2D(_OcclusionMap,sampler_OcclusionMap, uv).g;
#else
	half occ = UNITY_SAMPLE_TEX2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
	return LerpWhiteTo(occ, _OcclusionStrength);
#endif
#else
	return 1.0;
#endif
}


half3 UnpackNormalScale(half4 packedNormal, half bumpScale)
{
#if defined(UNITY_NO_DXT5nm)
	return UnpackNormalRGB(packedNormal, bumpScale);
#else
	return UnpackNormalmapRGorAG(packedNormal, bumpScale);
#endif
}

half3 SampleNormal(float2 uv, half scale = 1.0h)
{
#if _NORMALMAP
	half4 n = SAMPLE_TEXTURE2D(_BumpMap, uv);
#if BUMP_SCALE_NOT_SUPPORTED
	return UnpackNormal(n);
#else
	return UnpackNormalScale(n, scale);
#endif
#else
	return half3(0.0h, 0.0h, 1.0h);
#endif
}

half Alpha(half albedoAlpha, half4 color, half cutoff)
{
#if !defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A) && !defined(_GLOSSINESS_FROM_BASE_ALPHA)
	half alpha = albedoAlpha * color.a;
#else
	half alpha = color.a;
#endif

#if defined(_ALPHATEST_ON)
	clip(alpha - cutoff);
#endif

	return alpha;
}

//half4 SampleAlbedoAlpha (float2 uv, TEXTURE2D_ARGS (albedoAlphaMap, sampler_albedoAlphaMap))
//{
//	return SAMPLE_TEXTURE2D (albedoAlphaMap, sampler_albedoAlphaMap, uv);
//}

half4 SampleAlbedoAlpha(float2 uv)
{
	return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
}

half3 SampleEmission(float2 uv, half3 emissionColor)
{
#ifndef _EMISSION
	return 0;
#else
	return SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * emissionColor;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
	half4 albedoAlpha = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, uv);
	outSurfaceData.alpha = Alpha(albedoAlpha.a, _Color, _Cutoff);

	half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
	outSurfaceData.albedo = albedoAlpha.rgb * _Color.rgb;

#if _SPECULAR_SETUP
	outSurfaceData.metallic = 1.0h;
	outSurfaceData.specular = specGloss.rgb;
#else
	outSurfaceData.metallic = specGloss.r;
	outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
#endif

	outSurfaceData.smoothness = specGloss.a;
	outSurfaceData.normalTS = SampleNormal(uv, _BumpScale);
	outSurfaceData.occlusion = SampleOcclusion(uv);
	outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb);
}

float4 TexCoords (float2 uv0)
{
	float4 texcoord;
	texcoord.xy = TRANSFORM_TEX (uv0, _MainTex); // Always source from uv0
	//texcoord.zw = TRANSFORM_TEX (((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
	return texcoord;
}
inline half OneMinusReflectivityFromMetallic (half metallic)
{
	// We'll need oneMinusReflectivity, so
	//   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
	// store (1-dielectricSpec) in unity_ColorSpaceDielectricSpec.a, then
	//   1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) =
	//                  = alpha - metallic * alpha
	half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
	return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}
inline half3 DiffuseAndSpecularFromMetallic (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
	specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	oneMinusReflectivity = OneMinusReflectivityFromMetallic (metallic);
	return albedo * oneMinusReflectivity;
}


#endif