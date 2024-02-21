#pragma once

#ifndef GLM_ENABLE_EXPERIMENTAL
#define GLM_ENABLE_EXPERIMENTAL
#endif

#include "CesiumPropertyTablePropertyImpl.h"

#include "CesiumFeaturesMetadataUtility.h"
#include "UnityMetadataConversions.h"

#include <CesiumGltf/MetadataConversions.h>
#include <CesiumGltf/PropertyTypeTraits.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>
#include <DotNet/System/String.h>
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

using namespace CesiumGltf;
using namespace DotNet;
using namespace DotNet::UnityEngine;
using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

namespace {
/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * of the specified type. If the type does not match, the callback is performed
 * on an invalid PropertyTablePropertyView instead.
 *
 * @param property The std::any containing the property.
 * @param callback The callback function.
 *
 * @tparam TProperty The property type.
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <
    typename TProperty,
    bool Normalized,
    typename TResult,
    typename Callback>
TResult
propertyTablePropertyCallback(const std::any& property, Callback&& callback) {
  const PropertyTablePropertyView<TProperty, Normalized>* pProperty =
      std::any_cast<PropertyTablePropertyView<TProperty, Normalized>>(
          &property);
  if (pProperty) {
    return callback(*pProperty);
  }

  return callback(PropertyTablePropertyView<uint8_t>());
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a scalar type. If the valueType does not have a valid component type, the
 * callback is performed on an invalid PropertyTablePropertyView instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <bool Normalized, typename TResult, typename Callback>
TResult scalarPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.componentType) {
  case CesiumForUnity::CesiumMetadataComponentType::Int8:
    return propertyTablePropertyCallback<int8_t, Normalized, TResult, Callback>(
        property,
        std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint8:
    return propertyTablePropertyCallback<
        uint8_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int16:
    return propertyTablePropertyCallback<
        int16_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint16:
    return propertyTablePropertyCallback<
        uint16_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int32:
    return propertyTablePropertyCallback<
        int32_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint32:
    return propertyTablePropertyCallback<
        uint32_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int64:
    return propertyTablePropertyCallback<
        int64_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint64:
    return propertyTablePropertyCallback<
        uint64_t,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float32:
    return propertyTablePropertyCallback<float, false, TResult, Callback>(
        property,
        std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float64:
    return propertyTablePropertyCallback<double, false, TResult, Callback>(
        property,
        std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a scalar array type. If the valueType does not have a valid component
 * type, the callback is performed on an invalid PropertyTablePropertyView
 * instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <bool Normalized, typename TResult, typename Callback>
TResult scalarArrayPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.componentType) {
  case CesiumForUnity::CesiumMetadataComponentType::Int8:
    return propertyTablePropertyCallback<
        PropertyArrayView<int8_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint8:
    return propertyTablePropertyCallback<
        PropertyArrayView<uint8_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int16:
    return propertyTablePropertyCallback<
        PropertyArrayView<int16_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint16:
    return propertyTablePropertyCallback<
        PropertyArrayView<uint16_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int32:
    return propertyTablePropertyCallback<
        PropertyArrayView<int32_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint32:
    return propertyTablePropertyCallback<
        PropertyArrayView<uint32_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int64:
    return propertyTablePropertyCallback<
        PropertyArrayView<int64_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint64:
    return propertyTablePropertyCallback<
        PropertyArrayView<uint64_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float32:
    return propertyTablePropertyCallback<
        PropertyArrayView<float>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float64:
    return propertyTablePropertyCallback<
        PropertyArrayView<double>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::vecN type. If the valueType does not have a valid component type,
 * the callback is performed on an invalid PropertyTablePropertyView instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam N The dimensions of the glm::vecN
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <glm::length_t N, bool Normalized, typename TResult, typename Callback>
TResult vecNPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.componentType) {
  case CesiumForUnity::CesiumMetadataComponentType::Int8:
    return propertyTablePropertyCallback<
        glm::vec<N, int8_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint8:
    return propertyTablePropertyCallback<
        glm::vec<N, uint8_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int16:
    return propertyTablePropertyCallback<
        glm::vec<N, int16_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint16:
    return propertyTablePropertyCallback<
        glm::vec<N, uint16_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int32:
    return propertyTablePropertyCallback<
        glm::vec<N, int32_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint32:
    return propertyTablePropertyCallback<
        glm::vec<N, uint32_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int64:
    return propertyTablePropertyCallback<
        glm::vec<N, int64_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint64:
    return propertyTablePropertyCallback<
        glm::vec<N, uint64_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float32:
    return propertyTablePropertyCallback<
        glm::vec<N, float>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float64:
    return propertyTablePropertyCallback<
        glm::vec<N, double>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::vecN type. If the valueType does not have a valid component type,
 * the callback is performed on an invalid PropertyTablePropertyView instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <bool Normalized, typename TResult, typename Callback>
TResult vecNPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  if (valueType.type == CesiumForUnity::CesiumMetadataType::Vec2) {
    return vecNPropertyTablePropertyCallback<2, Normalized, TResult, Callback>(
        property,
        valueType,
        std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Vec3) {
    return vecNPropertyTablePropertyCallback<3, Normalized, TResult, Callback>(
        property,
        valueType,
        std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Vec4) {
    return vecNPropertyTablePropertyCallback<4, Normalized, TResult, Callback>(
        property,
        valueType,
        std::forward<Callback>(callback));
  }

  return callback(PropertyTablePropertyView<uint8_t>());
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::vecN array type. If the valueType does not have a valid component
 * type, the callback is performed on an invalid PropertyTablePropertyView
 * instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam N The dimensions of the glm::vecN
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <glm::length_t N, bool Normalized, typename TResult, typename Callback>
TResult vecNArrayPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.componentType) {
  case CesiumForUnity::CesiumMetadataComponentType::Int8:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, int8_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint8:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, uint8_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int16:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, int16_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint16:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, uint16_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int32:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, int32_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint32:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, uint32_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int64:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, int64_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint64:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, uint64_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float32:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, float>>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float64:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::vec<N, double>>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::vecN array type. If the valueType does not have a valid component
 * type, the callback is performed on an invalid PropertyTablePropertyView
 * instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <bool Normalized, typename TResult, typename Callback>
TResult vecNArrayPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  if (valueType.type == CesiumForUnity::CesiumMetadataType::Vec2) {
    return vecNArrayPropertyTablePropertyCallback<
        2,
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Vec3) {
    return vecNArrayPropertyTablePropertyCallback<
        3,
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Vec4) {
    return vecNArrayPropertyTablePropertyCallback<
        4,
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  }

  return callback(PropertyTablePropertyView<uint8_t>());
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::matN type. If the valueType does not have a valid component type,
 * the callback is performed on an invalid PropertyTablePropertyView instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam N The dimensions of the glm::matN
 * @tparam TNormalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <glm::length_t N, bool Normalized, typename TResult, typename Callback>
TResult matNPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.componentType) {
  case CesiumForUnity::CesiumMetadataComponentType::Int8:
    return propertyTablePropertyCallback<
        glm::mat<N, N, int8_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint8:
    return propertyTablePropertyCallback<
        glm::mat<N, N, uint8_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int16:
    return propertyTablePropertyCallback<
        glm::mat<N, N, int16_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint16:
    return propertyTablePropertyCallback<
        glm::mat<N, N, uint16_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int32:
    return propertyTablePropertyCallback<
        glm::mat<N, N, int32_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint32:
    return propertyTablePropertyCallback<
        glm::mat<N, N, uint32_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int64:
    return propertyTablePropertyCallback<
        glm::mat<N, N, int64_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint64:
    return propertyTablePropertyCallback<
        glm::mat<N, N, uint64_t>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float32:
    return propertyTablePropertyCallback<
        glm::mat<N, N, float>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float64:
    return propertyTablePropertyCallback<
        glm::mat<N, N, double>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::matN type. If the valueType does not have a valid component type,
 * the callback is performed on an invalid PropertyTablePropertyView instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <bool Normalized, typename TResult, typename Callback>
TResult matNPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  if (valueType.type == CesiumForUnity::CesiumMetadataType::Mat2) {
    return matNPropertyTablePropertyCallback<2, Normalized, TResult, Callback>(
        property,
        valueType,
        std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Mat3) {
    return matNPropertyTablePropertyCallback<3, Normalized, TResult, Callback>(
        property,
        valueType,
        std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Mat4) {
    return matNPropertyTablePropertyCallback<4, Normalized, TResult, Callback>(
        property,
        valueType,
        std::forward<Callback>(callback));
  }

  return callback(PropertyTablePropertyView<uint8_t>());
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::matN array type. If the valueType does not have a valid component
 * type, the callback is performed on an invalid PropertyTablePropertyView
 * instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam N The dimensions of the glm::matN
 * @tparam TNormalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <glm::length_t N, bool Normalized, typename TResult, typename Callback>
TResult matNArrayPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.componentType) {
  case CesiumForUnity::CesiumMetadataComponentType::Int8:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, int8_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint8:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, uint8_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int16:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, int16_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint16:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, uint16_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int32:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, int32_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint32:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, uint32_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Int64:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, int64_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Uint64:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, uint64_t>>,
        Normalized,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float32:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, float>>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataComponentType::Float64:
    return propertyTablePropertyCallback<
        PropertyArrayView<glm::mat<N, N, double>>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

/**
 * Callback on a std::any, assuming that it contains a PropertyTablePropertyView
 * on a glm::matN array type. If the valueType does not have a valid component
 * type, the callback is performed on an invalid PropertyTablePropertyView
 * instead.
 *
 * @param property The std::any containing the property.
 * @param valueType The CesiumMetadataValueType of the property.
 * @param callback The callback function.
 *
 * @tparam Normalized Whether the PropertyTablePropertyView is normalized.
 * @tparam TResult The type of the output from the callback function.
 * @tparam Callback The callback function type.
 */
template <bool Normalized, typename TResult, typename Callback>
TResult matNArrayPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  if (valueType.type == CesiumForUnity::CesiumMetadataType::Mat2) {
    return matNArrayPropertyTablePropertyCallback<
        2,
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Mat3) {
    return matNArrayPropertyTablePropertyCallback<
        3,
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  }

  if (valueType.type == CesiumForUnity::CesiumMetadataType::Mat4) {
    return matNArrayPropertyTablePropertyCallback<
        4,
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  }

  return callback(PropertyTablePropertyView<uint8_t>());
}

template <bool Normalized, typename TResult, typename Callback>
TResult arrayPropertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    Callback&& callback) {
  switch (valueType.type) {
  case CesiumForUnity::CesiumMetadataType::Scalar:
    return scalarArrayPropertyTablePropertyCallback<
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::Vec2:
  case CesiumForUnity::CesiumMetadataType::Vec3:
  case CesiumForUnity::CesiumMetadataType::Vec4:
    return vecNArrayPropertyTablePropertyCallback<
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::Mat2:
  case CesiumForUnity::CesiumMetadataType::Mat3:
  case CesiumForUnity::CesiumMetadataType::Mat4:
    return matNArrayPropertyTablePropertyCallback<
        Normalized,
        TResult,
        Callback>(property, valueType, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::Boolean:
    return propertyTablePropertyCallback<
        PropertyArrayView<bool>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::String:
    return propertyTablePropertyCallback<
        PropertyArrayView<std::string_view>,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

template <typename TResult, typename Callback>
TResult propertyTablePropertyCallback(
    const std::any& property,
    const DotNet::CesiumForUnity::CesiumMetadataValueType& valueType,
    bool normalized,
    Callback&& callback) {
  if (valueType.isArray) {
    return normalized
               ? arrayPropertyTablePropertyCallback<true, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback))
               : arrayPropertyTablePropertyCallback<false, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback));
  }

  switch (valueType.type) {
  case CesiumForUnity::CesiumMetadataType::Scalar:
    return normalized
               ? scalarPropertyTablePropertyCallback<true, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback))
               : scalarPropertyTablePropertyCallback<false, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::Vec2:
  case CesiumForUnity::CesiumMetadataType::Vec3:
  case CesiumForUnity::CesiumMetadataType::Vec4:
    return normalized
               ? vecNPropertyTablePropertyCallback<true, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback))
               : vecNPropertyTablePropertyCallback<false, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::Mat2:
  case CesiumForUnity::CesiumMetadataType::Mat3:
  case CesiumForUnity::CesiumMetadataType::Mat4:
    return normalized
               ? matNPropertyTablePropertyCallback<true, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback))
               : matNPropertyTablePropertyCallback<false, TResult, Callback>(
                     property,
                     valueType,
                     std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::Boolean:
    return propertyTablePropertyCallback<bool, false, TResult, Callback>(
        property,
        std::forward<Callback>(callback));
  case CesiumForUnity::CesiumMetadataType::String:
    return propertyTablePropertyCallback<
        std::string_view,
        false,
        TResult,
        Callback>(property, std::forward<Callback>(callback));
  default:
    return callback(PropertyTablePropertyView<uint8_t>());
  }
}

} // namespace

