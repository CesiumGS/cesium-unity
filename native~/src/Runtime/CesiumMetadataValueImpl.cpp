#include "CesiumMetadataValueImpl.h"

#include "UnityMetadataConversions.h"

#include <CesiumGltf/MetadataConversions.h>

#include <DotNet/CesiumForUnity/CesiumIntMat2x2.h>
#include <DotNet/CesiumForUnity/CesiumIntMat3x3.h>
#include <DotNet/CesiumForUnity/CesiumIntMat4x4.h>
#include <DotNet/CesiumForUnity/CesiumIntVec2.h>
#include <DotNet/CesiumForUnity/CesiumIntVec3.h>
#include <DotNet/CesiumForUnity/CesiumIntVec4.h>
#include <DotNet/CesiumForUnity/CesiumMetadataValue.h>
#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/CesiumForUnity/CesiumUintMat2x2.h>
#include <DotNet/CesiumForUnity/CesiumUintMat3x3.h>
#include <DotNet/CesiumForUnity/CesiumUintMat4x4.h>
#include <DotNet/CesiumForUnity/CesiumUintVec2.h>
#include <DotNet/CesiumForUnity/CesiumUintVec3.h>
#include <DotNet/CesiumForUnity/CesiumUintVec4.h>
#include <DotNet/System/Object.h>
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
#include <swl/variant.hpp>

#include <string_view>

using namespace CesiumGltf;
using namespace DotNet;
using namespace DotNet::CesiumForUnity;
using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

