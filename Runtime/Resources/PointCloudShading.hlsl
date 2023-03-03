#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

#include "UnityCG.cginc"

struct VertexInput
{
	float3 position;
#ifdef HAS_POINT_COLORS
	uint packedColor;
#endif
#ifdef HAS_POINT_NORMALS
	float3 normal;
#endif
};

StructuredBuffer<VertexInput> _inVertices;

float4x4 _worldTransform;
float4 _attenuationParameters;

#ifndef HAS_POINT_COLORS
float4 _constantColor;
#endif

struct VertexOutput
{
	float4 positionClip : SV_POSITION; // Position in clip space
#ifdef HAS_POINT_COLORS
	uint packedColor : COLOR_0; // Packed vertex colors
#endif
#ifdef HAS_POINT_NORMALS
	float3 normal : TEXCOORD0; // Normals in view space. 
							   // There's no 'NORMAL' semantic in fragment shader.
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
	// 1 ----- 2
	// |    /  |
	// |   /   |
	// 0 / --- 3
	float2 offset;

	if (vertexIndex == 0)
	{
		offset = float2(-0.5, -0.5);
	}
	else if (vertexIndex == 1)
	{
		offset = float2(-0.5, 0.5);
	}
	else if (vertexIndex == 2)
	{
		offset = float2(0.5, 0.5);
	}
	else
	{
		offset = float2(0.5, -0.5);
	}

	float4 positionWC = mul(_worldTransform, float4(position, 1.0));
	float4 positionClip = mul(unity_MatrixVP, positionWC);
	float4 positionEC = mul(unity_MatrixV, positionWC);
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
	float4 normalWC = mul(_worldTransform, vec4(input.normal, 0));
	output.normal = mul(unity_MatrixV, normalWC).xyz;
#endif

	return output;
}

float4 Fragment(VertexOutput input) : SV_TARGET{
	// The shadow caster pass renders to a shadow map, so we can ignore
	// the color logic here.
	#ifdef SHADOW_CASTER_PASS
	return float4(1, 1, 1, 1);
	#else
	float4 result;
	#ifdef HAS_POINT_COLORS
	uint packedColor = input.packedColor;
	uint r = packedColor & 255;
	uint g = (packedColor >> 8) & 255;
	uint b = (packedColor >> 16) & 255;
	uint a = packedColor >> 24;
	result = float4(r, g, b, a) / 255;
	#else
	result = _constantColor;
	#endif

	#ifdef HAS_POINT_NORMALS
	// TODO: shade with normals
	#endif

	return result;
	#endif
}

#endif