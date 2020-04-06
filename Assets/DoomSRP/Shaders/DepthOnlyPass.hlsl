﻿#ifndef DOOMSRP_DEPTH_ONLY_PASS_INCLUDED
#define DOOMSRP_DEPTH_ONLY_PASS_INCLUDED

#include "doomsrp/Core.hlsl"

struct Attributes
{
	float4 position     : POSITION;
	float2 texcoord     : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float2 uv           : TEXCOORD0;
	float4 positionCS   : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

Varyings DepthOnlyVertex (Attributes input)
{
	Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID (input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO (output);

	output.uv = TRANSFORM_TEX (input.texcoord, _MainTex);
	output.positionCS = TransformObjectToHClip (input.position.xyz);
	return output;
}

half4 DepthOnlyFragment (Varyings input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX (input);

	//Alpha (SampleAlbedoAlpha (input.uv, TEXTURE2D_ARGS (_MainTex, sampler_BaseMap)).a, _BaseColor, _Cutoff);
	Alpha(SampleAlbedoAlpha(input.uv).a, _Color, _Cutoff);
	return 0;
}
#endif
