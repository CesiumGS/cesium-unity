Shader "Hidden/Cesium/PointCloudUnlit"
{
	Properties
	{
		_ColorGBuffer("Color G-Buffer", 2D) = "white" {}
		_DepthGBuffer("Depth G-Buffer", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		LOD 100 // Built-in LOD value for unlit shaders

		Pass
		{
			Name "Lighting Pass"
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

		Pass
		{

			Name "Shadow Pass"
			Tags { "LightMode" = "ShadowCaster" }

			HLSLPROGRAM

			// This sets up various keywords for different light types and shadow settings
			#pragma multi_compile_shadowcaster

			#pragma vertex Vertex
			#pragma fragment Fragment

			// TODO: find a way to calculate the aspect ratio relative to the screen in Unity,
			// not the _ScreenParams in the shader (which are dependent on the render texture)
			#define SHADOW_CASTER_PASS

			#include "PointCloudShading.hlsl"

			ENDHLSL
		}
	}
}