namespace {

template <typename F>
auto getNativeBooleanValue(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  std::optional<bool> maybeBoolean =
      CesiumForUnity::CesiumMetadataValue::GetObjectAsBoolean(
          value.objectValue());
  if (maybeBoolean) {
    return callback(*maybeBoolean);
  }

  return callback(swl::monostate());
}

template <typename T, typename F>
auto getNativeScalarValue(const DotNet::System::Object& object, F&& callback) {
  std::optional<T> maybeValue;
  if constexpr (std::is_same_v<T, int8_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsSByte(object);
  } else if constexpr (std::is_same_v<T, uint8_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsByte(object);
  } else if constexpr (std::is_same_v<T, int16_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsInt16(object);
  } else if constexpr (std::is_same_v<T, uint16_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsUInt16(object);
  } else if constexpr (std::is_same_v<T, int32_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsInt32(object);
  } else if constexpr (std::is_same_v<T, uint32_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsUInt32(object);
  } else if constexpr (std::is_same_v<T, int64_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsInt64(object);
  } else if constexpr (std::is_same_v<T, uint64_t>) {
    maybeValue = CesiumMetadataValue::GetObjectAsUInt64(object);
  } else if constexpr (std::is_same_v<T, float>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat(object);
  } else if constexpr (std::is_same_v<T, double>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble(object);
  }

  if (maybeValue) {
    return callback(*maybeValue);
  }

  return callback(swl::monostate());
}

template <typename F>
auto getNativeScalarValue(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Scalar);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeScalarValue<int8_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeScalarValue<uint8_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeScalarValue<int16_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeScalarValue<uint16_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeScalarValue<int32_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeScalarValue<uint32_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeScalarValue<int64_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeScalarValue<uint64_t>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeScalarValue<float>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeScalarValue<double>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename TTo, typename TFrom, typename F>
auto getNativeVecValue(const DotNet::System::Object& object, F&& callback) {
  std::optional<TFrom> maybeValue;
  if constexpr (std::is_same_v<TFrom, CesiumIntVec2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumIntVec2(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumUintVec2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumUintVec2(object);
  } else if constexpr (std::is_same_v<TFrom, float2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat2(object);
  } else if constexpr (std::is_same_v<TFrom, double2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble2(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumIntVec3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumIntVec3(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumUintVec3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumUintVec3(object);
  } else if constexpr (std::is_same_v<TFrom, float3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat3(object);
  } else if constexpr (std::is_same_v<TFrom, double3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble3(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumIntVec4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumIntVec4(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumUintVec4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumUintVec4(object);
  } else if constexpr (std::is_same_v<TFrom, float4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat4(object);
  } else if constexpr (std::is_same_v<TFrom, double4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble4(object);
  }

  if (!maybeValue) {
    return callback(swl::monostate());
  }

  TTo result = TTo();
  TFrom value = *maybeValue;
  for (glm::length_t i = 0; i < TTo::length(); i++) {
    result[i] = value[i];
  }

  return callback(std::move(result));
}

template <typename F>
auto getNativeVec2Value(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Vec2);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeVecValue<glm::i8vec2, CesiumIntVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeVecValue<glm::u8vec2, CesiumUintVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeVecValue<glm::i16vec2, CesiumIntVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeVecValue<glm::u16vec2, CesiumUintVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeVecValue<glm::ivec2, CesiumIntVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeVecValue<glm::uvec2, CesiumUintVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeVecValue<glm::i64vec2, CesiumIntVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeVecValue<glm::u64vec2, CesiumUintVec2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeVecValue<glm::vec2, float2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeVecValue<glm::dvec2, double2>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename F>
auto getNativeVec3Value(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Vec3);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeVecValue<glm::i8vec3, CesiumIntVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeVecValue<glm::u8vec3, CesiumUintVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeVecValue<glm::i16vec3, CesiumIntVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeVecValue<glm::u16vec3, CesiumUintVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeVecValue<glm::ivec3, CesiumIntVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeVecValue<glm::uvec3, CesiumUintVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeVecValue<glm::i64vec3, CesiumIntVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeVecValue<glm::u64vec3, CesiumUintVec3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeVecValue<glm::vec3, float3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeVecValue<glm::dvec3, double3>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename F>
auto getNativeVec4Value(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Vec4);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeVecValue<glm::i8vec4, CesiumIntVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeVecValue<glm::u8vec4, CesiumUintVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeVecValue<glm::i16vec4, CesiumIntVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeVecValue<glm::u16vec4, CesiumUintVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeVecValue<glm::ivec4, CesiumIntVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeVecValue<glm::uvec4, CesiumUintVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeVecValue<glm::i64vec4, CesiumIntVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeVecValue<glm::u64vec4, CesiumUintVec4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeVecValue<glm::vec4, float4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeVecValue<glm::dvec4, double4>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename TTo, typename TFrom, typename F>
auto getNativeMatValue(const DotNet::System::Object& object, F&& callback) {
  std::optional<TFrom> maybeValue;
  if constexpr (std::is_same_v<TFrom, CesiumIntMat2x2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumIntMat2x2(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumUintMat2x2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumUintMat2x2(object);
  } else if constexpr (std::is_same_v<TFrom, float2x2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat2x2(object);
  } else if constexpr (std::is_same_v<TFrom, double2x2>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble2x2(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumIntMat3x3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumIntMat3x3(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumUintMat3x3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumUintMat3x3(object);
  } else if constexpr (std::is_same_v<TFrom, float3x3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat3x3(object);
  } else if constexpr (std::is_same_v<TFrom, double3x3>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble3x3(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumIntMat4x4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumIntMat4x4(object);
  } else if constexpr (std::is_same_v<TFrom, CesiumUintMat4x4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsCesiumUintMat4x4(object);
  } else if constexpr (std::is_same_v<TFrom, float4x4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsFloat4x4(object);
  } else if constexpr (std::is_same_v<TFrom, double4x4>) {
    maybeValue = CesiumMetadataValue::GetObjectAsDouble4x4(object);
  }

  if (!maybeValue) {
    return callback(swl::monostate());
  }

  TTo result = TTo();
  TFrom value = *maybeValue;
  for (glm::length_t i = 0; i < TTo::length(); i++) {
    for (glm::length_t j = 0; j < TTo::length(); j++) {
      result[i][j] = value[i][j];
    }
  }

  return callback(std::move(result));
}

template <typename F>
auto getNativeMat2Value(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Mat2);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeMatValue<glm::i8mat2x2, CesiumIntMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeMatValue<glm::u8mat2x2, CesiumUintMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeMatValue<glm::i16mat2x2, CesiumIntMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeMatValue<glm::u16mat2x2, CesiumUintMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeMatValue<glm::imat2x2, CesiumIntMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeMatValue<glm::umat2x2, CesiumUintMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeMatValue<glm::i64mat2x2, CesiumIntMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeMatValue<glm::u64mat2x2, CesiumUintMat2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeMatValue<glm::mat2x2, float2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeMatValue<glm::dmat2x2, double2x2>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename F>
auto getNativeMat3Value(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Mat3);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeMatValue<glm::i8mat3x3, CesiumIntMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeMatValue<glm::u8mat3x3, CesiumUintMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeMatValue<glm::i16mat3x3, CesiumIntMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeMatValue<glm::u16mat3x3, CesiumUintMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeMatValue<glm::imat3x3, CesiumIntMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeMatValue<glm::umat3x3, CesiumUintMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeMatValue<glm::i64mat3x3, CesiumIntMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeMatValue<glm::u64mat3x3, CesiumUintMat3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeMatValue<glm::mat3x3, float3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeMatValue<glm::dmat3x3, double3x3>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename F>
auto getNativeMat4Value(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumMetadataValueType valueType = value.valueType();
  assert(valueType.type == CesiumMetadataType::Mat4);
  switch (valueType.componentType) {
  case CesiumMetadataComponentType::Int8:
    return getNativeMatValue<glm::i8mat4x4, CesiumIntMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint8:
    return getNativeMatValue<glm::u8mat4x4, CesiumUintMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int16:
    return getNativeMatValue<glm::i16mat4x4, CesiumIntMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint16:
    return getNativeMatValue<glm::u16mat4x4, CesiumUintMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int32:
    return getNativeMatValue<glm::imat4x4, CesiumIntMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint32:
    return getNativeMatValue<glm::umat4x4, CesiumUintMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Int64:
    return getNativeMatValue<glm::i64mat4x4, CesiumIntMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Uint64:
    return getNativeMatValue<glm::u64mat4x4, CesiumUintMat4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float32:
    return getNativeMatValue<glm::mat4x4, float4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  case CesiumMetadataComponentType::Float64:
    return getNativeMatValue<glm::dmat4x4, double4x4>(
        value.objectValue(),
        std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

template <typename F>
auto getNativeStringValue(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  DotNet::System::String string =
      CesiumForUnity::CesiumMetadataValue::GetObjectAsString(
          value.objectValue());
  if (string == nullptr) {
    return callback(swl::monostate());
  }

  return callback(string.ToStlString());
}

template <typename F>
auto getNativeValue(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    F&& callback) {
  CesiumForUnity::CesiumMetadataValueType valueType = value.valueType();
  switch (valueType.type) {
  case CesiumForUnity::CesiumMetadataType::Boolean:
    return getNativeBooleanValue(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Scalar:
    return getNativeScalarValue(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Vec2:
    return getNativeVec2Value(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Vec3:
    return getNativeVec3Value(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Vec4:
    return getNativeVec4Value(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Mat2:
    return getNativeMat2Value(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Mat3:
    return getNativeMat3Value(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::Mat4:
    return getNativeMat4Value(value, std::forward<F>(callback));
  case CesiumForUnity::CesiumMetadataType::String:
    return getNativeStringValue(value, std::forward<F>(callback));
  default:
    return callback(swl::monostate());
  }
}

} // namespace

/*static*/ bool CesiumMetadataValueImpl::ConvertToBoolean(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    bool defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<bool, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ int8_t CesiumMetadataValueImpl::ConvertToSByte(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int8_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> int8_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<int8_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ uint8_t CesiumMetadataValueImpl::ConvertToByte(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint8_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> uint8_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<uint8_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ int16_t CesiumMetadataValueImpl::ConvertToInt16(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int16_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> int16_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<int16_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ uint16_t CesiumMetadataValueImpl::ConvertToUInt16(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint16_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> uint16_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<uint16_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ int32_t CesiumMetadataValueImpl::ConvertToInt32(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int32_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> int32_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<int32_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ uint32_t CesiumMetadataValueImpl::ConvertToUInt32(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint32_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> uint32_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<uint32_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}
/*static*/ int64_t CesiumMetadataValueImpl::ConvertToInt64(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int64_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> int64_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<int64_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ uint64_t CesiumMetadataValueImpl::ConvertToUInt64(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint64_t defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> uint64_t {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<uint64_t, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ float CesiumMetadataValueImpl::ConvertToFloat(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> float {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<float, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ double CesiumMetadataValueImpl::ConvertToDouble(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double defaultValue) {
  return getNativeValue(value, [defaultValue](auto&& trueValue) -> double {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      return MetadataConversions<double, TValue>::convert(trueValue).value_or(
          defaultValue);
    }
  });
}

/*static*/ int2 CesiumMetadataValueImpl::ConvertToInt2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> int2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::ivec2> maybeVec2 =
          MetadataConversions<glm::ivec2, TValue>::convert(trueValue);
      return maybeVec2 ? UnityMetadataConversions::toInt2(*maybeVec2)
                       : defaultValue;
    }
  });
}

/*static*/ uint2 CesiumMetadataValueImpl::ConvertToUInt2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> uint2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::uvec2> maybeVec2 =
          MetadataConversions<glm::uvec2, TValue>::convert(trueValue);
      return maybeVec2 ? UnityMetadataConversions::toUint2(*maybeVec2)
                       : defaultValue;
    }
  });
}

/*static*/ float2 CesiumMetadataValueImpl::ConvertToFloat2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> float2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::vec2> maybeVec2 =
          MetadataConversions<glm::vec2, TValue>::convert(trueValue);
      return maybeVec2 ? UnityMetadataConversions::toFloat2(*maybeVec2)
                       : defaultValue;
    }
  });
}

/*static*/ double2 CesiumMetadataValueImpl::ConvertToDouble2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> double2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::dvec2> maybeVec2 =
          MetadataConversions<glm::dvec2, TValue>::convert(trueValue);
      return maybeVec2 ? UnityMetadataConversions::toDouble2(*maybeVec2)
                       : defaultValue;
    }
  });
}

/*static*/ int3 CesiumMetadataValueImpl::ConvertToInt3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> int3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::ivec3> maybeVec3 =
          MetadataConversions<glm::ivec3, TValue>::convert(trueValue);
      return maybeVec3 ? UnityMetadataConversions::toInt3(*maybeVec3)
                       : defaultValue;
    }
  });
}

/*static*/ uint3 CesiumMetadataValueImpl::ConvertToUInt3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> uint3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::uvec3> maybeVec3 =
          MetadataConversions<glm::uvec3, TValue>::convert(trueValue);
      return maybeVec3 ? UnityMetadataConversions::toUint3(*maybeVec3)
                       : defaultValue;
    }
  });
}

/*static*/ float3 CesiumMetadataValueImpl::ConvertToFloat3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> float3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::vec3> maybeVec3 =
          MetadataConversions<glm::vec3, TValue>::convert(trueValue);
      return maybeVec3 ? UnityMetadataConversions::toFloat3(*maybeVec3)
                       : defaultValue;
    }
  });
}

/*static*/ double3 CesiumMetadataValueImpl::ConvertToDouble3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> double3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::dvec3> maybeVec3 =
          MetadataConversions<glm::dvec3, TValue>::convert(trueValue);
      return maybeVec3 ? UnityMetadataConversions::toDouble3(*maybeVec3)
                       : defaultValue;
    }
  });
}

