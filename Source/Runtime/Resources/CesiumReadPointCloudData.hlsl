#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

#ifndef UNITY_UV_STARTS_AT_TOP
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH)
#define UNITY_UV_STARTS_AT_TOP 1
#else
#define UNITY_UV_STARTS_AT_TOP 0
#endif
#endif

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

float2 _attenuationParameters;

float computeDepthMultiplier()
{
#if UNITY_UV_STARTS_AT_TOP
    float t = -1.0f * UNITY_MATRIX_P._m11;
#else
    float t = UNITY_MATRIX_P._m11;
#endif
    float fovRadians = atan(1.0F / t) * 2.0F;
    float sseDenominator = 2.0f * tan(0.5f * fovRadians);
    return _ScreenParams.y / sseDenominator;
}

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

    switch (subVertex)
    {
        case 0:
        case 3:
            offset = float2(-0.5, -0.5);
            break;
        case 1:
            offset = float2(-0.5, 0.5);
            break;
        case 2:
        case 4:
            offset = float2(0.5, 0.5);
            break;
        default:
            offset = float2(0.5, -0.5);
            break;
    }

	//position += float3(offset * 0.1, 0.0);

    float4 positionWC = mul(UNITY_MATRIX_M, float4(position, 1.0));
    float4 positionEC = mul(UNITY_MATRIX_V, positionWC);
    float4 positionClip = mul(UNITY_MATRIX_VP, positionWC);
    
    float maximumPointSize = _attenuationParameters.x;
    float geometricError = _attenuationParameters.y;
    float depthMultiplier = computeDepthMultiplier();
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


#ifdef _BUILT_IN_RP
	// For some reason UNITY_MATRIX_I_VP is not available in the built-in render pipeline, so we have to compute it manually.
	float4x4 I_VP = mul(UNITY_MATRIX_I_V, UNITY_MATRIX_I_P);
#else
    float4x4 I_VP = UNITY_MATRIX_I_VP;
#endif
	
 	// Transform the clip space position back to object space.
	// It's unfortunate we need to do this, but shader graph exclusively outputs object space positions.
    positionWC = mul(I_VP, positionClip);
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