bool CesiumPropertyTablePropertyImpl::GetBoolean(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    bool defaultValue) {
  return propertyTablePropertyCallback<bool>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> bool {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<bool, decltype(value)>::convert(value)
            .value_or(defaultValue);
      });
}

std::int8_t CesiumPropertyTablePropertyImpl::GetSByte(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::int8_t defaultValue) {
  return propertyTablePropertyCallback<std::int8_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::int8_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::int8_t, decltype(value)>::convert(value)
            .value_or(defaultValue);
      });
}

std::uint8_t CesiumPropertyTablePropertyImpl::GetByte(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::uint8_t defaultValue) {
  return propertyTablePropertyCallback<std::uint8_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::uint8_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::uint8_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

std::int16_t CesiumPropertyTablePropertyImpl::GetInt16(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::int16_t defaultValue) {
  return propertyTablePropertyCallback<std::int16_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::int16_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::int16_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

std::uint16_t CesiumPropertyTablePropertyImpl::GetUInt16(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::uint16_t defaultValue) {
  return propertyTablePropertyCallback<std::uint16_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::uint16_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::uint16_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

std::int32_t CesiumPropertyTablePropertyImpl::GetInt32(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::int32_t defaultValue) {
  return propertyTablePropertyCallback<std::int32_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::int32_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::int32_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

std::uint32_t CesiumPropertyTablePropertyImpl::GetUInt32(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::uint32_t defaultValue) {
  return propertyTablePropertyCallback<std::uint32_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::uint32_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::uint32_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

std::int64_t CesiumPropertyTablePropertyImpl::GetInt64(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::int64_t defaultValue) {
  return propertyTablePropertyCallback<std::int64_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::int64_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::int64_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

std::uint64_t CesiumPropertyTablePropertyImpl::GetUInt64(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    std::uint64_t defaultValue) {
  return propertyTablePropertyCallback<std::uint64_t>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> std::uint64_t {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<std::uint64_t, decltype(value)>::convert(
                   value)
            .value_or(defaultValue);
      });
}

float CesiumPropertyTablePropertyImpl::GetFloat(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    float defaultValue) {
  return propertyTablePropertyCallback<float>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> float {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<float, decltype(value)>::convert(value)
            .value_or(defaultValue);
      });
}

double CesiumPropertyTablePropertyImpl::GetDouble(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    double defaultValue) {
  return propertyTablePropertyCallback<double>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, defaultValue](const auto& v) -> double {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        return MetadataConversions<double, decltype(value)>::convert(value)
            .value_or(defaultValue);
      });
}

DotNet::Unity::Mathematics::int2 CesiumPropertyTablePropertyImpl::GetInt2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::int2 defaultValue) {
  std::optional<glm::ivec2> maybeVec2 =
      propertyTablePropertyCallback<std::optional<glm::ivec2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::ivec2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::ivec2, decltype(value)>::convert(
                value);
          });

  return maybeVec2 ? UnityMetadataConversions::toInt2(*maybeVec2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::uint2 CesiumPropertyTablePropertyImpl::GetUInt2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::uint2 defaultValue) {
  std::optional<glm::uvec2> maybeVec2 =
      propertyTablePropertyCallback<std::optional<glm::uvec2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::uvec2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::uvec2, decltype(value)>::convert(
                value);
          });

  return maybeVec2 ? UnityMetadataConversions::toUint2(*maybeVec2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::float2 CesiumPropertyTablePropertyImpl::GetFloat2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::float2 defaultValue) {
  std::optional<glm::vec2> maybeVec2 =
      propertyTablePropertyCallback<std::optional<glm::vec2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::vec2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::vec2, decltype(value)>::convert(
                value);
          });

  return maybeVec2 ? UnityMetadataConversions::toFloat2(*maybeVec2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::double2 CesiumPropertyTablePropertyImpl::GetDouble2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::double2 defaultValue) {
  std::optional<glm::dvec2> maybeVec2 =
      propertyTablePropertyCallback<std::optional<glm::dvec2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::dvec2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::dvec2, decltype(value)>::convert(
                value);
          });

  return maybeVec2 ? UnityMetadataConversions::toDouble2(*maybeVec2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::int3 CesiumPropertyTablePropertyImpl::GetInt3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::int3 defaultValue) {
  std::optional<glm::ivec3> maybeVec3 =
      propertyTablePropertyCallback<std::optional<glm::ivec3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::ivec3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::ivec3, decltype(value)>::convert(
                value);
          });

  return maybeVec3 ? UnityMetadataConversions::toInt3(*maybeVec3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::uint3 CesiumPropertyTablePropertyImpl::GetUInt3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::uint3 defaultValue) {
  std::optional<glm::uvec3> maybeVec3 =
      propertyTablePropertyCallback<std::optional<glm::uvec3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::uvec3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::uvec3, decltype(value)>::convert(
                value);
          });

  return maybeVec3 ? UnityMetadataConversions::toUint3(*maybeVec3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::float3 CesiumPropertyTablePropertyImpl::GetFloat3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::float3 defaultValue) {
  std::optional<glm::vec3> maybeVec3 =
      propertyTablePropertyCallback<std::optional<glm::vec3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::vec3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::vec3, decltype(value)>::convert(
                value);
          });

  return maybeVec3 ? UnityMetadataConversions::toFloat3(*maybeVec3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::double3 CesiumPropertyTablePropertyImpl::GetDouble3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::double3 defaultValue) {
  std::optional<glm::dvec3> maybeVec3 =
      propertyTablePropertyCallback<std::optional<glm::dvec3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::dvec3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::dvec3, decltype(value)>::convert(
                value);
          });

  return maybeVec3 ? UnityMetadataConversions::toDouble3(*maybeVec3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::int4 CesiumPropertyTablePropertyImpl::GetInt4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::int4 defaultValue) {
  std::optional<glm::ivec4> maybeVec4 =
      propertyTablePropertyCallback<std::optional<glm::ivec4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::ivec4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::ivec4, decltype(value)>::convert(
                value);
          });

  return maybeVec4 ? UnityMetadataConversions::toInt4(*maybeVec4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::uint4 CesiumPropertyTablePropertyImpl::GetUInt4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::uint4 defaultValue) {
  std::optional<glm::uvec4> maybeVec4 =
      propertyTablePropertyCallback<std::optional<glm::uvec4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::uvec4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::uvec4, decltype(value)>::convert(
                value);
          });

  return maybeVec4 ? UnityMetadataConversions::toUint4(*maybeVec4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::float4 CesiumPropertyTablePropertyImpl::GetFloat4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::float4 defaultValue) {
  std::optional<glm::vec4> maybeVec4 =
      propertyTablePropertyCallback<std::optional<glm::vec4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::vec4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::vec4, decltype(value)>::convert(
                value);
          });

  return maybeVec4 ? UnityMetadataConversions::toFloat4(*maybeVec4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::double4 CesiumPropertyTablePropertyImpl::GetDouble4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::double4 defaultValue) {
  std::optional<glm::dvec4> maybeVec4 =
      propertyTablePropertyCallback<std::optional<glm::dvec4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::dvec4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::dvec4, decltype(value)>::convert(
                value);
          });

  return maybeVec4 ? UnityMetadataConversions::toDouble4(*maybeVec4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::int2x2 CesiumPropertyTablePropertyImpl::GetInt2x2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::int2x2 defaultValue) {
  std::optional<glm::imat2x2> maybeMat2 =
      propertyTablePropertyCallback<std::optional<glm::imat2x2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::imat2x2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::imat2x2, decltype(value)>::convert(
                value);
          });

  return maybeMat2 ? UnityMetadataConversions::toInt2x2(*maybeMat2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::uint2x2 CesiumPropertyTablePropertyImpl::GetUInt2x2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::uint2x2 defaultValue) {
  std::optional<glm::umat2x2> maybeMat2 =
      propertyTablePropertyCallback<std::optional<glm::umat2x2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::umat2x2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::umat2x2, decltype(value)>::convert(
                value);
          });
  return maybeMat2 ? UnityMetadataConversions::toUint2x2(*maybeMat2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::float2x2
CesiumPropertyTablePropertyImpl::GetFloat2x2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::float2x2 defaultValue) {
  std::optional<glm::mat2> maybeMat2 =
      propertyTablePropertyCallback<std::optional<glm::mat2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::mat2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::mat2, decltype(value)>::convert(
                value);
          });

  return maybeMat2 ? UnityMetadataConversions::toFloat2x2(*maybeMat2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::double2x2
CesiumPropertyTablePropertyImpl::GetDouble2x2(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::double2x2 defaultValue) {
  std::optional<glm::dmat2> maybeMat2 =
      propertyTablePropertyCallback<std::optional<glm::dmat2>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::dmat2> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::dmat2, decltype(value)>::convert(
                value);
          });

  return maybeMat2 ? UnityMetadataConversions::toDouble2x2(*maybeMat2)
                   : defaultValue;
}

