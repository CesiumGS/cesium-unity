#pragma once
#include <CesiumGltf/MetadataArrayView.h>
#include <CesiumGltf/PropertyType.h>
#include <CesiumGltf/PropertyTypeTraits.h>

#include <DotNet/CesiumForUnity/MetadataType.h>

#include <cstdint>
#include <variant>

namespace DotNet::CesiumForUnity {
class MetadataValue;
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

class MetadataValueImpl {

public:
  ~MetadataValueImpl(){};
  MetadataValueImpl(const DotNet::CesiumForUnity::MetadataValue& value){};
  void JustBeforeDelete(const DotNet::CesiumForUnity::MetadataValue& value){};
  std::int8_t GetInt8(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int8_t defaultValue);
  std::uint8_t GetUint8(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint8_t defaultValue);
  std::int16_t GetInt16(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int16_t defaultValue);
  std::uint16_t GetUint16(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint16_t defaultValue);
  std::int32_t GetInt32(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int32_t defaultValue);
  std::uint32_t GetUint32(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint32_t defaultValue);
  std::int64_t GetInt64(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int64_t defaultValue);
  std::uint64_t GetUint64(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint64_t defaultValue);
  float GetFloat32(
      const DotNet::CesiumForUnity::MetadataValue& value,
      float defaultValue);
  double GetFloat64(
      const DotNet::CesiumForUnity::MetadataValue& value,
      double defaultValue);
  bool GetBoolean(
      const DotNet::CesiumForUnity::MetadataValue& value,
      bool defaultValue);
  DotNet::System::String GetString(
      const DotNet::CesiumForUnity::MetadataValue& value,
      const DotNet::System::String& defaultValue);
  void SetValue(const ValueType& value) {
    _value = value;
  }

  DotNet::CesiumForUnity::MetadataType
  GetMetadataType(const DotNet::CesiumForUnity::MetadataValue& value) {
    return DotNet::CesiumForUnity::MetadataType::Boolean;
  }

private:
  ValueType _value;
};
} // namespace CesiumForUnityNative
