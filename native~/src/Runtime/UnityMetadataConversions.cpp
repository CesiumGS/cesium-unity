#include "UnityMetadataConversions.h"

#include <DotNet/Unity/Mathematics/double2.h>
#include <DotNet/Unity/Mathematics/double2x2.h>
#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/Unity/Mathematics/double3x3.h>
#include <DotNet/Unity/Mathematics/double4.h>
#include <DotNet/Unity/Mathematics/double4x4.h>
#include <DotNet/Unity/Mathematics/float2.h>
#include <DotNet/Unity/Mathematics/float2x2.h>
#include <DotNet/Unity/Mathematics/float3.h>
#include <DotNet/Unity/Mathematics/float3x3.h>
#include <DotNet/Unity/Mathematics/float4.h>
#include <DotNet/Unity/Mathematics/float4x4.h>
#include <DotNet/Unity/Mathematics/int2.h>
#include <DotNet/Unity/Mathematics/int2x2.h>
#include <DotNet/Unity/Mathematics/int3.h>
#include <DotNet/Unity/Mathematics/int3x3.h>
#include <DotNet/Unity/Mathematics/int4.h>
#include <DotNet/Unity/Mathematics/int4x4.h>
#include <DotNet/Unity/Mathematics/uint2.h>
#include <DotNet/Unity/Mathematics/uint2x2.h>
#include <DotNet/Unity/Mathematics/uint3.h>
#include <DotNet/Unity/Mathematics/uint3x3.h>
#include <DotNet/Unity/Mathematics/uint4.h>
#include <DotNet/Unity/Mathematics/uint4x4.h>

using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

DotNet::Unity::Mathematics::int2
UnityMetadataConversions::toInt2(const glm::ivec2& vec2) noexcept {
  return int2{vec2[0], vec2[1]};
}

DotNet::Unity::Mathematics::uint2
UnityMetadataConversions::toUint2(const glm::uvec2& vec2) noexcept {
  return uint2{vec2[0], vec2[1]};
}

DotNet::Unity::Mathematics::float2
UnityMetadataConversions::toFloat2(const glm::vec2& vec2) noexcept {
  return float2{vec2[0], vec2[1]};
}

DotNet::Unity::Mathematics::double2
UnityMetadataConversions::toDouble2(const glm::dvec2& vec2) noexcept {
  return double2{vec2[0], vec2[1]};
}

DotNet::Unity::Mathematics::int3
UnityMetadataConversions::toInt3(const glm::ivec3& vec3) noexcept {
  return int3{vec3[0], vec3[1], vec3[2]};
}

DotNet::Unity::Mathematics::uint3
UnityMetadataConversions::toUint3(const glm::uvec3& vec3) noexcept {
  return uint3{vec3[0], vec3[1], vec3[2]};
}

DotNet::Unity::Mathematics::float3
UnityMetadataConversions::toFloat3(const glm::vec3& vec3) noexcept {
  return float3{vec3[0], vec3[1], vec3[2]};
}

DotNet::Unity::Mathematics::double3
UnityMetadataConversions::toDouble3(const glm::dvec3& vec3) noexcept {
  return double3{vec3[0], vec3[1], vec3[2]};
}

DotNet::Unity::Mathematics::int4
UnityMetadataConversions::toInt4(const glm::ivec4& vec4) noexcept {
  return int4{vec4[0], vec4[1], vec4[2], vec4[3]};
}

DotNet::Unity::Mathematics::uint4
UnityMetadataConversions::toUint4(const glm::uvec4& vec4) noexcept {
  return uint4{vec4[0], vec4[1], vec4[2], vec4[3]};
}

DotNet::Unity::Mathematics::float4
UnityMetadataConversions::toFloat4(const glm::vec4& vec4) noexcept {
  return float4{vec4[0], vec4[1], vec4[2], vec4[3]};
}

DotNet::Unity::Mathematics::double4
UnityMetadataConversions::toDouble4(const glm::dvec4& vec4) noexcept {
  return double4{vec4[0], vec4[1], vec4[2], vec4[3]};
}

DotNet::Unity::Mathematics::int2x2
UnityMetadataConversions::toInt2x2(const glm::imat2x2& mat2) noexcept {
  return int2x2::Construct(toInt2(mat2[0]), toInt2(mat2[1]));
}

DotNet::Unity::Mathematics::uint2x2
UnityMetadataConversions::toUint2x2(const glm::umat2x2& mat2) noexcept {
  return uint2x2::Construct(toUint2(mat2[0]), toUint2(mat2[1]));
}

DotNet::Unity::Mathematics::float2x2
UnityMetadataConversions::toFloat2x2(const glm::mat2& mat2) noexcept {
  return float2x2::Construct(toFloat2(mat2[0]), toFloat2(mat2[1]));
}

DotNet::Unity::Mathematics::double2x2
UnityMetadataConversions::toDouble2x2(const glm::dmat2& mat2) noexcept {
  return double2x2::Construct(toDouble2(mat2[0]), toDouble2(mat2[1]));
}

DotNet::Unity::Mathematics::int3x3
UnityMetadataConversions::toInt3x3(const glm::imat3x3& mat3) noexcept {
  return int3x3::Construct(toInt3(mat3[0]), toInt3(mat3[1]), toInt3(mat3[2]));
}

DotNet::Unity::Mathematics::uint3x3
UnityMetadataConversions::toUint3x3(const glm::umat3x3& mat3) noexcept {
  return uint3x3::Construct(
      toUint3(mat3[0]),
      toUint3(mat3[1]),
      toUint3(mat3[2]));
}

DotNet::Unity::Mathematics::float3x3
UnityMetadataConversions::toFloat3x3(const glm::mat3& mat3) noexcept {
  return float3x3::Construct(
      toFloat3(mat3[0]),
      toFloat3(mat3[1]),
      toFloat3(mat3[2]));
}

DotNet::Unity::Mathematics::double3x3
UnityMetadataConversions::toDouble3x3(const glm::dmat3& mat3) noexcept {
  return double3x3::Construct(
      toDouble3(mat3[0]),
      toDouble3(mat3[1]),
      toDouble3(mat3[2]));
}

DotNet::Unity::Mathematics::int4x4
UnityMetadataConversions::toInt4x4(const glm::imat4x4& mat4) noexcept {
  return int4x4::Construct(
      toInt4(mat4[0]),
      toInt4(mat4[1]),
      toInt4(mat4[2]),
      toInt4(mat4[3]));
}

DotNet::Unity::Mathematics::uint4x4
UnityMetadataConversions::toUint4x4(const glm::umat4x4& mat4) noexcept {
  return uint4x4::Construct(
      toUint4(mat4[0]),
      toUint4(mat4[1]),
      toUint4(mat4[2]),
      toUint4(mat4[3]));
}

DotNet::Unity::Mathematics::float4x4
UnityMetadataConversions::toFloat4x4(const glm::mat4& mat4) noexcept {
  return float4x4::Construct(
      toFloat4(mat4[0]),
      toFloat4(mat4[1]),
      toFloat4(mat4[2]),
      toFloat4(mat4[3]));
}

DotNet::Unity::Mathematics::double4x4
UnityMetadataConversions::toDouble4x4(const glm::dmat4& mat4) noexcept {
  return double4x4::Construct(
      toDouble4(mat4[0]),
      toDouble4(mat4[1]),
      toDouble4(mat4[2]),
      toDouble4(mat4[3]));
}

} // namespace CesiumForUnityNative
