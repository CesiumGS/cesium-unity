#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

#include "UnityCG.cginc"

StructuredBuffer<float3> _inPositions;
StructuredBuffer<uint> _inColors;
StructuredBuffer<float3> _inNormals;

float4 _constantColor;
float4x4 _worldTransform;

struct VertexOutput
{
	float4 positionClip : SV_POSITION; // Position in clip space
	float4 color : COLOR0; // Vertex colors
};

struct FragmentOutput
{
	float4 colorGBuffer : COLOR0;
	float4 depthGBuffer : COLOR1;
};

VertexOutput Vertex(uint vertexID : SV_VertexID) {
	VertexOutput output;

	uint pointIndex = vertexID / 6;
	uint vertexIndex = vertexID - (pointIndex * 6); // Modulo
	float3 position = _inPositions[pointIndex];
	float pointSize = 20.0;
	float2 offset;

	float4 positionWC = mul(_worldTransform, float4(position, 1.0));
	float4 positionClip = mul(unity_MatrixVP, positionWC);

	// Using the vertex ID  saves us from creating extra attribute buffers 
	// for the corners. We can hardcode the corners of the quad as follows. 
	// (Unity uses clockwise vertex winding.)
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
	
	uint packedColor = _inColors[pointIndex];
	uint r = packedColor & 255;
	uint g = (packedColor >> 8) & 255;
	uint b = (packedColor >> 16) & 255;
	uint a = packedColor >> 24;

	output.positionClip = positionClip;
	output.color = float4(r, g, b, a) / 255;

	return output;
}

float4 Fragment(VertexOutput input) : SV_TARGET{
	return input.color;
}

#endif