DotNet::Unity::Mathematics::int3x3 CesiumPropertyTablePropertyImpl::GetInt3x3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::int3x3 defaultValue) {
  std::optional<glm::imat3x3> maybeMat3 =
      propertyTablePropertyCallback<std::optional<glm::imat3x3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::imat3x3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::imat3x3, decltype(value)>::convert(
                value);
          });

  return maybeMat3 ? UnityMetadataConversions::toInt3x3(*maybeMat3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::uint3x3 CesiumPropertyTablePropertyImpl::GetUInt3x3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::uint3x3 defaultValue) {
  std::optional<glm::umat3x3> maybeMat3 =
      propertyTablePropertyCallback<std::optional<glm::umat3x3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::umat3x3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::umat3x3, decltype(value)>::convert(
                value);
          });

  return maybeMat3 ? UnityMetadataConversions::toUint3x3(*maybeMat3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::float3x3
CesiumPropertyTablePropertyImpl::GetFloat3x3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::float3x3 defaultValue) {
  std::optional<glm::mat3> maybeMat3 =
      propertyTablePropertyCallback<std::optional<glm::mat3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::mat3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::mat3, decltype(value)>::convert(
                value);
          });

  return maybeMat3 ? UnityMetadataConversions::toFloat3x3(*maybeMat3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::double3x3
CesiumPropertyTablePropertyImpl::GetDouble3x3(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::double3x3 defaultValue) {
  std::optional<glm::dmat3> maybeMat3 =
      propertyTablePropertyCallback<std::optional<glm::dmat3>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::dmat3> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::dmat3, decltype(value)>::convert(
                value);
          });

  return maybeMat3 ? UnityMetadataConversions::toDouble3x3(*maybeMat3)
                   : defaultValue;
}