/*static*/ int4 CesiumMetadataValueImpl::ConvertToInt4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> int4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::ivec4> maybeVec4 =
          MetadataConversions<glm::ivec4, TValue>::convert(trueValue);
      return maybeVec4 ? UnityMetadataConversions::toInt4(*maybeVec4)
                       : defaultValue;
    }
  });
}

/*static*/ uint4 CesiumMetadataValueImpl::ConvertToUInt4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> uint4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::uvec4> maybeVec4 =
          MetadataConversions<glm::uvec4, TValue>::convert(trueValue);
      return maybeVec4 ? UnityMetadataConversions::toUint4(*maybeVec4)
                       : defaultValue;
    }
  });
}

/*static*/ float4 CesiumMetadataValueImpl::ConvertToFloat4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> float4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::vec4> maybeVec4 =
          MetadataConversions<glm::vec4, TValue>::convert(trueValue);
      return maybeVec4 ? UnityMetadataConversions::toFloat4(*maybeVec4)
                       : defaultValue;
    }
  });
}

/*static*/ double4 CesiumMetadataValueImpl::ConvertToDouble4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> double4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::dvec4> maybeVec4 =
          MetadataConversions<glm::dvec4, TValue>::convert(trueValue);
      return maybeVec4 ? UnityMetadataConversions::toDouble4(*maybeVec4)
                       : defaultValue;
    }
  });
}

