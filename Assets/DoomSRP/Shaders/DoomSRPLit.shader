Shader "DoomSRP/DoomSRPLit"
{
	Properties
	{
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		_MainTex("Albedo", 2D) = "white" {}

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		_SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
		_SpecGlossMap("Specular", 2D) = "white" {}

		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_EmissionColor("Emission Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		
		//[Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0

		// Blending state
		//[HideInInspector] _Surface("__surface", Float) = 0.0
		//[HideInInspector] _Blend("__blend", Float) = 0.0
		//[HideInInspector] _AlphaClip("__clip", Float) = 0.0
		//[HideInInspector] _SrcBlend("__src", Float) = 1.0
		//[HideInInspector] _DstBlend("__dst", Float) = 0.0
		//[HideInInspector] _ZWrite("__zw", Float) = 1.0
		//[HideInInspector] _Cull("__cull", Float) = 2.0

		_ReceiveShadows("Receive Shadows", Float) = 1.0
	}
	HLSLINCLUDE
#define _TEXTURE_CLUSTER_ON

	ENDHLSL
	SubShader
	{
		Tags { "RenderType" = "Opaque"  "RenderPipeline" = "DoomSRP" "IgnoreProjector" = "True"}
		LOD 300
			 
		Pass
		{
			Name "DoomSRPLit"
			Tags{"LightMode"="ClusterForward"}
			//Name "FORWARD"
			//Tags { "LightMode" = "ForwardBase" }

			//Blend[_SrcBlend][_DstBlend]
			//ZWrite[_ZWrite]
			//Cull[_Cull]

			ZTest  Always
			ZWrite On
			Blend  Off
			ColorMask RGBA

			HLSLPROGRAM
			//#pragma prefer_hlslcc gles
			//#pragma exclude_renderers d3d11_9x
			#pragma target 5.0

			//#pragma enable_GL_ARB_explicit_attrib_location : require
			//#pragma enable_GL_ARB_shader_bit_encoding : enable
			//#pragma GL_ARB_gpu_shader5：enalbe
			#pragma enable_d3d11_debug_symbols
			//##pragma enalbe_GL_ARB_gpu_shader5
			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICSPECGLOSSMAP
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature _OCCLUSIONMAP

			#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature _SPECULAR_SETUP
			#pragma shader_feature _RECEIVE_SHADOWS_OFF
			
			// -------------------------------------
			// IFPipeline Keywords
			#pragma multi_compile _ _SHADOWS_ENABLED
			#pragma multi_compile _ _TEXTURE_CLUSTER_ON // below gles3.2

			// -------------------------------------
			// Unity defined keywords
			//#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			//#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fog

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			#include "UnityCG.cginc"
			#include "ShaderLibrary/Common.hlsl"
			#include "ShaderLibrary/Cluster.hlsl"
			#include "ShaderLibrary/Lighting.hlsl"
			#include "ShaderLibrary/CommonPosition.hlsl"

			#include "Lit/InputSurfaceLit.hlsl"
			#include "DoomSRPLit.hlsl"
			
			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "Lit/InputSurfaceLit.hlsl"
			#include "DepthOnlyPass.hlsl"
			ENDHLSL
		}
	}
	Fallback "Legacy Shaders/VertexLit"
	//CustomEditor "DoomSRP.DoomSRPLitShaderGUI"
}