DotNet::Unity::Mathematics::int4x4 CesiumPropertyTablePropertyImpl::GetInt4x4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::int4x4 defaultValue) {
  std::optional<glm::imat4x4> maybeMat4 =
      propertyTablePropertyCallback<std::optional<glm::imat4x4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::imat4x4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::imat4x4, decltype(value)>::convert(
                value);
          });

  return maybeMat4 ? UnityMetadataConversions::toInt4x4(*maybeMat4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::uint4x4 CesiumPropertyTablePropertyImpl::GetUInt4x4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::uint4x4 defaultValue) {
  std::optional<glm::umat4x4> maybeMat4 =
      propertyTablePropertyCallback<std::optional<glm::umat4x4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::umat4x4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::umat4x4, decltype(value)>::convert(
                value);
          });

  return maybeMat4 ? UnityMetadataConversions::toUint4x4(*maybeMat4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::float4x4
CesiumPropertyTablePropertyImpl::GetFloat4x4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::float4x4 defaultValue) {
  std::optional<glm::mat4> maybeMat4 =
      propertyTablePropertyCallback<std::optional<glm::mat4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::mat4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::mat4, decltype(value)>::convert(
                value);
          });

  return maybeMat4 ? UnityMetadataConversions::toFloat4x4(*maybeMat4)
                   : defaultValue;
}

