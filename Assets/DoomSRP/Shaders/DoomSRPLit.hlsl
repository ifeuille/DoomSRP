#ifndef __IFPIPELINE_LIT_HLSL__
#define __IFPIPELINE_LIT_HLSL__

struct IFVertexInput
{
	float4 vertex   : POSITION;
	half3 normal    : NORMAL;
	float2 uv0      : TEXCOORD0;
	//float2 uv1      : TEXCOORD1;
//#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
//	float2 uv2      : TEXCOORD2;
//#endif
//#ifdef _TANGENT_TO_WORLD
	half4 tangent   : TANGENT;
//#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct IFVertex2Fragment
{
	float4 positionSS : SV_POSITION;
	float4 uv : TEXCOORD0;
	float3 screenUV:TEXCOORD1;
	// [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
	half4 tangentToWorldAndPackedData[3] : TEXCOORD2;
	half3 eyeVec : TEXCOORD5;
	UNITY_FOG_COORDS (6)
};

half3x3 CreateTangentToWorldPerVertex(half3 normal, half3 tangent, half tangentSign)
{
	// For odd-negative scale transforms we need to flip the sign
	half sign = tangentSign * unity_WorldTransformParams.w;
	half3 binormal = cross(normal, tangent) * sign;
	return half3x3(tangent, binormal, normal);
}

half3 PerPixelWorldNormal(float3 normalTangent, half4 tangentToWorld[3])
{
#ifdef _NORMALMAP
	half3 tangent = tangentToWorld[0].xyz;
	half3 binormal = tangentToWorld[1].xyz;
	half3 normal = tangentToWorld[2].xyz;

#if UNITY_TANGENT_ORTHONORMALIZE
	normal = NormalizePerPixelNormal(normal);

	// ortho-normalize Tangent
	tangent = normalize(tangent - normal * dot(tangent, normal));

	// recalculate Binormal
	half3 newB = cross(normal, tangent);
	binormal = newB * sign(dot(newB, binormal));
#endif

	half3 normalWorld = normalize(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
#else
	half3 normalWorld = normalize(tangentToWorld[2].xyz);
#endif
	return normalWorld;
}

IFVertex2Fragment LitPassVertex(IFVertexInput v)
{
	IFVertex2Fragment o;
	ZERO_INITIALIZE(IFVertex2Fragment, o);
	o.positionSS = UnityObjectToClipPos(v.vertex);
	o.uv = TexCoords (v.uv0);
	float4 posWS = mul(unity_ObjectToWorld, v.vertex);
	float4 posVPS = mul(UNITY_MATRIX_VP, posWS);
	o.positionSS = posVPS;
	o.screenUV = ComputeScreenPos(o.positionSS).xyw;
	half3 normalWorld = UnityObjectToWorldNormal (v.normal);
	float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
	o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
	o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
	o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
	o.tangentToWorldAndPackedData[0].w = posWS.x;
	o.tangentToWorldAndPackedData[1].w = posWS.y;
	o.tangentToWorldAndPackedData[2].w = posWS.z;
	o.eyeVec = normalize(posWS.xyz - _WorldSpaceCameraPos);
	UNITY_TRANSFER_FOG(o, o.positionSS);
	return o;
}


float4 LitPassFragment(IFVertex2Fragment i) : SV_Target
{
	lightingInput_t inputs;
	//ZERO_INITIALIZE(lightingInput_t, inputs);
	InitilizeLightingInput(inputs);
	float3 posWS = float3(
	i.tangentToWorldAndPackedData[0].w,
	i.tangentToWorldAndPackedData[1].w,
	i.tangentToWorldAndPackedData[2].w
	);
	PositionInputs posInput = GetPositionInput(i.positionSS.xy, _IFScreenSize, i.positionSS.z, i.positionSS.w, posWS.xyz);
	SurfaceData surfaceData;
	InitializeStandardLitSurfaceData(i.uv.xy, surfaceData);
	//fill the lighting input
	{
		inputs.texCoords = i.uv;
		inputs.fragCoord = i.positionSS;
		inputs.position = posWS;
		//input.pos = 
		inputs.view = i.eyeVec;
		//used by decals..
		inputs.vertexNormal = half3(
			i.tangentToWorldAndPackedData[0].z,
			i.tangentToWorldAndPackedData[1].z,
			i.tangentToWorldAndPackedData[2].z);
		/*inputs.invTS = float3x3(
			i.tangentToWorldAndPackedData[0].x, i.tangentToWorldAndPackedData[0].y, i.tangentToWorldAndPackedData[0].z,
			i.tangentToWorldAndPackedData[1].x, i.tangentToWorldAndPackedData[1].y, i.tangentToWorldAndPackedData[1].z,
			i.tangentToWorldAndPackedData[2].x, i.tangentToWorldAndPackedData[2].y, i.tangentToWorldAndPackedData[2].z
			);*/
		inputs.normalTS = surfaceData.normalTS;
		inputs.normal = PerPixelWorldNormal(inputs.normalTS, i.tangentToWorldAndPackedData);
	}

	{
		inputs.emissive = surfaceData.emission;
		inputs.albedo = surfaceData.albedo;
		inputs.specular = surfaceData.specular;
		inputs.smoothness = surfaceData.smoothness;
		half oneMinusReflectivity;
		inputs.albedo = DiffuseAndSpecularFromMetallic (inputs.albedo, surfaceData.metallic, /*out*/ inputs.specular, /*out*/ oneMinusReflectivity);

		//return float4(inputs.specular, 1);
		float3 param_68 = inputs.albedo;
		uint _1803 = packR10G10B10 (param_68);
		inputs.albedo_packed = _1803;
		float3 param_69 = inputs.specular;
		uint _1809 = packR10G10B10 (param_69);
		inputs.specular_packed = _1809;
		//return float4(unpackR10G10B10 (inputs.specular_packed),1);
	}
	
	SetupAmbientLighting(inputs.ambient_lighting, inputs.diffuse_lighting_packed, inputs);
	/*
	d3d _IFScreenSize.y - i.positionSS.y > 0
	d3d _IFScreenSize.y - i.positionSS.y > 0
	*/
	float2 screenUV = i.screenUV.xy / i.screenUV.z;

	//return float4(screenUV.xy, 0, 1);
	uint clusterOffset = GetLightClusterIndex(screenUV, posInput.linearDepth);
	//uint clusterOffset = GetLightClusterIndex(i.screenUV, posInput.linearDepth);
	//uint clusterOffset = GetLightClusterIndex(i.positionSS.xy, posInput.linearDepth);
	ClusterData clusterData = GetClusterData (clusterOffset);
	//if (clusterData.lightsMax - clusterData.lightsMin > 1)
	//{
	//	return float4(1, 0, 0, 1);
	//}
	//return float4(0, 1, 0, 1);
	inputs = PipeLighting(inputs, clusterData.lightsMin, clusterData.lightsMax);
	half4 col;
	col.rgb = inputs.output_lighting;
	col.a = 1;// inputs.alpha;
	// Other output :normal .... 
	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, col);
	col.rgb = inputs.debugColor;
	//col.rgb = inputs.diffuse_lighting;// +inputs.specular_lighting;// color
	return col;
}

#endif