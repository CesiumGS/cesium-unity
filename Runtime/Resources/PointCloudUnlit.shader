Shader "Cesium/PointCloudUnlit"
{
	Properties
	{
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		LOD 100 // Built-in LOD value for unlit shaders

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward"}

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 5.0

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			#pragma vertex Vertex
			#pragma fragment Fragment

			#include "PointCloudShading.hlsl"

		ENDHLSL

		}

		// The shadow caster pass renders to a shadow map, so we can remove any
		// lighting / color logic here.
		Pass {

			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			HLSLPROGRAM
		// Signal this shader requires compute buffers
		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x
		#pragma target 5.0

		// This sets up various keywords for different light types and shadow settings
		#pragma multi_compile_shadowcaster

		#pragma vertex Vertex
		#pragma fragment Fragment

		#define SHADOW_CASTER_PASS

		#include "PointCloudShading.hlsl"

	ENDHLSL

	}
	}
}