DotNet::Unity::Mathematics::double4x4
CesiumPropertyTablePropertyImpl::GetDouble4x4(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    DotNet::Unity::Mathematics::double4x4 defaultValue) {
  std::optional<glm::dmat4> maybeMat4 =
      propertyTablePropertyCallback<std::optional<glm::dmat4>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<glm::dmat4> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<glm::dmat4, decltype(value)>::convert(
                value);
          });

  return maybeMat4 ? UnityMetadataConversions::toDouble4x4(*maybeMat4)
                   : defaultValue;
}

System::String CesiumPropertyTablePropertyImpl::GetString(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    const DotNet::System::String& defaultValue) {
  std::optional<std::string> maybeString =
      propertyTablePropertyCallback<std::optional<std::string>>(
          this->_property,
          property.valueType(),
          property.isNormalized(),
          [featureID](const auto& v) -> std::optional<std::string> {
            // size() returns zero if the view is invalid.
            if (featureID < 0 || featureID >= v.size()) {
              return std::nullopt;
            }

            auto maybeValue = v.get(featureID);
            if (!maybeValue) {
              return std::nullopt;
            }

            auto value = *maybeValue;
            return MetadataConversions<std::string, decltype(value)>::convert(
                value);
          });

  return maybeString ? System::String(*maybeString) : defaultValue;
}

