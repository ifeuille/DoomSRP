#ifndef _IFPIPELINE_LIGHTING_HLSL
#define _IFPIPELINE_LIGHTING_HLSL
#include "Common.hlsl"
#include "Cluster.hlsl"

struct LightData
{
	float3 pos;
	uint lightParms;
	float4 posShadow;
	float4 falloffR;
	float4 projS;
	float4 projT;
	float4 projQ;
	uint4 scaleBias;
	float4 boxMin;
	float4 boxMax;
	float4 areaPlane;
	uint lightParms2;
	uint colorPacked;
	float specMultiplier;
	float shadowBleedReduce;
};

//UNITY_SAMPLE_TEX2D _lightsatlasmap;
//UNITY_DECLARE_TEX2D(_lightsatlasmap);
sampler2D  _lightsatlasmap;
StructuredBuffer<LightData> _LightsDataList;


void SetupAmbientLighting(out float3 ambient_lighting, out uint diffuse_lighting_packed, lightingInput_t inputs)
{
	//todo sh indirectDiffuse
	half3 indirectDiffuse = half3(0, 0, 0);
	half3 ambient = max(indirectDiffuse, half3(0, 0, 0));
	ambient += inputs.emissive;
	ambient_lighting = ambient;
	float3 param = ambient;
	diffuse_lighting_packed = packRGBE(param);
}

LightData GetLightParam(int light_id)
{
	return _LightsDataList[light_id];
}

