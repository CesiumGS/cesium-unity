#pragma once

#include "UnityMetadataConversions.h"

#include <CesiumGltf/PropertyTablePropertyView.h>
#include <CesiumGltf/PropertyTypeTraits.h>

#include <DotNet/CesiumForUnity/CesiumIntVec2.h>
#include <DotNet/CesiumForUnity/CesiumIntVec3.h>
#include <DotNet/CesiumForUnity/CesiumIntVec4.h>
#include <DotNet/CesiumForUnity/CesiumMetadataValue.h>
#include <DotNet/CesiumForUnity/CesiumMetadataValueType.h>
#include <DotNet/CesiumForUnity/CesiumPropertyArray.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>
#include <DotNet/CesiumForUnity/CesiumPropertyTablePropertyStatus.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/String.h>

namespace DotNet::CesiumForUnity {
class CesiumPrimitiveFeatures;
class CesiumModelMetadata;
} // namespace DotNet::CesiumForUnity

namespace DotNet::UnityEngine {
class GameObject;
}

namespace CesiumGltf {
struct Model;
struct MeshPrimitive;
struct ExtensionExtMeshFeatures;
struct ExtensionModelExtStructuralMetadata;
} // namespace CesiumGltf

namespace CesiumForUnityNative {
class CesiumFeaturesMetadataUtility {
public:
  template <typename T>
  static DotNet::CesiumForUnity::CesiumMetadataValueType
  typeToMetadataValueType() noexcept;

  static DotNet::CesiumForUnity::CesiumPrimitiveFeatures addPrimitiveFeatures(
      const DotNet::UnityEngine::GameObject& primitiveGameObject,
      const CesiumGltf::Model& model,
      const CesiumGltf::MeshPrimitive& primitive,
      const CesiumGltf::ExtensionExtMeshFeatures& extension) noexcept;

  static DotNet::CesiumForUnity::CesiumModelMetadata addModelMetadata(
      const DotNet::UnityEngine::GameObject& modelGameObject,
      const CesiumGltf::Model& model,
      const CesiumGltf::ExtensionModelExtStructuralMetadata&
          extension) noexcept;

  template <typename T>
  static DotNet::CesiumForUnity::CesiumMetadataValue
  makeMetadataValue(const T& nativeValue) noexcept;

  template <typename T>
  static DotNet::CesiumForUnity::CesiumPropertyArray
  makePropertyArray(const CesiumGltf::PropertyArrayView<T>& arrayView);

  template <typename T>
  static DotNet::CesiumForUnity::CesiumPropertyArray
  makePropertyArray(const CesiumGltf::PropertyArrayCopy<T>& arrayCopy);

  template <typename T, bool Normalized>
  static DotNet::CesiumForUnity::CesiumPropertyTableProperty
  makePropertyTableProperty(
      const CesiumGltf::PropertyTablePropertyView<T, Normalized>& propertyView);
};

template <typename T>
DotNet::CesiumForUnity::CesiumMetadataValueType
CesiumFeaturesMetadataUtility::typeToMetadataValueType() noexcept {
  DotNet::CesiumForUnity::CesiumMetadataType type;
  DotNet::CesiumForUnity::CesiumMetadataComponentType componentType;
  bool isArray;

  if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
    using ArrayType = typename CesiumGltf::MetadataArrayType<T>::type;
    type = DotNet::CesiumForUnity::CesiumMetadataType(
        CesiumGltf::TypeToPropertyType<ArrayType>::value);
    componentType = DotNet::CesiumForUnity::CesiumMetadataComponentType(
        CesiumGltf::TypeToPropertyType<ArrayType>::component);
    isArray = true;
  } else {
    type = DotNet::CesiumForUnity::CesiumMetadataType(
        CesiumGltf::TypeToPropertyType<T>::value);
    componentType = DotNet::CesiumForUnity::CesiumMetadataComponentType(
        CesiumGltf::TypeToPropertyType<T>::component);
    isArray = false;
  }

  return DotNet::CesiumForUnity::CesiumMetadataValueType::Construct(
      type,
      componentType,
      isArray);
}