DotNet::CesiumForUnity::CesiumPropertyArray
CesiumPropertyTablePropertyImpl::GetArray(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID) {
  return propertyTablePropertyCallback<
      DotNet::CesiumForUnity::CesiumPropertyArray>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID](
          const auto& v) -> DotNet::CesiumForUnity::CesiumPropertyArray {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return DotNet::CesiumForUnity::CesiumPropertyArray();
        }

        auto maybeValue = v.get(featureID);
        if (maybeValue) {
          auto value = *maybeValue;
          if constexpr (CesiumGltf::IsMetadataArray<decltype(value)>::value) {
            return CesiumFeaturesMetadataUtility::makePropertyArray(value);
          }
        }

        return DotNet::CesiumForUnity::CesiumPropertyArray();
      });
}

DotNet::CesiumForUnity::CesiumMetadataValue
CesiumPropertyTablePropertyImpl::GetValue(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID) {
  return propertyTablePropertyCallback<CesiumForUnity::CesiumMetadataValue>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID](const auto& v) -> CesiumForUnity::CesiumMetadataValue {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return CesiumForUnity::CesiumMetadataValue();
        }

        auto maybeValue = v.get(featureID);
        return maybeValue ? CesiumFeaturesMetadataUtility::makeMetadataValue(
                                *maybeValue)
                          : CesiumForUnity::CesiumMetadataValue();
      });
}

DotNet::CesiumForUnity::CesiumMetadataValue
CesiumPropertyTablePropertyImpl::GetRawValue(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID) {
  return propertyTablePropertyCallback<CesiumForUnity::CesiumMetadataValue>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID](const auto& v) -> CesiumForUnity::CesiumMetadataValue {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return CesiumForUnity::CesiumMetadataValue();
        }

        return CesiumFeaturesMetadataUtility::makeMetadataValue(
            v.getRaw(featureID));
      });
}

} // namespace CesiumForUnityNative
