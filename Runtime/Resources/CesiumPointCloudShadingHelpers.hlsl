#ifndef CESIUM_POINT_CLOUD_SHADING_HELPERS
#define CESIUM_POINT_CLOUD_SHADING_HELPERS

struct VertexInput
{
	float3 position;
	uint packedColor;
#ifdef HAS_POINT_NORMALS
	float3 normal;
#endif
#ifdef HAS_POINT_COLORS
#endif
};

StructuredBuffer<VertexInput> _inVertices;

float4x4 _worldTransform;
float4x4 _invWorldTransform;
float4 _attenuationParameters;

#ifndef HAS_POINT_COLORS
float4 _constantColor;
#endif

void GetVertexPositionAndCornerOffset_float(float VertexID, out float3 PositionWorld, out float2 Offset) {
	uint vertexID = (uint)VertexID;
	uint pointIndex = vertexID / 6;
	uint vertexIndex = vertexID - (pointIndex * 6); // Modulo
	VertexInput input = _inVertices[pointIndex];
	float3 position = input.position;
	PositionWorld = mul(_worldTransform, float4(position, 1.0)).xyz;

	// Using the vertex ID saves us from creating extra attribute buffers 
	// for the corners. We can hardcode the corners of the quad as follows. 
	// (Unity uses clockwise vertex winding.)
	// 1 ----- 2,4
	// |    /   |
	// |   /    |
	// |  /     |
	// 0,3 ---- 5
	if (vertexIndex == 0 || vertexIndex == 3)
	{
		Offset = float2(-0.5, -0.5);
	}
	else if (vertexIndex == 1)
	{
		Offset = float2(-0.5, 0.5);
	}
	else if (vertexIndex == 2 || vertexIndex == 4)
	{
		Offset = float2(0.5, 0.5);
	}
	else
	{
		Offset = float2(0.5, -0.5);
	}
}

void OffsetPositionInClipSpace_float(float4 InPositionClip, float2 Offset, float Depth, out float4 OutPositionClip) {
	float maximumPointSize = _attenuationParameters.x;
	float geometricError = _attenuationParameters.y;
	float depthMultiplier = _attenuationParameters.z;

	float pointSize = 10;// min((geometricError / Depth) * depthMultiplier, maximumPointSize);
	float2 pixelOffset = (pointSize * Offset);

	// Platforms can have different conventions of clip space (y-top vs. y-bottom).
	// ProjectionParams.x will adjust the y-coordinate for clip space so that the output
	// is the same across different platforms.
	float2 screenOffset =
		pixelOffset / float2(_ScreenParams.x, _ScreenParams.y * _ProjectionParams.x);

	// The clip space position xy in Unity is in [-w, w] where w is the w-coordinate.
	// Perspective divide with w is done between the vertex and fragment shaders.
	OutPositionClip = InPositionClip;
	OutPositionClip.xy += screenOffset * InPositionClip.w;
}

void MultiplyByInverseWorldTransform_float(float4 PositionWorld, out float4 PositionObject) {
	PositionObject = mul(_invWorldTransform, PositionWorld);
}

#endif