/*static*/ int2x2 CesiumMetadataValueImpl::ConvertToInt2x2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int2x2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> int2x2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::imat2x2> maybeMat2 =
          MetadataConversions<glm::imat2x2, TValue>::convert(trueValue);
      return maybeMat2 ? UnityMetadataConversions::toInt2x2(*maybeMat2)
                       : defaultValue;
    }
  });
}

/*static*/ uint2x2 CesiumMetadataValueImpl::ConvertToUInt2x2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint2x2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> uint2x2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::umat2x2> maybeMat2 =
          MetadataConversions<glm::umat2x2, TValue>::convert(trueValue);
      return maybeMat2 ? UnityMetadataConversions::toUint2x2(*maybeMat2)
                       : defaultValue;
    }
  });
}

/*static*/ float2x2 CesiumMetadataValueImpl::ConvertToFloat2x2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float2x2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> float2x2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::mat2> maybeMat2 =
          MetadataConversions<glm::mat2, TValue>::convert(trueValue);
      return maybeMat2 ? UnityMetadataConversions::toFloat2x2(*maybeMat2)
                       : defaultValue;
    }
  });
}

/*static*/ double2x2 CesiumMetadataValueImpl::ConvertToDouble2x2(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double2x2 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> double2x2 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::dmat2> maybeMat2 =
          MetadataConversions<glm::dmat2, TValue>::convert(trueValue);
      return maybeMat2 ? UnityMetadataConversions::toDouble2x2(*maybeMat2)
                       : defaultValue;
    }
  });
}

