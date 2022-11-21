#pragma once

#include <CesiumGltf/MetadataPropertyView.h>

#include <DotNet/CesiumForUnity/MetadataType.h>

#include <variant>

namespace DotNet::CesiumForUnity {
class MetadataProperty;
}

namespace DotNet::System {
class String;
}

namespace CesiumForUnityNative {

using ValueType = std::variant<
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
    CesiumGltf::MetadataArrayView<int8_t>,
    CesiumGltf::MetadataArrayView<uint8_t>,
    CesiumGltf::MetadataArrayView<int16_t>,
    CesiumGltf::MetadataArrayView<uint16_t>,
    CesiumGltf::MetadataArrayView<int32_t>,
    CesiumGltf::MetadataArrayView<uint32_t>,
    CesiumGltf::MetadataArrayView<int64_t>,
    CesiumGltf::MetadataArrayView<uint64_t>,
    CesiumGltf::MetadataArrayView<float>,
    CesiumGltf::MetadataArrayView<double>,
    CesiumGltf::MetadataArrayView<bool>,
    CesiumGltf::MetadataArrayView<std::string_view>>;

using PropertyType = std::variant<
    CesiumGltf::MetadataPropertyView<int8_t>,
    CesiumGltf::MetadataPropertyView<uint8_t>,
    CesiumGltf::MetadataPropertyView<int16_t>,
    CesiumGltf::MetadataPropertyView<uint16_t>,
    CesiumGltf::MetadataPropertyView<int32_t>,
    CesiumGltf::MetadataPropertyView<uint32_t>,
    CesiumGltf::MetadataPropertyView<int64_t>,
    CesiumGltf::MetadataPropertyView<uint64_t>,
    CesiumGltf::MetadataPropertyView<float>,
    CesiumGltf::MetadataPropertyView<double>,
    CesiumGltf::MetadataPropertyView<bool>,
    CesiumGltf::MetadataPropertyView<std::string_view>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int8_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint8_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int16_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint16_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int32_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint32_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<int64_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<uint64_t>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<float>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<double>>,
    CesiumGltf::MetadataPropertyView<CesiumGltf::MetadataArrayView<bool>>,
    CesiumGltf::MetadataPropertyView<
        CesiumGltf::MetadataArrayView<std::string_view>>>;

class MetadataPropertyImpl {
public:
  ~MetadataPropertyImpl(){};
  MetadataPropertyImpl(
      const DotNet::CesiumForUnity::MetadataProperty& property){};
  void
  JustBeforeDelete(const DotNet::CesiumForUnity::MetadataProperty& property){};
  DotNet::System::String
  GetPropertyName(const DotNet::CesiumForUnity::MetadataProperty& property);
  void SetProperty(
      const std::string& propertyName,
      const PropertyType& property,
      ValueType value);
  int GetComponentCount(
      const DotNet::CesiumForUnity::MetadataProperty& property);
  void GetComponent(
      const DotNet::CesiumForUnity::MetadataProperty& property,
      const DotNet::CesiumForUnity::MetadataProperty& component,
      int index);
  DotNet::CesiumForUnity::MetadataType
  GetMetadataType(const DotNet::CesiumForUnity::MetadataProperty& property);
  DotNet::CesiumForUnity::MetadataType
  GetComponentType(const DotNet::CesiumForUnity::MetadataProperty& property);
  bool IsNormalized(const DotNet::CesiumForUnity::MetadataProperty& property);

  std::int8_t GetInt8(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::int8_t defaultValue);
  std::uint8_t GetUInt8(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::uint8_t defaultValue);
  std::int16_t GetInt16(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::int16_t defaultValue);
  std::uint16_t GetUInt16(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::uint16_t defaultValue);
  std::int32_t GetInt32(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::int32_t defaultValue);
  std::uint32_t GetUInt32(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::uint32_t defaultValue);
  std::int64_t GetInt64(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::int64_t defaultValue);
  std::uint64_t GetUInt64(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      std::uint64_t defaultValue);
  float GetFloat32(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      float defaultValue);
  double GetFloat64(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      double defaultValue);
  bool GetBoolean(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      bool defaultValue);
  DotNet::System::String GetString(
      const DotNet::CesiumForUnity::MetadataProperty& value,
      const DotNet::System::String& defaultValue);

private:
  std::string _propertyName;
  PropertyType _propertyType;
  ValueType _value;
};
} // namespace CesiumForUnityNative