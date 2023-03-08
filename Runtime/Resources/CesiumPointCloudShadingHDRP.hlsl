#ifndef CESIUM_POINT_CLOUD_SHADING
#define CESIUM_POINT_CLOUD_SHADING

//#include "UnityCG.cginc"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixV;
float4x4 unity_MatrixVP;
CBUFFER_END

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

#ifndef HAS_POINT_COLORS
float4 _constantColor;
#endif

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
/*
VaryingsMeshType CreateVaryingsMeshType(VertexInput input, float2 offset) {
	VaryingsMeshType output;

	float4 positionWC = mul(_worldTransform, float4(input.position, 1.0));
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
	
	output.positionCS = positionClip;

#ifdef HAS_POINT_NORMALS
	float4 normalWC = mul(_worldTransform, float4(input.normal, 0));
	float4 tangentWC = float4(cross(normalWC.xyz, float3(0, 1, 0)), 1.0);
	output.normalWS = normalWC.xyz;
	output.tangentWS = tangentWC;
#endif

#ifdef HAS_POINT_COLORS
	uint packedColor = input.packedColor;
	uint r = packedColor & 255;
	uint g = (packedColor >> 8) & 255;
	uint b = (packedColor >> 16) & 255;
	uint a = packedColor >> 24;
	output.color = float4(r, g, b, a) / 255;
#else
	output.color = _constantColor;
#endif

	return output;
}

PackedVaryingsType PointCloudShadingVertex(uint vertexID : SV_VertexID) {
	VertexOutput output;
	uint pointIndex = vertexID / 6;
	uint vertexIndex = vertexID - (pointIndex * 6); // Modulo
	VertexInput input = _inVertices[pointIndex];
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

	VaryingsType varyingsType;
	varyingsType.vmesh = CreateVaryingsMeshType(input, offset);
	
	return PackVaryingsType(varyingsType);
}*/

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
	//positionClip.xy += screenOffset * positionClip.w;
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
	float3 lightWC = _WorldSpaceLightPos0.xyz;
	float3 lightColor = _LightColor0.rgb;

	float3 normalWC = input.normalWC;
	float NdotL = saturate(dot(normalWC, lightWC)); // saturate() clamps between 0 and 1

	result.xyz *= lightColor * NdotL;
	#endif

	return result;
	#endif
}

void FragmentCommon(VertexOutput input, out SurfaceData surfaceData, out BuiltinData builtinData) {
	float4 color;
	// The depth / shadow caster pass renders to a shadow map, so we can ignore
	// the color logic here.
#if defined(DEPTH_PASS) || defined(SHADOW_CASTER_PASS)
	color = float4(1, 1, 1, 1);
#elif defined(HAS_POINT_COLORS)
	uint packedColor = input.packedColor;
	uint r = packedColor & 255;
	uint g = (packedColor >> 8) & 255;
	uint b = (packedColor >> 16) & 255;
	uint a = packedColor >> 24;
	color = float4(r, g, b, a) / 255;
#else
	color = _constantColor;
#endif

	ZERO_INITIALIZE(SurfaceData, surfaceData);
	ZERO_INITIALIZE(BuiltinData, builtinData);

	surfaceData.baseColor = float4(1, 1, 1, 1);// color.xyz;
#ifdef HAS_POINT_NORMALS
	surfaceData.normalWS = input.normalWC;
#endif

	builtinData.opacity = 1;// color.w;
}

void FragmentGBuffer(VertexOutput input,
	OUTPUT_GBUFFER(outGBuffer)
#ifdef _DEPTHOFFSET_ON
	, out float outputDepth : DEPTH_OFFSET_SEMANTIC
#endif
) {
	SurfaceData surfaceData;
	BuiltinData builtinData;
	FragmentCommon(input, surfaceData, builtinData);
	ENCODE_INTO_GBUFFER(surfaceData, builtinData, input.positionClip.xy, outGBuffer);

	#ifdef _DEPTHOFFSET_ON
	outputDepth = input.positionClip.z;
	#endif
}

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl"

float4 FragmentMeta(VertexOutput input) : SV_Target {
	SurfaceData surfaceData;
	BuiltinData builtinData;
	FragmentCommon(input, surfaceData, builtinData);

	// Code taken from ShaderPassLightTransport.hlsl
	BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionClip.xy, surfaceData);
	LightTransportData lightTransportData = GetLightTransportData(surfaceData, builtinData, bsdfData);

	float4 res = float4(0.0, 0.0, 0.0, 1.0);

	UnityMetaInput metaInput;
	metaInput.Albedo = lightTransportData.diffuseColor.rgb;
	metaInput.Emission = lightTransportData.emissiveColor;
	
	res = UnityMetaFragment(metaInput);

	return res;
}

void FragmentDepthOnly(VertexOutput input
#ifdef WRITE_MSAA_DEPTH
	// We need the depth color as SV_Target0 for alpha to coverage
	, out float4 depthColor : SV_Target0
#if defined(WRITE_NORMAL_BUFFER)
	, out float4 outNormalBuffer : SV_Target1
#endif
#elif defined(WRITE_NORMAL_BUFFER)
	, out float4 outNormalBuffer : SV_Target0
#endif
#ifdef _DEPTHOFFSET_ON
	, out float outputDepth : DEPTH_OFFSET_SEMANTIC
#endif
)
{
	// Code taken from ShaderPassDepthOnly.hlsl
	SurfaceData surfaceData;
	BuiltinData builtinData;
	FragmentCommon(input, surfaceData, builtinData);

#ifdef _DEPTHOFFSET_ON
	float depth = input.positionClip.z;
	outputDepth = depth;

#ifdef SHADOW_CASTER_PASS
	// If we are using the depth offset and manually outputting depth, the slope-scale depth bias is not properly applied
	// we need to manually apply.
	
	float bias = max(abs(ddx(depth)), abs(ddy(depth))) * _SlopeScaleDepthBias;
	outputDepth += bias;
#endif
#endif

#if defined(HAS_POINT_NORMALS) && defined(WRITE_NORMAL_BUFFER)
	EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
#endif
}


void FragmentForward(VertexOutput input
	, out float4 outColor : SV_Target0)
{
	outColor = float4(1, 0, 0, 1);
	// Code taken from ShaderPassDepthOnly.hlsl
	//SurfaceData surfaceData;
	//BuiltinData builtinData;
	//FragmentCommon(input, surfaceData, builtinData);

/*
#ifdef _DEPTHOFFSET_ON
	float depth = input.positionClip.z;
	outputDepth = depth;

#ifdef SHADOW_CASTER_PASS
	// If we are using the depth offset and manually outputting depth, the slope-scale depth bias is not properly applied
	// we need to manually apply.

	float bias = max(abs(ddx(depth)), abs(ddy(depth))) * _SlopeScaleDepthBias;
	outputDepth += bias;
#endif
#endif

#if defined(HAS_POINT_NORMALS) && defined(WRITE_NORMAL_BUFFER)
	EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);
#endif*/
}
#endif
