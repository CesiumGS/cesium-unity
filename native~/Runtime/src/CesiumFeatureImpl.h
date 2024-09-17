#pragma once

#include "CesiumImpl.h"

#include <CesiumGltf/PropertyTablePropertyView.h>

#include <DotNet/CesiumForUnity/CesiumMetadataValue.h>
#include <DotNet/CesiumForUnity/MetadataType.h>
#include <swl/variant.hpp>

#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumFeature;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
}

namespace CesiumForUnityNative {

using ValueType = swl::variant<
    int8_t,
    uint8_t,
    int16_t,
    uint16_t,
    int32_t,
    uint32_t,
    int64_t,
    uint64_t,
    float,
    double,
    bool,
    std::string_view,
    CesiumGltf::PropertyArrayView<int8_t>,
    CesiumGltf::PropertyArrayView<uint8_t>,
    CesiumGltf::PropertyArrayView<int16_t>,
    CesiumGltf::PropertyArrayView<uint16_t>,
    CesiumGltf::PropertyArrayView<int32_t>,
    CesiumGltf::PropertyArrayView<uint32_t>,
    CesiumGltf::PropertyArrayView<int64_t>,
    CesiumGltf::PropertyArrayView<uint64_t>,
    CesiumGltf::PropertyArrayView<float>,
    CesiumGltf::PropertyArrayView<double>,
    CesiumGltf::PropertyArrayView<bool>,
    CesiumGltf::PropertyArrayView<std::string_view>>;

class CesiumFeatureImpl : public CesiumImpl<CesiumFeatureImpl> {
public:
  CesiumFeatureImpl(const DotNet::CesiumForUnity::CesiumFeature& feature);
  ~CesiumFeatureImpl();

  std::int8_t GetInt8(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::int8_t defaultValue);
  std::uint8_t GetUInt8(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::uint8_t defaultValue);
  std::int16_t GetInt16(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::int16_t defaultValue);
  std::uint16_t GetUInt16(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::uint16_t defaultValue);
  std::int32_t GetInt32(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::int32_t defaultValue);
  std::uint32_t GetUInt32(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::uint32_t defaultValue);
  std::int64_t GetInt64(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::int64_t defaultValue);
  std::uint64_t GetUInt64(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      std::uint64_t defaultValue);
  float GetFloat32(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      float defaultValue);
  double GetFloat64(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      double defaultValue);
  bool GetBoolean(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      bool defaultValue);
  DotNet::System::String GetString(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      const DotNet::System::String& defaultValue);

  std::int8_t GetComponentInt8(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::int8_t defaultValue);
  std::uint8_t GetComponentUInt8(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::uint8_t defaultValue);
  std::int16_t GetComponentInt16(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::int16_t defaultValue);
  std::uint16_t GetComponentUInt16(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::uint16_t defaultValue);
  std::int32_t GetComponentInt32(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::int32_t defaultValue);
  std::uint32_t GetComponentUInt32(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::uint32_t defaultValue);
  std::int64_t GetComponentInt64(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::int64_t defaultValue);
  std::uint64_t GetComponentUInt64(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      std::uint64_t defaultValue);
  float GetComponentFloat32(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      float defaultValue);
  double GetComponentFloat64(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      double defaultValue);
  bool GetComponentBoolean(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      bool defaultValue);
  DotNet::System::String GetComponentString(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property,
      int index,
      const DotNet::System::String& defaultValue);

  int GetComponentCount(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property);

  DotNet::CesiumForUnity::MetadataType GetComponentType(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property);

  DotNet::CesiumForUnity::MetadataType GetMetadataType(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property);

  bool IsNormalized(
      const DotNet::CesiumForUnity::CesiumFeature& feature,
      const DotNet::System::String& property);

  struct PropertyInfo {
    int64_t count;
    bool isNormalized;
  };

  std::unordered_map<
      std::string,
      std::pair<PropertyInfo, DotNet::CesiumForUnity::CesiumMetadataValue>>
      values;

private:
  PropertyInfo getPropertyInfo(const DotNet::System::String& property);
  DotNet::CesiumForUnity::CesiumMetadataValue
  getValue(const DotNet::System::String& property);
};
} // namespace CesiumForUnityNative
