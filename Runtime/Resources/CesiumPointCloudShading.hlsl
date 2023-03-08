#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"

struct VertexInput
{
	float3 position;
#ifdef HAS_POINT_NORMALS
	float3 normal;
#endif
#ifdef HAS_POINT_COLORS
	uint packedColor;
#endif
};

StructuredBuffer<VertexInput> _inVertices;

float4x4 _worldTransform;
float4 _attenuationParameters;
float4 _constantColor;

struct VertexOutput
{
	float4 positionClip : SV_POSITION; // Position in clip space
#ifdef HAS_POINT_NORMALS
	float3 normalWC : TEXCOORD0; // Normal in world space. Using TEXCOORD0 because there's no 'NORMAL' semantic.
#endif
#ifdef HAS_POINT_COLORS
	uint packedColor : COLOR_0; // Packed vertex color
#endif
};

VertexOutput Vertex(uint vertexID : SV_VertexID) {
	VertexOutput output;
	uint pointIndex = vertexID / 6;
	uint vertexIndex = vertexID - (pointIndex * 6); // Modulo
	VertexInput input = _inVertices[pointIndex];
	float3 position = input.position;

	// Using the vertex ID saves us from creating extra attribute buffers 
	// for the corners. We can hardcode the corners of the quad as follows. 
	// (Unity uses clockwise vertex winding.)
	// 1 ----- 2,4
	// |    /   |
	// |   /    |
	// |  /     |
	// 0,3 ---- 5
	float2 offset;

	if (vertexIndex == 0 || vertexIndex == 3)
	{
		offset = float2(-0.5, -0.5);
	}
	else if (vertexIndex == 1)
	{
		offset = float2(-0.5, 0.5);
	}
	else if (vertexIndex == 2 || vertexIndex == 4)
	{
		offset = float2(0.5, 0.5);
	}
	else
	{
		offset = float2(0.5, -0.5);
	}

	float4 positionWC = mul(_worldTransform, float4(position, 1.0));
	float4 positionEC = mul(unity_MatrixV, positionWC);
	float4 positionClip = mul(unity_MatrixVP, positionWC);
	float maximumPointSize = _attenuationParameters.x;
	float geometricError = _attenuationParameters.y;
	float depthMultiplier = _attenuationParameters.z;
	float depth = -positionEC.z;

	float pointSize = min((geometricError / depth) * depthMultiplier, maximumPointSize);
	float2 pixelOffset = (pointSize * offset);
	// Platforms can have different conventions of clip space (y-top vs. y-bottom).
	// ProjectionParams.x will adjust the y-coordinate for clip space so that the output
	// is the same across different platforms.
	float2 screenOffset =
		pixelOffset / float2(_ScreenParams.x, _ScreenParams.y * _ProjectionParams.x);
	// The clip space position xy in Unity is in [-w, w] where w is the w-coordinate.
	// Perspective divide with w is done between the vertex and fragment shaders.
	positionClip.xy += screenOffset * positionClip.w;
	output.positionClip = positionClip;

#ifdef HAS_POINT_COLORS
	output.packedColor = input.packedColor;
#endif

#ifdef HAS_POINT_NORMALS
	float4 normalWC = mul(_worldTransform, float4(input.normal, 0));
	output.normalWC = normalWC;
#endif

	return output;
}

float4 Fragment(VertexOutput input) : SV_TARGET{
	// The shadow caster pass renders to a shadow map, so we can ignore
	// the color logic here.
	#ifdef SHADOW_CASTER_PASS
	return float4(1, 1, 1, 1);
	#else
	float4 color;

	#ifdef HAS_POINT_COLORS
	uint packedColor = input.packedColor;
	uint r = packedColor & 255;
	uint g = (packedColor >> 8) & 255;
	uint b = (packedColor >> 16) & 255;
	uint a = packedColor >> 24;
	color = float4(r, g, b, a) / 255;
	#else
	color = _constantColor;
	#endif

	#ifdef HAS_POINT_NORMALS
	float3 lightWC = _WorldSpaceLightPos0.xyz;
	float3 lightColor = _LightColor0.rgb;

	float3 normalWC = input.normalWC;
	float NdotL = clamp(dot(normalWC, lightWC), 0.01, 1);

	color.xyz *= lightColor * NdotL;
	#endif

	return color;
	#endif
}

#endif