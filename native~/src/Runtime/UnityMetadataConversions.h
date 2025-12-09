#pragma once

#include <DotNet/CesiumForUnity/CesiumIntMat2x2.h>
#include <DotNet/CesiumForUnity/CesiumIntMat3x3.h>
#include <DotNet/CesiumForUnity/CesiumIntMat4x4.h>
#include <DotNet/CesiumForUnity/CesiumIntVec2.h>
#include <DotNet/CesiumForUnity/CesiumIntVec3.h>
#include <DotNet/CesiumForUnity/CesiumIntVec4.h>
#include <DotNet/CesiumForUnity/CesiumUintMat2x2.h>
#include <DotNet/CesiumForUnity/CesiumUintMat3x3.h>
#include <DotNet/CesiumForUnity/CesiumUintMat4x4.h>
#include <DotNet/CesiumForUnity/CesiumUintVec2.h>
#include <DotNet/CesiumForUnity/CesiumUintVec3.h>
#include <DotNet/CesiumForUnity/CesiumUintVec4.h>
#include <glm/glm.hpp>

#include <type_traits>

namespace DotNet::UnityEngine {
class GameObject;
}

namespace DotNet::Unity::Mathematics {
struct int2;
struct int3;
struct int4;
struct int2x2;
struct int3x3;
struct int4x4;
struct uint2;
struct uint3;
struct uint4;
struct uint2x2;
struct uint3x3;
struct uint4x4;
struct float2;
struct float3;
struct float4;
struct float2x2;
struct float3x3;
struct float4x4;
struct double2;
struct double3;
struct double4;
struct double2x2;
struct double3x3;
struct double4x4;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class UnityMetadataConversions {
public:
  template <typename T>
  static DotNet::CesiumForUnity::CesiumIntVec2
  toCesiumIntVec2(const glm::vec<2, T>& vec2);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumIntVec3
  toCesiumIntVec3(const glm::vec<3, T>& vec3);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumIntVec4
  toCesiumIntVec4(const glm::vec<4, T>& vec4);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumUintVec2
  toCesiumUintVec2(const glm::vec<2, T>& vec2);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumUintVec3
  toCesiumUintVec3(const glm::vec<3, T>& vec3);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumUintVec4
  toCesiumUintVec4(const glm::vec<4, T>& vec4);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumIntMat2x2
  toCesiumIntMat2x2(const glm::mat<2, 2, T>& mat2);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumIntMat3x3
  toCesiumIntMat3x3(const glm::mat<3, 3, T>& mat3);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumIntMat4x4
  toCesiumIntMat4x4(const glm::mat<4, 4, T>& mat4);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumUintMat2x2
  toCesiumUintMat2x2(const glm::mat<2, 2, T>& mat2);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumUintMat3x3
  toCesiumUintMat3x3(const glm::mat<3, 3, T>& mat3);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumUintMat4x4
  toCesiumUintMat4x4(const glm::mat<4, 4, T>& mat4);

  static DotNet::Unity::Mathematics::int2
  toInt2(const glm::ivec2& vec2) noexcept;
  static DotNet::Unity::Mathematics::uint2
  toUint2(const glm::uvec2& vec2) noexcept;
  static DotNet::Unity::Mathematics::float2
  toFloat2(const glm::vec2& vec2) noexcept;
  static DotNet::Unity::Mathematics::double2
  toDouble2(const glm::dvec2& vec2) noexcept;

  static DotNet::Unity::Mathematics::int3
  toInt3(const glm::ivec3& vec3) noexcept;
  static DotNet::Unity::Mathematics::uint3
  toUint3(const glm::uvec3& vec3) noexcept;
  static DotNet::Unity::Mathematics::float3
  toFloat3(const glm::vec3& vec3) noexcept;
  static DotNet::Unity::Mathematics::double3
  toDouble3(const glm::dvec3& vec3) noexcept;

  static DotNet::Unity::Mathematics::int4
  toInt4(const glm::ivec4& vec4) noexcept;
  static DotNet::Unity::Mathematics::uint4
  toUint4(const glm::uvec4& vec4) noexcept;
  static DotNet::Unity::Mathematics::float4
  toFloat4(const glm::vec4& vec4) noexcept;
  static DotNet::Unity::Mathematics::double4
  toDouble4(const glm::dvec4& vec4) noexcept;

  static DotNet::Unity::Mathematics::int2x2
  toInt2x2(const glm::imat2x2& mat2) noexcept;
  static DotNet::Unity::Mathematics::uint2x2
  toUint2x2(const glm::umat2x2& mat2) noexcept;
  static DotNet::Unity::Mathematics::float2x2
  toFloat2x2(const glm::mat2& mat2) noexcept;
  static DotNet::Unity::Mathematics::double2x2
  toDouble2x2(const glm::dmat2& mat2) noexcept;

  static DotNet::Unity::Mathematics::int3x3
  toInt3x3(const glm::imat3x3& mat3) noexcept;
  static DotNet::Unity::Mathematics::uint3x3
  toUint3x3(const glm::umat3x3& mat3) noexcept;
  static DotNet::Unity::Mathematics::float3x3
  toFloat3x3(const glm::mat3& mat3) noexcept;
  static DotNet::Unity::Mathematics::double3x3
  toDouble3x3(const glm::dmat3& mat3) noexcept;

