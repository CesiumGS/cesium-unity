#pragma once

namespace CesiumForUnityNative {
class CesiumPropertyTablePropertyImpl;
}

//#include "CesiumFeaturesMetadataUtility.h"

#include <CesiumGltf/PropertyTablePropertyView.h>

#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>

#include <any>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumPropertyTableProperty;
}

namespace DotNet::System {
class String;
}

namespace CesiumForUnityNative {

class CesiumPropertyTablePropertyImpl {
public:
  ~CesiumPropertyTablePropertyImpl(){};
  CesiumPropertyTablePropertyImpl(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property){};
  void JustBeforeDelete(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property){};

  template <typename T, bool Normalized>
  static DotNet::CesiumForUnity::CesiumPropertyTableProperty CreateProperty(
      const CesiumGltf::PropertyTablePropertyView<T, Normalized>& propertyView);

  bool GetBoolean(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      bool defaultValue);

  std::int8_t GetSByte(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::int8_t defaultValue);

  std::uint8_t GetByte(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::uint8_t defaultValue);

  std::int16_t GetInt16(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::int16_t defaultValue);

  std::uint16_t GetUInt16(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::uint16_t defaultValue);

  std::int32_t GetInt32(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::int32_t defaultValue);

  std::uint32_t GetUInt32(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::uint32_t defaultValue);

  std::int64_t GetInt64(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::int64_t defaultValue);

  std::uint64_t GetUInt64(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      std::uint64_t defaultValue);

  float GetFloat(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      float defaultValue);

  double GetDouble(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      double defaultValue);

  DotNet::System::String GetString(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      const DotNet::System::String& defaultValue);

  // std::int8_t GetComponentInt8(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::int8_t defaultValue);
  // std::uint8_t GetComponentUInt8(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::uint8_t defaultValue);
  // std::int16_t GetComponentInt16(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::int16_t defaultValue);
  // std::uint16_t GetComponentUInt16(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::uint16_t defaultValue);
  // std::int32_t GetComponentInt32(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::int32_t defaultValue);
  // std::uint32_t GetComponentUInt32(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::uint32_t defaultValue);
  // std::int64_t GetComponentInt64(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::int64_t defaultValue);
  // std::uint64_t GetComponentUInt64(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    std::uint64_t defaultValue);
  // float GetComponentFloat32(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    float defaultValue);
  // double GetComponentFloat64(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    double defaultValue);
  // bool GetComponentBoolean(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    bool defaultValue);
  // DotNet::System::String GetComponentString(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property,
  //    int index,
  //    const DotNet::System::String& defaultValue);

  // int GetComponentCount(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property);

  // DotNet::CesiumForUnity::MetadataType GetComponentType(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property);

  // DotNet::CesiumForUnity::MetadataType GetMetadataType(
  //    const DotNet::CesiumForUnity::CesiumFeature& feature,
  //    const DotNet::System::String& property);

private:
  std::any _property;
};

template <typename T, bool Normalized>
/*static*/ DotNet::CesiumForUnity::CesiumPropertyTableProperty
CesiumPropertyTablePropertyImpl::CreateProperty(
    const CesiumGltf::PropertyTablePropertyView<T, Normalized>& propertyView) {
  DotNet::CesiumForUnity::CesiumPropertyTableProperty property =
      DotNet::CesiumForUnity::CesiumPropertyTableProperty();
  property.size(propertyView.size());
  property.arraySize(propertyView.arrayCount());
  property.valueType(
      CesiumFeaturesMetadataUtility::TypeToMetadataValueType<T>());
  property.isNormalized(Normalized);

  CesiumPropertyTablePropertyImpl& propertyImpl =
      property.NativeImplementation();
  propertyImpl._property = propertyView;

  return property;
}
} // namespace CesiumForUnityNative