template <typename T>
DotNet::CesiumForUnity::CesiumMetadataValue
CesiumFeaturesMetadataUtility::makeMetadataValue(
    const T& nativeValue) noexcept {
  DotNet::CesiumForUnity::CesiumMetadataValue value;
  if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
    value.SetObjectValue(
        CesiumFeaturesMetadataUtility::makePropertyArray(nativeValue));
  }

  if constexpr (CesiumGltf::IsMetadataVecN<T>::value) {
    using componentType = typename T::value_type;
    constexpr glm::length_t length = T::length();

    if constexpr (length == 2) {
      // Integer vecN types should be stored as special CesiumIntVecN or
      // CesiumUintVecN structs to preserve their precise type.
      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_unsigned_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumUintVec2<componentType>(
                nativeValue));
      }

      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_signed_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumIntVec2<componentType>(
                nativeValue));
      }

      if constexpr (std::is_same_v<componentType, float>) {
        value.SetObjectValue(UnityMetadataConversions::toFloat2(nativeValue));
      }

      if constexpr (std::is_same_v<componentType, double>) {
        value.SetObjectValue(UnityMetadataConversions::toDouble2(nativeValue));
      }
    }

    if constexpr (length == 3) {
      // Integer vecN types should be stored as special CesiumIntVecN or
      // CesiumUintVecN structs to preserve their precise type.
      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_unsigned_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumUintVec3<componentType>(
                nativeValue));
      }

      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_signed_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumIntVec3<componentType>(
                nativeValue));
      }

      if constexpr (std::is_same_v<componentType, float>) {
        value.SetObjectValue(UnityMetadataConversions::toFloat3(nativeValue));
      }

      if constexpr (std::is_same_v<componentType, double>) {
        value.SetObjectValue(UnityMetadataConversions::toDouble3(nativeValue));
      }
    }

    if constexpr (length == 4) {
      // Integer vecN types should be stored as special CesiumIntVecN or
      // CesiumUintVecN structs to preserve their precise type.
      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_unsigned_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumUintVec4<componentType>(
                nativeValue));
      }

      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_signed_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumIntVec4<componentType>(
                nativeValue));
      }

      if constexpr (std::is_same_v<componentType, float>) {
        value.SetObjectValue(UnityMetadataConversions::toFloat4(nativeValue));
      }

      if constexpr (std::is_same_v<componentType, double>) {
        value.SetObjectValue(UnityMetadataConversions::toDouble4(nativeValue));
      }
    }
  }

  if constexpr (CesiumGltf::IsMetadataMatN<T>::value) {
    using componentType = typename T::value_type;
    constexpr glm::length_t length = T::length();

    if constexpr (length == 2) {
      // Integer matN types should be stored as special CesiumIntMatN or
      // CesiumUintMatN structs to preserve their precise type.
      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_unsigned_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumUintMat2x2<componentType>(
                nativeValue));
      }

      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_signed_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumIntMat2x2<componentType>(
                nativeValue));
      }

      if constexpr (std::is_same_v<componentType, float>) {
        value.SetObjectValue(UnityMetadataConversions::toFloat2x2(nativeValue));
      }

      if constexpr (std::is_same_v<componentType, double>) {
        value.SetObjectValue(
            UnityMetadataConversions::toDouble2x2(nativeValue));
      }
    }

    if constexpr (length == 3) {
      // Integer vecN types should be stored as special CesiumIntVecN or
      // CesiumUintVecN structs to preserve their precise type.
      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_unsigned_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumUintMat3x3<componentType>(
                nativeValue));
      }

      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_signed_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumIntMat3x3<componentType>(
                nativeValue));
      }

      if constexpr (std::is_same_v<componentType, float>) {
        value.SetObjectValue(UnityMetadataConversions::toFloat3x3(nativeValue));
      }

      if constexpr (std::is_same_v<componentType, double>) {
        value.SetObjectValue(
            UnityMetadataConversions::toDouble3x3(nativeValue));
      }
    }

    if constexpr (length == 4) {
      // Integer vecN types should be stored as special CesiumIntVecN or
      // CesiumUintVecN structs to preserve their precise type.
      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_unsigned_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumUintMat4x4<componentType>(
                nativeValue));
      }

      if constexpr (
          CesiumGltf::IsMetadataInteger<componentType>::value &&
          std::is_signed_v<componentType>) {
        value.SetObjectValue(
            UnityMetadataConversions::toCesiumIntMat4x4<componentType>(
                nativeValue));
      }

      if constexpr (std::is_same_v<componentType, float>) {
        value.SetObjectValue(UnityMetadataConversions::toFloat4x4(nativeValue));
      }

      if constexpr (std::is_same_v<componentType, double>) {
        value.SetObjectValue(
            UnityMetadataConversions::toDouble4x4(nativeValue));
      }
    }
  }

  if constexpr (CesiumGltf::IsMetadataString<T>::value) {
    value.SetObjectValue(DotNet::System::String(
        std::string(nativeValue.data(), nativeValue.size())));
  }

  if constexpr (
      CesiumGltf::IsMetadataBoolean<T>::value ||
      CesiumGltf::IsMetadataScalar<T>::value) {
    value.SetObjectValue(nativeValue);
  }

  return value;
}

