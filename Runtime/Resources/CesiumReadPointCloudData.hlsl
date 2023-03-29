#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

//#define _HAS_POINT_NORMALS
#define _HAS_POINT_COLORS

struct Point
{
	float3 position;
#ifdef _HAS_POINT_NORMALS
	float3 normal;
#endif
#ifdef _HAS_POINT_COLORS
	uint packedColor;
#endif
};

StructuredBuffer<Point> _inPoints;

float4 _attenuationParameters;

void CesiumReadPointCloudData_float(float vertexIndex, out float3 position, out float3 normal, out float4 color)
{
	uint pointIndex = vertexIndex / 6;
	uint subVertex = vertexIndex - (pointIndex * 6); // Modulo

	Point inPoint = _inPoints[pointIndex];
	position = inPoint.position;

	// Using the vertex ID saves us from creating extra attribute buffers 
	// for the corners. We can hardcode the corners of the quad as follows. 
	// (Unity uses clockwise vertex winding.)
	// 1 ----- 2,4
	// |    /   |
	// |   /    |
	// |  /     |
	// 0,3 ---- 5
	float2 offset;

	if (subVertex == 0 || subVertex == 3)
	{
		offset = float2(-0.5, -0.5);
	}
	else if (subVertex == 1)
	{
		offset = float2(-0.5, 0.5);
	}
	else if (subVertex == 2 || subVertex == 4)
	{
		offset = float2(0.5, 0.5);
	}
	else
	{
		offset = float2(0.5, -0.5);
	}

	//position += float3(offset * 0.1, 0.0);

	float4 positionWC = mul(UNITY_MATRIX_M, float4(position, 1.0));
	float4 positionEC = mul(UNITY_MATRIX_V, positionWC);
	float4 positionClip = mul(UNITY_MATRIX_VP, positionWC);
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

	// Transform the clip space position back to object space.
	// It's unfortunate we need to do this, but shader graph exclusively outputs object space positions.
	//positionEC = mul(UNITY_MATRIX_I_P, positionClip);
	//position = mul(UNITY_MATRIX_T_MV, positionEC);
	positionWC = mul(UNITY_MATRIX_I_VP, positionClip);
	position = mul(UNITY_MATRIX_I_M, positionWC);

#ifdef _HAS_POINT_NORMALS
	normal = inPoint.normal;
#else
	normal = float3(0.0, 0.0, 1.0);
#endif

#ifdef _HAS_POINT_COLORS
	uint packedColor = inPoint.packedColor;
	uint r = packedColor & 255;
	uint g = (packedColor >> 8) & 255;
	uint b = (packedColor >> 16) & 255;
	uint a = packedColor >> 24;
	color = float4(r, g, b, a) / 255;
#else
	color = float4(1.0, 1.0, 1.0, 1.0);
#endif
}

#endif