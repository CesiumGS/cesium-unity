#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

#include "UnityCG.cginc"

struct Point
{
	float3 position;
	float3 normal;
	uint4 color;
};

StructuredBuffer<float3> _inPositions;

float4x4 _worldTransform;

struct VertexOutput
{
	float4 positionClip : SV_POSITION; // Position in clip space
	fixed4 color : COLOR0; // Vertex colors
};

VertexOutput Vertex(uint vertexID : SV_VertexID) {
	VertexOutput output;

	uint geometryIndex = vertexID / 6;
	uint vertexIndex = vertexID - (geometryIndex * 6); // Modulo
	float3 position = _inPositions[geometryIndex];
	float pointSize = 50.0;
	float2 offset;

	float4 positionWC = mul(_worldTransform, float4(position, 1.0));
	float4 positionClip = mul(unity_MatrixVP, positionWC);

	// We can hardcode the corners of the quad as follows. Using the vertex ID 
	// saves us from creating extra attribute buffers for the corners.
	// Unity uses clockwise vertex winding.
	// 1 ---- 2/4
	// |    /  |
	// |   /   |
	// 0/3 --- 5
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

	float2 pixelOffset = (pointSize * offset);
	// Platforms can have different conventions of clip space (y-top vs. y-bottom).
	// ProjectionParams.x will adjust the y-coordinate for clip space so that the output
	// is the same across different platforms.
	float2 screenOffset = 
		pixelOffset / float2(_ScreenParams.x, _ScreenParams.y * _ProjectionParams.x);
	// The clip space position xy in Unity is in [-w, w]
	// (perspective divide with the w-coordinate is done between the shaders)
	positionClip.xy += screenOffset * positionClip.w;

	output.positionClip = positionClip;
	return output;
}

float4 Fragment(VertexOutput input) : SV_TARGET {
	return float4(1, 0, 0, 1);
}

#endif