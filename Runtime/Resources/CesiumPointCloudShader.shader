Shader "Hidden/Cesium/PointCloudShading"
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

			#pragma target 5.0

			#pragma multi_compile __ HAS_POINT_COLORS
			#pragma multi_compile __ HAS_POINT_NORMALS
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

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

			// This is needed here to ensure the point data struct is the correct size.
			#pragma multi_compile __ HAS_POINT_COLORS
			#pragma multi_compile __ HAS_POINT_NORMALS

			// This sets up various keywords for different light types and shadow settings.
			#pragma multi_compile_shadowcaster

			#pragma vertex Vertex
			#pragma fragment Fragment

			#define SHADOW_CASTER_PASS

			#include "CesiumPointCloudShading.hlsl"

			ENDHLSL
		}
	}
}