template <typename T>
DotNet::CesiumForUnity::CesiumPropertyArray
CesiumFeaturesMetadataUtility::makePropertyArray(
    const CesiumGltf::PropertyArrayView<T>& arrayView) {
  DotNet::CesiumForUnity::CesiumPropertyArray array =
      DotNet::CesiumForUnity::CesiumPropertyArray();
  DotNet::CesiumForUnity::CesiumMetadataValueType valueType =
      CesiumFeaturesMetadataUtility::typeToMetadataValueType<T>();
  array.elementValueType({valueType.type, valueType.componentType, false});

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumMetadataValue> values(
      arrayView.size());
  for (int64_t i = 0; i < arrayView.size(); i++) {
    values.Item(
        i,
        CesiumFeaturesMetadataUtility::makeMetadataValue(arrayView[i]));
  }

  array.values(values);
  return array;
}

template <typename T>
DotNet::CesiumForUnity::CesiumPropertyArray
CesiumFeaturesMetadataUtility::makePropertyArray(
    const CesiumGltf::PropertyArrayCopy<T>& arrayCopy) {
  return makePropertyArray(arrayCopy.view());
}

template <typename T, bool Normalized>
DotNet::CesiumForUnity::CesiumPropertyTableProperty
CesiumFeaturesMetadataUtility::makePropertyTableProperty(
    const CesiumGltf::PropertyTablePropertyView<T, Normalized>& propertyView) {
  DotNet::CesiumForUnity::CesiumPropertyTableProperty property =
      DotNet::CesiumForUnity::CesiumPropertyTableProperty();
  switch (propertyView.status()) {
  case CesiumGltf::PropertyTablePropertyViewStatus::Valid:
    property.status(
        DotNet::CesiumForUnity::CesiumPropertyTablePropertyStatus::Valid);
    break;
  case CesiumGltf::PropertyTablePropertyViewStatus::EmptyPropertyWithDefault:
    property.status(DotNet::CesiumForUnity::CesiumPropertyTablePropertyStatus::
                        EmptyPropertyWithDefault);
    break;
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidPropertyTable:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorNonexistentProperty:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorTypeMismatch:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorComponentTypeMismatch:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorArrayTypeMismatch:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidNormalization:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorNormalizationMismatch:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidOffset:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidScale:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidMax:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidMin:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidNoDataValue:
  case CesiumGltf::PropertyTablePropertyViewStatus::ErrorInvalidDefaultValue:
    // The status was already set in the C# default constructor.
    return property;
  default:
    property.status(DotNet::CesiumForUnity::CesiumPropertyTablePropertyStatus::
                        ErrorInvalidPropertyData);
    return property;
  }

  property.size(propertyView.size());
  property.arraySize(propertyView.arrayCount());
  property.valueType(
      CesiumFeaturesMetadataUtility::typeToMetadataValueType<T>());
  property.isNormalized(Normalized);
  if (propertyView.offset()) {
    property.offset(CesiumFeaturesMetadataUtility::makeMetadataValue(
        *propertyView.offset()));
  }

  if (propertyView.scale()) {
    property.scale(CesiumFeaturesMetadataUtility::makeMetadataValue(
        *propertyView.scale()));
  }

  if (propertyView.min()) {
    property.min(
        CesiumFeaturesMetadataUtility::makeMetadataValue(*propertyView.min()));
  }

  if (propertyView.max()) {
    property.max(
        CesiumFeaturesMetadataUtility::makeMetadataValue(*propertyView.max()));
  }

  if (propertyView.noData()) {
    property.noData(CesiumFeaturesMetadataUtility::makeMetadataValue(
        *propertyView.noData()));
  }

  if (propertyView.defaultValue()) {
    property.defaultValue(CesiumFeaturesMetadataUtility::makeMetadataValue(
        *propertyView.defaultValue()));
  }

  CesiumPropertyTablePropertyImpl& propertyImpl =
      property.NativeImplementation();
  propertyImpl._property = propertyView;

  return property;
}

} // namespace CesiumForUnityNative
