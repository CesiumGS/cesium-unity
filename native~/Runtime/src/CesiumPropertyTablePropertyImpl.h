#pragma once

namespace CesiumForUnityNative {
class CesiumPropertyTablePropertyImpl;
}

#include "CesiumFeaturesMetadataUtility.h"

#include <CesiumGltf/PropertyTablePropertyView.h>

#include <DotNet/CesiumForUnity/CesiumPropertyTableProperty.h>

#include <any>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumPropertyTableProperty;
class CesiumMetadataValue;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
}

namespace DotNet::Unity::Mathematics {
class int2;
class uint2;
class float2;
class double2;
class int3;
class uint3;
class float3;
class double3;
class int4;
class uint4;
class float4;
class double4;
} // namespace DotNet::Unity::Mathematics

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

  DotNet::Unity::Mathematics::int2 GetInt2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::int2 defaultValue);

  DotNet::Unity::Mathematics::uint2 GetUInt2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::uint2 defaultValue);

  DotNet::Unity::Mathematics::float2 GetFloat2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::float2 defaultValue);

  DotNet::Unity::Mathematics::double2 GetDouble2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::double2 defaultValue);

  DotNet::Unity::Mathematics::int3 GetInt3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::int3 defaultValue);

  DotNet::Unity::Mathematics::uint3 GetUInt3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::uint3 defaultValue);

  DotNet::Unity::Mathematics::float3 GetFloat3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::float3 defaultValue);

  DotNet::Unity::Mathematics::double3 GetDouble3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::double3 defaultValue);

    DotNet::Unity::Mathematics::int4 GetInt4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::int4 defaultValue);

  DotNet::Unity::Mathematics::uint4 GetUInt4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::uint4 defaultValue);

  DotNet::Unity::Mathematics::float4 GetFloat4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::float4 defaultValue);

  DotNet::Unity::Mathematics::double4 GetDouble4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::double4 defaultValue);

  DotNet::System::String GetString(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      const DotNet::System::String& defaultValue);

  // clang-format off
  // DotNet::CesiumForUnity::CesiumMetadataValue GetValue(
  //   const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
  //   std::int64_t featureID);
  // clang-format on

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