lightingInput_t PipeLighting (lightingInput_t inputs, uint lightsMin, uint lightsMax)
{
	uint lightIdx = lightsMin;
	const float clip_min_1 = 0.0039215688593685626983642578125;

	while (lightIdx < lightsMax)
	{
		//init
		//uint light_id = ItemGetLightID (lightIdx++);
		//inputs.debugColor = half3(lightIdx, 0, 0); break;
		LightData lightParms = GetLightParam(ItemGetLightID (lightIdx++));//MACRO_NAME(_LightsDataList)[light_id];
		//cull
		float3 projTC_1 = float3 (
			(inputs.position.x * lightParms.projS.x) + 
			((inputs.position.y * lightParms.projS.y) + 
			((inputs.position.z * lightParms.projS.z) + lightParms.projS.w)),
			(inputs.position.x * lightParms.projT.x) + 
			((inputs.position.y * lightParms.projT.y) + 
			((inputs.position.z * lightParms.projT.z) + lightParms.projT.w)),
			(inputs.position.x * lightParms.projQ.x) + 
			((inputs.position.y * lightParms.projQ.y) + 
			((inputs.position.z * lightParms.projQ.z) + lightParms.projQ.w)));
		//Needs project UV to screen space.
		projTC_1.xy = float2 (projTC_1.x/ projTC_1.z, projTC_1.y/ projTC_1.z);
		//Depth doesn't need to project to the screen space.
		projTC_1.z = 
			(inputs.position.x * lightParms.falloffR.x) +
			((inputs.position.y * lightParms.falloffR.y) + 
			((inputs.position.z * lightParms.falloffR.z) + lightParms.falloffR.w));
		//inputs.debugColor = lightParms.falloffR.xyz; break;
		//如果[0,1,3]为shadow的控制参数？ [4] shadow?,[22~31]shadowsparmsubo索引
		//[5] circle area light,[6] rect area light,[8] diffuse?
		//public bool lightParms_Shadow;//4  
		//public bool lightParms_Rect;//32
		//public bool lightParms_Circle;//16
		//public bool lightParms_SkipSkipModel;//是否允许忽略模型忽略 64
		//public bool lightParams_IsArea;//48=32+16 夹角接近超过90°是否还计算, 也就是是否是区域光,如果是则继续判断,因为区域光的lightPos要偏移
		//public bool lightParams_NoDiffuse;//256，将diffuse light color置为0
		projTC_1.z = 0.5;
		uint light_parms = lightParms.lightParms;
		uint light_flags = 0;
		//light_flags |= 64u;
		//inputs.debugColor = float3(0, 0, projTC_1.z); break;
		float min_tc = fmin3 (projTC_1.x, projTC_1.y, projTC_1.z);
		bool /*_3161*/isOutFrustumMin = min_tc <= clip_min_1;//out of the frustum
		/*if (projTC_1.z <= clip_min_1)
		{
			inputs.debugColor = half3(1, 0, 0); break;
		}
		inputs.debugColor = half3(0, 1, 0); break;*/
		bool /*_3178*/isOutFrustumMax;
		//如果最小的小于极小值，则_3161为true,_3178为true,_3186为true，跳过
		//如果最小的大于极小值,最大的大于极大值,则_3161为false,_3178为true,_3186为true,跳过
		//如果是跳过模型且灯允许跳过,则跳过
		//否则继续。这里在进行裁剪。。。方法是用视锥体原理。视锥体这个东西很有用。可能是正视投影，不知道形状怎么处理，也许只有立方体。
		//projective texture mapping技术
		if (!isOutFrustumMin)
		{
			isOutFrustumMax = fmax3 (projTC_1.x, projTC_1.y, projTC_1.z) >= (1.0 - clip_min_1);
		}
		else
		{
			//inputs.debugColor = half3(1, 0, 0); break;
			isOutFrustumMax = isOutFrustumMin;
		}
		bool /*_3186*/needCull;
		if (!isOutFrustumMax)
		{
			//inputs.debugColor = half3(1, 0, 0); break;
			needCull = false;//todo (light_parms & light_flags) != 0u;//无光照mesh，允许无光照
		}
		else
		{
			needCull = isOutFrustumMax;
		}
		if (needCull)
		{
			//inputs.debugColor = half3(1, 0, 0); break;
			continue;
		}
		//inputs.debugColor = half3(0, 1, 0); break;
		float3 light_position = lightParms.pos;
		float3 light_vector = light_position - inputs.position;
		//float param_140 = dot (inputs.normal, light_vector);
		float NdotL = saturate (dot (inputs.normal, light_vector));
		bool _3207 = NdotL <= clip_min_1;//夹角大概接近或超过90°就不计算,注意这个角度clip_min_1好像有点讲究
		//bool outOfFrustum = NdotL <= clip_min_1;
		//bool isRectAreaLight = Bit_And_Single(light_parms, 16);
		//bool isCircleAreaLight = Bit_And_Single(light_parms, 32);
		//bool isAreaLight = isRectAreaLight && isCircleAreaLight;
		//bool isShadow = Bit_And_Single(light_parms, 4);
		//if (outOfFrustum && !isAreaLight)
		//{
		//	continue;
		//}
		bool _3214;
		//如果夹角大于90°
		if (_3207)
		{
			//inputs.debugColor = half3(0, 0, 1); break;
			_3214 = (light_parms & 48u) == 0u;//如果不是区域光
		}
		else
		{
			//inputs.debugColor = half3(0, 1, 1); break;
			_3214 = _3207;
		}
		if (_3214)
		{
			//inputs.debugColor = half3(0, 1, 0); break;
			continue;
		}		
		//inputs.debugColor = half3(1, 0, 0); break;
		// 区域光还要做偏移
		float4 falloffScaleBias = unpackR15G15B15A15 (lightParms.scaleBias.xy);
		float4 projFilterScaleBias = unpackR15G15B15A15 (lightParms.scaleBias.zw);
		float4 lightaltasuv = float4 ((projTC_1.xy * projFilterScaleBias.xy) + projFilterScaleBias.zw, 0.0, 0.0);
		//这里使用UNITY_SAMPLE_TEX2D,回导致unroll失败得问题。。
		float projFilter = tex2Dlod (_lightsatlasmap, lightaltasuv).x;
		lightaltasuv = float4 ((float2 (projTC_1.z, 0.5) * falloffScaleBias.xy) + falloffScaleBias.zw, 0.0, 0.0);
		float falloff = tex2Dlod (_lightsatlasmap, lightaltasuv).x;
		inputs.debugColor = float3(lightParms.scaleBias.xy, 0); break;
		float light_attenuation = (falloff * falloff) * (projFilter * projFilter);
		// 太弱了
		if (light_attenuation <= (clip_min_1 / 256.0))
		{
			//inputs.debugColor = float4(1,1,0,1); break;
			continue;
		}
		//inputs.debugColor = float4(1,0, 0, 1); break;
		// shadow
		float shadow = 1.0;
		if ((light_parms & 4u) != 0u)
		{
			//TODO shadow
		}
		uint param_159 = lightParms.colorPacked;
		float3 light_color = (unpackRGBE(param_159) * shadow);
		//inputs.debugColor = light_color; break;
		light_color = light_color * light_attenuation;
		//inputs.debugColor = light_color; break;
		//inputs.debugColor = float3(light_attenuation, light_attenuation, light_attenuation); break;
		float light_spec_multiplier = lightParms.specMultiplier;

		// area light,todo
		if ((light_parms & 48u) != 0u)
		{
			//inputs.debugColor = float4(0, 1, 0, 1); break;
			//9133.465	6647.126	7134
			float4 areaLightPlane = lightParms.areaPlane;//0.00, 0.00, 0.00, 0.00
			float4 areaLightPlaneU = lightParms.boxMin;//8833.36523, 6385.43604, 6813.99902, 1.00
			float4 areaLightPlaneV = lightParms.boxMax;//9433.56445, 6908.81592, 7453.99902, 0.00
			float projToOrigin = dot (inputs.position, areaLightPlane.xyz) + areaLightPlane.w;
			if (projToOrigin < 0.0)//与平面法向量夹角小于90°就不算了？
			{
				//inputs.debugColor = float4(0, 1, 0, 1); break;
				continue;
			}
			//inputs.debugColor = float4(1, 0, 0, 1); break;
			float falloffDistance = length (areaLightPlane.xyz);
			float falloff_1 = saturate (projToOrigin);
			light_color *= (falloff_1 * falloff_1);//夹角带来的衰减
			projToOrigin /= falloffDistance;
			areaLightPlane /= float4 (falloffDistance, falloffDistance, falloffDistance, falloffDistance);//单位化
			float3 mirrorDir = reflect (-inputs.view, inputs.normal);
			if (dot (mirrorDir, areaLightPlane.xyz) > 0.0)
			{
				mirrorDir = reflect (mirrorDir, areaLightPlane.xyz);
			}
			float projToPlane = dot (mirrorDir, areaLightPlane.xyz);
			float projLength = projToOrigin / projToPlane;
			float3 hitPos = inputs.position - (mirrorDir * projLength);//区域上的点
			float3 hitDir = hitPos - light_position;
			float u = dot (hitDir, areaLightPlaneU.xyz);
			float v = dot (hitDir, areaLightPlaneV.xyz);
			//这里是为了减少分支语句带来的效率问题？原理是什么？指令类型？
			float lenCircle = length (float2 (u, v));
			float lenRect = max (abs (u), abs (v));
			float len;
			if ((light_parms & 48u) == 32u)
			{
				len = lenCircle;
			}
			else
			{
				len = lenRect;
			}
			//float len = _4233;
			//这里没有意义？
			if ((light_parms & 48u) == 48u)
			{
				//float2 param_161 = float2 (u, v);
				float2 _4252 = saturate (float2 (u, v));
				projTC_1 = float3 (_4252.x, _4252.y, projTC_1.z);
			}
			len = min (1.0, len) / len;
			u *= (len * areaLightPlaneU.w);
			v *= (len * areaLightPlaneV.w);
			hitDir = (areaLightPlaneU.xyz * u) + (areaLightPlaneV.xyz * v);
			light_position += hitDir;//world space
		}
		//inputs.debugColor = float4(1, 0, 0, 1); break;
		float3 light_color_final = light_color;
		float luma = GetLuma (light_color);
		//light_color_final = light_color_final;// mix (float3 (luma, luma, luma), light_color_final, freqLow_fragmentUniforms.pbrdebugparms.www);
		float3 light_vector_1 = normalize (light_position - inputs.position);
		//float param_163 = dot (inputs.normal, light_vector_1);
		float NdotL_1 = saturate (dot (inputs.normal, light_vector_1));
		//uint param_164 = inputs.specular_packed;
		//float3 param_165 = inputs.normal;
		//float3 param_166 = inputs.view;
	/*	float3 param_167 = light_vector_1;
		float3 param_168 = unpackR10G10B10 (param_164);*/
		//inputs.debugColor = param_168; break;
		//float param_169 = abs (inputs.smoothness);
		float3 spec = specBRDF (inputs.normal, inputs.view, light_vector_1, unpackR10G10B10 (inputs.specular_packed), abs (inputs.smoothness));
		//inputs.debugColor = spec; break;
		//uint param_170 = inputs.specular_lighting_packed;
		float3 param_171 = (((spec * light_spec_multiplier) * light_color_final) * NdotL_1) + unpackRGBE (inputs.specular_lighting_packed);
		inputs.specular_lighting_packed = packRGBE (param_171);
		float3 diffuse_light_color;
		if ((light_parms & 256u) > 0u)
		{
			//inputs.debugColor = float4(1, 0, 0, 1); break;
			diffuse_light_color = float3 (0.0, 0.0, 0.0);
		}
		else
		{
			diffuse_light_color = light_color_final;
		}
		//float3 diffuse_light_color = _4343;
		//uint param_172 = inputs.diffuse_lighting_packed;
		float3 param_173 = (diffuse_light_color * NdotL_1) + unpackRGBE (inputs.diffuse_lighting_packed);
		inputs.diffuse_lighting_packed = packRGBE (param_173);
	}
	//uint param_174 = inputs.diffuse_lighting_packed;
	//uint param_175 = inputs.albedo_packed;
	inputs.diffuse_lighting = unpackRGBE(inputs.diffuse_lighting_packed) * unpackR10G10B10(inputs.albedo_packed);
	//uint param_176 = inputs.specular_lighting_packed;
	inputs.specular_lighting = unpackRGBE(inputs.specular_lighting_packed);
	inputs.output_lighting = inputs.diffuse_lighting + inputs.specular_lighting;
	return inputs;
}


#endif