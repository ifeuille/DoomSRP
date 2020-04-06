Shader "DoomSRP/TestCluster"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		//[HideInInspector] _IFScreenSize("ScreenSize",Vector) = (0,0,0,0)
		//[HideInInspector] NUM_CLUSTERS_X ("NUM_CLUSTERS_X",Int) = 0
		//[HideInInspector] NUM_CLUSTERS_Y ("NUM_CLUSTERS_Y",Int) = 0
		//[HideInInspector] NUM_CLUSTERS_Z ("NUM_CLUSTERS_Z",Int) = 0
		//[HideInInspector] MAX_ITEMS_PERCUSTER ("MAX_ITEMS_PERCUSTER",Int) = 0
		//[HideInInspector] _ClusterLighting ("_ClusterLighting", Vector) = (0, 0, 0, 0)
		//[HideInInspector] _ClusterCB_Size ("_ClusterCB_Size", Vector) = (0, 0, 0, 0)
    }
	HLSLINCLUDE

	//#include "UnityCG.cginc"
	//#include "doomsrp/Common.hlsl"
	#include "doomsrp/Core.hlsl"
	#include "doomsrp/Cluster.hlsl"
	#include "doomsrp/Lighting.hlsl"

	ENDHLSL
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Name "Test Cluster"
			Cull   Off
			ZTest  Always
			ZWrite On
			Blend  Off
			ColorMask RGBA

			HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
			#pragma enable_d3d11_debug_symbols
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				//SV_POSITION,xy screen pixel position,z:device depth,w view linear depth
                float4 positionSS : SV_POSITION;
				float4 screenPos : TEXCOORD2;
				float3 positionRWS:TEXCOORD3; // Relative camera space position
				float3 worldPos:TEXCOORD4;
            };


            v2f vert (appdata v)
            {
                v2f o;
				o.worldPos = mul (UNITY_MATRIX_M, v.vertex).xyz;
				o.screenPos = mul (UNITY_MATRIX_MV, v.vertex);
				o.screenPos.z = -UnityObjectToViewPos(v.vertex).z;
                o.positionSS = mul(UNITY_MATRIX_VP, float4(o.worldPos,1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.positionRWS = mul(UNITY_MATRIX_M, v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
				lightingInput_t input;
				input.albedo = float3(1, 1, 1);
				float2 tmp = _IFScreenSize.xy;
				PositionInputs posInput = GetPositionInput(i.positionSS.xy, GetScreenSize(), i.positionSS.z, i.positionSS.w, i.positionRWS.xyz);
				uint clusterOffset = GetLightClusterIndex(i.positionSS.xy, posInput.linearDepth);

				ClusterData clusterData = GetClusterData (clusterOffset);
				//CLUSTER_GET_CLUSTERDATA(clusterOffset);
				if (clusterData.lightsMax - clusterData.lightsMin > 0)
				{
					return float4(1, 0, 0, 1);
				}
				return float4(0, 1, 0, 1);
				input = PipeLighting (input, clusterData.lightsMin, clusterData.lightsMax);
				col.rgb = input.albedo;

                return col;
            }
			ENDHLSL
        }
    }
	Fallback Off
}
