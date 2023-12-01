#pragma once

#include "CesiumPropertyTablePropertyImpl.h"

#include <CesiumGltf/MetadataConversions.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Mathematics/double2.h>
#include <DotNet/Unity/Mathematics/float2.h>
#include <DotNet/Unity/Mathematics/int2.h>
#include <DotNet/Unity/Mathematics/uint2.h>

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

  return maybeVec2 ? int2{(*maybeVec2)[0], (*maybeVec2)[1]} : defaultValue;
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

  return maybeVec2 ? uint2{(*maybeVec2)[0], (*maybeVec2)[1]} : defaultValue;
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

  return maybeVec2 ? float2{(*maybeVec2)[0], (*maybeVec2)[1]} : defaultValue;
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

  return maybeVec2 ? double2{(*maybeVec2)[0], (*maybeVec2)[1]} : defaultValue;
}

System::String CesiumPropertyTablePropertyImpl::GetString(
    const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
    std::int64_t featureID,
    const DotNet::System::String& defaultValue) {
  return propertyTablePropertyCallback<System::String>(
      this->_property,
      property.valueType(),
      property.isNormalized(),
      [featureID, &defaultValue](const auto& v) -> System::String {
        // size() returns zero if the view is invalid.
        if (featureID < 0 || featureID >= v.size()) {
          return defaultValue;
        }

        auto maybeValue = v.get(featureID);
        if (!maybeValue) {
          return defaultValue;
        }

        auto value = *maybeValue;
        auto maybeConvertedValue =
            MetadataConversions<std::string, decltype(value)>::convert(value);

        return maybeConvertedValue ? System::String(*maybeConvertedValue)
                                   : defaultValue;
      });
}
} // namespace CesiumForUnityNative