/*static*/ int3x3 CesiumMetadataValueImpl::ConvertToInt3x3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int3x3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> int3x3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::imat3x3> maybeMat3 =
          MetadataConversions<glm::imat3x3, TValue>::convert(trueValue);
      return maybeMat3 ? UnityMetadataConversions::toInt3x3(*maybeMat3)
                       : defaultValue;
    }
  });
}

/*static*/ uint3x3 CesiumMetadataValueImpl::ConvertToUInt3x3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint3x3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> uint3x3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::umat3x3> maybeMat3 =
          MetadataConversions<glm::umat3x3, TValue>::convert(trueValue);
      return maybeMat3 ? UnityMetadataConversions::toUint3x3(*maybeMat3)
                       : defaultValue;
    }
  });
}

/*static*/ float3x3 CesiumMetadataValueImpl::ConvertToFloat3x3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float3x3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> float3x3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::mat3> maybeMat3 =
          MetadataConversions<glm::mat3, TValue>::convert(trueValue);
      return maybeMat3 ? UnityMetadataConversions::toFloat3x3(*maybeMat3)
                       : defaultValue;
    }
  });
}

/*static*/ double3x3 CesiumMetadataValueImpl::ConvertToDouble3x3(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double3x3 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> double3x3 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::dmat3> maybeMat3 =
          MetadataConversions<glm::dmat3, TValue>::convert(trueValue);
      return maybeMat3 ? UnityMetadataConversions::toDouble3x3(*maybeMat3)
                       : defaultValue;
    }
  });
}

/*static*/ int4x4 CesiumMetadataValueImpl::ConvertToInt4x4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    int4x4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> int4x4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::imat4x4> maybeMat4 =
          MetadataConversions<glm::imat4x4, TValue>::convert(trueValue);
      return maybeMat4 ? UnityMetadataConversions::toInt4x4(*maybeMat4)
                       : defaultValue;
    }
  });
}

/*static*/ uint4x4 CesiumMetadataValueImpl::ConvertToUInt4x4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    uint4x4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> uint4x4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<TValue, swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::umat4x4> maybeMat4 =
          MetadataConversions<glm::umat4x4, TValue>::convert(trueValue);
      return maybeMat4 ? UnityMetadataConversions::toUint4x4(*maybeMat4)
                       : defaultValue;
    }
  });
}

/*static*/ float4x4 CesiumMetadataValueImpl::ConvertToFloat4x4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    float4x4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> float4x4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<
                      std::remove_cvref_t<decltype(trueValue)>,
                      swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::mat4> maybeMat4 = MetadataConversions<
          glm::mat4,
          std::remove_cvref_t<decltype(trueValue)>>::convert(trueValue);
      return maybeMat4 ? UnityMetadataConversions::toFloat4x4(*maybeMat4)
                       : defaultValue;
    }
  });
}

/*static*/ double4x4 CesiumMetadataValueImpl::ConvertToDouble4x4(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    double4x4 defaultValue) {
  return getNativeValue(value, [&defaultValue](auto&& trueValue) -> double4x4 {
    using TValue = std::remove_cvref_t<decltype(trueValue)>;
    if constexpr (std::is_same_v<
                      std::remove_cvref_t<decltype(trueValue)>,
                      swl::monostate>) {
      return defaultValue;
    } else {
      std::optional<glm::dmat4> maybeMat4 = MetadataConversions<
          glm::dmat4,
          std::remove_cvref_t<decltype(trueValue)>>::convert(trueValue);
      return maybeMat4 ? UnityMetadataConversions::toDouble4x4(*maybeMat4)
                       : defaultValue;
    }
  });
}

/*static*/ DotNet::System::String CesiumMetadataValueImpl::ConvertToString(
    const DotNet::CesiumForUnity::CesiumMetadataValue& value,
    DotNet::System::String defaultValue) {
  return getNativeValue(
      value,
      [&defaultValue](auto&& trueValue) -> System::String {
        using TValue = std::remove_cvref_t<decltype(trueValue)>;
        if constexpr (std::is_same_v<
                          std::remove_cvref_t<decltype(trueValue)>,
                          swl::monostate>) {
          return defaultValue;
        } else {
          auto maybeString = MetadataConversions<
              std::string,
              std::remove_cvref_t<decltype(trueValue)>>::convert(trueValue);
          return maybeString ? System::String(*maybeString) : defaultValue;
        }
      });
}

} // namespace CesiumForUnityNative