  static DotNet::Unity::Mathematics::int4x4
  toInt4x4(const glm::imat4x4& mat4) noexcept;
  static DotNet::Unity::Mathematics::uint4x4
  toUint4x4(const glm::umat4x4& mat4) noexcept;
  static DotNet::Unity::Mathematics::float4x4
  toFloat4x4(const glm::mat4& mat4) noexcept;
  static DotNet::Unity::Mathematics::double4x4
  toDouble4x4(const glm::dmat4& mat4) noexcept;
};

template <typename T>
DotNet::CesiumForUnity::CesiumIntVec2
UnityMetadataConversions::toCesiumIntVec2(const glm::vec<2, T>& vec2) {
  return DotNet::CesiumForUnity::CesiumIntVec2::Construct(vec2[0], vec2[1]);
}

template <typename T>
DotNet::CesiumForUnity::CesiumIntVec3
UnityMetadataConversions::toCesiumIntVec3(const glm::vec<3, T>& vec3) {
  return DotNet::CesiumForUnity::CesiumIntVec3::Construct(
      vec3[0],
      vec3[1],
      vec3[2]);
}

template <typename T>
DotNet::CesiumForUnity::CesiumIntVec4
UnityMetadataConversions::toCesiumIntVec4(const glm::vec<4, T>& vec4) {
  return DotNet::CesiumForUnity::CesiumIntVec4::Construct(
      vec4[0],
      vec4[1],
      vec4[2],
      vec4[3]);
}

template <typename T>
DotNet::CesiumForUnity::CesiumUintVec2
UnityMetadataConversions::toCesiumUintVec2(const glm::vec<2, T>& vec2) {
  return DotNet::CesiumForUnity::CesiumUintVec2::Construct(vec2[0], vec2[1]);
}

template <typename T>
DotNet::CesiumForUnity::CesiumUintVec3
UnityMetadataConversions::toCesiumUintVec3(const glm::vec<3, T>& vec3) {
  return DotNet::CesiumForUnity::CesiumUintVec3::Construct(
      vec3[0],
      vec3[1],
      vec3[2]);
}

template <typename T>
DotNet::CesiumForUnity::CesiumUintVec4
UnityMetadataConversions::toCesiumUintVec4(const glm::vec<4, T>& vec4) {
  return DotNet::CesiumForUnity::CesiumUintVec4::Construct(
      vec4[0],
      vec4[1],
      vec4[2],
      vec4[3]);
}

template <typename T>
DotNet::CesiumForUnity::CesiumIntMat2x2
UnityMetadataConversions::toCesiumIntMat2x2(const glm::mat<2, 2, T>& mat2) {
  return DotNet::CesiumForUnity::CesiumIntMat2x2::Construct(
      toCesiumIntVec2(mat2[0]),
      toCesiumIntVec2(mat2[1]));
}

template <typename T>
DotNet::CesiumForUnity::CesiumIntMat3x3
UnityMetadataConversions::toCesiumIntMat3x3(const glm::mat<3, 3, T>& mat3) {
  return DotNet::CesiumForUnity::CesiumIntMat3x3::Construct(
      toCesiumIntVec3(mat3[0]),
      toCesiumIntVec3(mat3[1]),
      toCesiumIntVec3(mat3[2]));
}

template <typename T>
DotNet::CesiumForUnity::CesiumIntMat4x4
UnityMetadataConversions::toCesiumIntMat4x4(const glm::mat<4, 4, T>& mat4) {
  return DotNet::CesiumForUnity::CesiumIntMat4x4::Construct(
      toCesiumIntVec4(mat4[0]),
      toCesiumIntVec4(mat4[1]),
      toCesiumIntVec4(mat4[2]),
      toCesiumIntVec4(mat4[3]));
}

template <typename T>
DotNet::CesiumForUnity::CesiumUintMat2x2
UnityMetadataConversions::toCesiumUintMat2x2(const glm::mat<2, 2, T>& mat2) {
  return DotNet::CesiumForUnity::CesiumUintMat2x2::Construct(
      toCesiumUintVec2(mat2[0]),
      toCesiumUintVec2(mat2[1]));
}

template <typename T>
DotNet::CesiumForUnity::CesiumUintMat3x3
UnityMetadataConversions::toCesiumUintMat3x3(const glm::mat<3, 3, T>& mat3) {
  return DotNet::CesiumForUnity::CesiumUintMat3x3::Construct(
      toCesiumUintVec3(mat3[0]),
      toCesiumUintVec3(mat3[1]),
      toCesiumUintVec3(mat3[2]));
}

template <typename T>
DotNet::CesiumForUnity::CesiumUintMat4x4
UnityMetadataConversions::toCesiumUintMat4x4(const glm::mat<4, 4, T>& mat4) {
  return DotNet::CesiumForUnity::CesiumUintMat4x4::Construct(
      toCesiumUintVec4(mat4[0]),
      toCesiumUintVec4(mat4[1]),
      toCesiumUintVec4(mat4[2]),
      toCesiumUintVec4(mat4[3]));
}

} // namespace CesiumForUnityNative
