/*
https://zhuanlan.zhihu.com/p/71932575
错误总结：
1.light需要变换到view空间,在进行碰撞测试

*/
Shader "DoomSRP/Shader_DebugCluster"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_ColorLighting ("ColorLighting", Color) = (1,0,0,1)
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Pass
		{
			CGPROGRAM

			#pragma vertex main_VS
			#pragma fragment main_PS
			#pragma geometry main_GS
			#pragma target 5.0
			#pragma enable_d3d11_debug_symbols
			float4 _Color;
			float4 _ColorLighting;
			#include "UnityCG.cginc"
			#include "ShaderLibrary/Cluster.hlsl"

			struct VertexShaderOutput
			{
				float4 Min          : AABB_MIN;  // Min vertex position in view space.
				float4 Max          : AABB_MAX;  // Max vertex position in view space.
				float4 Color        : COLOR;     // Cluster color.
			};

			struct GeometryShaderOutput
			{
				float4 Color        : COLOR;
				float4 Position     : SV_POSITION;          // Clip space position.
			};
			struct AABB
			{
				float4 Min;
				float4 Max;
			};

			StructuredBuffer<AABB> ClusterAABBs;// : register(t1);
			float4x4 _CameraWorldMatrix;
			//StructuredBuffer<uint2> PointLightGrid_Cluster;

			bool CMin (float3 a, float3 b)
			{
				if (a.x < b.x && a.y < b.y && a.z < b.z)
					return true;
				else
					return false;
			}

			bool CMax (float3 a, float3 b)
			{
				if (a.x > b.x && a.y > b.y && a.z > b.z)
				{
					return true;
				}
				else
				{
					return false;
				}
			}

			float4 WorldToProject (float4 posWorld)
			{
				float4 l_posWorld = mul (_CameraWorldMatrix, posWorld);
				float4 posVP0 = mul(UNITY_MATRIX_VP, l_posWorld);// UnityObjectToClipPos (l_posWorld);
				return posVP0;
			}

			VertexShaderOutput main_VS (uint VertexID : SV_VertexID)
			{
				uint clusterID = VertexID; ;// UniqueClusters[VertexID];// VertexID;

				VertexShaderOutput vsOutput = (VertexShaderOutput)0;

				AABB aabb = ClusterAABBs[clusterID];// ClusterAABBs[VertexID];

				vsOutput.Min = aabb.Min;
				vsOutput.Max = aabb.Max;

				float4 factor = aabb.Max - aabb.Min;
				//factor *= 0.2;
				vsOutput.Max = aabb.Min + factor;
				vsOutput.Color = _Color;

				ClusterData clusterData = GetClusterData (clusterID);
				//CLUSTER_GET_CLUSTERDATA (clusterID);
				//input = PipeLighting (input, lightsMin, lightsMax);
				//float fClusterLightCount = PointLightGrid_Cluster[clusterID].y;
				if (clusterData.lightsMax - clusterData.lightsMin > 0)
				{
					vsOutput.Color = _ColorLighting;
				}
				return vsOutput;
			}


			// Geometry shader to convert AABB to cube.
			[maxvertexcount (16)]
			void main_GS (point VertexShaderOutput IN[1], inout TriangleStream<GeometryShaderOutput> OutputStream)
			{
				float4 min = IN[0].Min;
				float4 max = IN[0].Max;

				// Clip space position
				GeometryShaderOutput OUT = (GeometryShaderOutput)0;

		/*		float scale = abs(max.z - min.z)*0.2;
				min.xyz += scale;
				max.xyz -= scale;*/
				// AABB vertices
				const float4 Pos[8] = {
					float4(min.x, min.y, min.z, 1.0f),    // 0
					float4(min.x, min.y, max.z, 1.0f),    // 1
					float4(min.x, max.y, min.z, 1.0f),    // 2

					float4(min.x, max.y, max.z, 1.0f),    // 3
					float4(max.x, min.y, min.z, 1.0f),    // 4
					float4(max.x, min.y, max.z, 1.0f),    // 5
					float4(max.x, max.y, min.z, 1.0f),    // 6
					float4(max.x, max.y, max.z, 1.0f)     // 7
				};

				// Colors (to test correctness of AABB vertices)
				const float4 Col[8] = {
					float4(0.0f, 0.0f, 0.0f, 1.0f),       // Black
					float4(0.0f, 0.0f, 1.0f, 1.0f),       // Blue
					float4(0.0f, 1.0f, 0.0f, 1.0f),       // Green
					float4(0.0f, 1.0f, 1.0f, 1.0f),       // Cyan
					float4(1.0f, 0.0f, 0.0f, 1.0f),       // Red
					float4(1.0f, 0.0f, 1.0f, 1.0f),       // Magenta
					float4(1.0f, 1.0f, 0.0f, 1.0f),       // Yellow
					float4(1.01, 1.0f, 1.0f, 1.0f)        // White
				};

				const uint Index[18] = {
					0, 1, 2,
					3, 6, 7,
					4, 5, -1,
					2, 6, 0,
					4, 1, 5,
					3, 7, -1
				};

				[unroll]
				for (uint i = 0; i < 18; ++i)
				{
					if (Index[i] == (uint) - 1)
					{
						OutputStream.RestartStrip ();
					}
					else
					{
						OUT.Position = WorldToProject (Pos[Index[i]]);
						OUT.Color = IN[0].Color;
						OutputStream.Append (OUT);
					}
				}
			}

			float4 main_PS (GeometryShaderOutput IN) : SV_Target
			{
				//return _Color;
				return IN.Color;
			}

			ENDCG
		}
	}
}