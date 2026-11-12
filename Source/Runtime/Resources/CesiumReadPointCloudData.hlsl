#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

#ifndef UNITY_UV_STARTS_AT_TOP
#if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH)
#define UNITY_UV_STARTS_AT_TOP 1
#else
#define UNITY_UV_STARTS_AT_TOP 0
#endif
#endif

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

void CesiumReadPointCloudData_float(float vertexIndex, out float3 position, out float4 color)
{
    #pragma multi_compile _HAS_POINT_COLORS
    #pragma multi_compile _HAS_POINT_NORMALS
    #pragma multi_compile _BUILT_IN_RP
    
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
    
 	// Transform the clip space position back to object space.
	// It's unfortunate we need to do this, but shader graph exclusively outputs object space positions.
#ifdef _BUILT_IN_RP
	// For some reason UNITY_MATRIX_I_VP is not available in the built-in render pipeline, but unity_WorldToObject is.
    float4x4 I_MVP = mul(unity_CameraInvProjection, unity_WorldToObject);
	position = mul(I_MVP, positionClip).xyz;
#else
    positionWC = mul(UNITY_MATRIX_I_VP, positionClip);
    position = mul(UNITY_MATRIX_I_M, positionWC).xyz;
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
    
#ifdef _HAS_POINT_NORMALS
#ifdef _BUILT_IN_RP
    float3 lightWC = _WorldSpaceLightPos0.xyz;
    float3 lightColor = _LightColor0.rgb;
#else 
    float3 lightWC = _MainLightPosition.xyz;
    float3 lightColor = _MainLightColor.rgb;
#endif

    // The shader is unlit, so fake the normal lighting here.
    float3 normalWC = inPoint.normal;
    float NdotL = clamp(dot(normalWC, lightWC), 0.01, 1);

    color.xyz *= lightColor * NdotL;
#endif
}

#endif