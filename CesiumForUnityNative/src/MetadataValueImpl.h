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
  std::uint8_t GetUInt8(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint8_t defaultValue);
  std::int16_t GetInt16(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int16_t defaultValue);
  std::uint16_t GetUInt16(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint16_t defaultValue);
  std::int32_t GetInt32(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int32_t defaultValue);
  std::uint32_t GetUInt32(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::uint32_t defaultValue);
  std::int64_t GetInt64(
      const DotNet::CesiumForUnity::MetadataValue& value,
      std::int64_t defaultValue);
  std::uint64_t GetUInt64(
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
    _type = std::visit(
        [](auto&& arg) {
          using T = std::decay_t<decltype(arg)>;
          return CesiumGltf::TypeToPropertyType<T>::value;
        },
        _value);
  }

  DotNet::CesiumForUnity::MetadataType
  GetMetadataType(const DotNet::CesiumForUnity::MetadataValue& value) {
    switch (_type) {
    case CesiumGltf::PropertyType::Int8:
      return DotNet::CesiumForUnity::MetadataType::Int8;
    case CesiumGltf::PropertyType::Uint8:
      return DotNet::CesiumForUnity::MetadataType::UInt8;
    case CesiumGltf::PropertyType::Int16:
      return DotNet::CesiumForUnity::MetadataType::Int16;
    case CesiumGltf::PropertyType::Uint16:
      return DotNet::CesiumForUnity::MetadataType::UInt16;
    case CesiumGltf::PropertyType::Int32:
      return DotNet::CesiumForUnity::MetadataType::Int32;
    case CesiumGltf::PropertyType::Uint32:
      return DotNet::CesiumForUnity::MetadataType::UInt32;
    case CesiumGltf::PropertyType::Int64:
      return DotNet::CesiumForUnity::MetadataType::Int64;
    case CesiumGltf::PropertyType::Uint64:
      return DotNet::CesiumForUnity::MetadataType::UInt64;
    case CesiumGltf::PropertyType::Float32:
      return DotNet::CesiumForUnity::MetadataType::Float;
    case CesiumGltf::PropertyType::Float64:
      return DotNet::CesiumForUnity::MetadataType::Double;
    case CesiumGltf::PropertyType::Boolean:
      return DotNet::CesiumForUnity::MetadataType::Boolean;
    case CesiumGltf::PropertyType::String:
      return DotNet::CesiumForUnity::MetadataType::String;
    case CesiumGltf::PropertyType::Array:
      return DotNet::CesiumForUnity::MetadataType::Array;
    default:
      return DotNet::CesiumForUnity::MetadataType::None;
    }
  }

  int64_t
  GetComponentCount(const DotNet::CesiumForUnity::MetadataValue& value) {
    return std::visit(
        [](auto&& arg) {
          using T = std::decay_t<decltype(arg)>;
          if constexpr (CesiumGltf::IsMetadataArray<T>::value) {
            return arg.size();
          } else {
            return static_cast<int64_t>(0);
          }
        },
        this->_value);
  }

  void GetComponent(
      const DotNet::CesiumForUnity::MetadataValue& value,
      const DotNet::CesiumForUnity::MetadataValue& component,
      int index);
  DotNet::CesiumForUnity::MetadataType
  GetComponentType(const DotNet::CesiumForUnity::MetadataValue& value);

private:
  ValueType _value;
  CesiumGltf::PropertyType _type;
};
} // namespace CesiumForUnityNative
