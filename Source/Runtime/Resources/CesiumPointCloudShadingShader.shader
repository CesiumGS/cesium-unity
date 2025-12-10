Shader "Hidden/Cesium/CesiumPointCloudShading"
{
	Properties
	{
	}

	SubShader
	{
		Tags { 
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		Blend [_SrcBlend] [_DstBlend]

		Pass
		{
			Name "Lighting Pass"
			Tags { "LightMode" = "UniversalForward"}

			HLSLPROGRAM

			#pragma target 4.5

			#pragma multi_compile __ HAS_POINT_COLORS
			#pragma multi_compile __ HAS_POINT_NORMALS
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			#pragma multi_compile_instancing // Needed for single-pass instanced VR.

			#pragma vertex Vertex
			#pragma fragment Fragment

			#include "CesiumPointCloudShading.hlsl"

			ENDHLSL
		}

		Pass
		{

			Name "Shadow Pass"
			Tags { "LightMode" = "ShadowCaster" }

			HLSLPROGRAM

			#pragma target 4.5

			#pragma multi_compile __ HAS_POINT_COLORS
			#pragma multi_compile __ HAS_POINT_NORMALS

			// This sets up various keywords for different light types and shadow settings.
			#pragma multi_compile_shadowcaster

			#pragma multi_compile_instancing // Needed for single-pass instanced VR.

			#pragma vertex Vertex
			#pragma fragment Fragment

			#define SHADOW_CASTER_PASS

			#include "CesiumPointCloudShading.hlsl"

			ENDHLSL
		}
	}
}
