#pragma once

#include <any>
#include <cstdint>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumPropertyTableProperty;
class CesiumPropertyArray;
class CesiumMetadataValue;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
}

namespace DotNet::Unity::Mathematics {
class int2;
class int3;
class int4;
class uint2;
class uint3;
class uint4;
class float2;
class float3;
class float4;
class double2;
class double3;
class double4;
class int2x2;
class uint2x2;
class float2x2;
class double2x2;
class int3x3;
class uint3x3;
class float3x3;
class double3x3;
class int4x4;
class uint4x4;
class float4x4;
class double4x4;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumPropertyTablePropertyImpl {
public:
  CesiumPropertyTablePropertyImpl(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property);
  ~CesiumPropertyTablePropertyImpl();

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

  DotNet::Unity::Mathematics::int2x2 GetInt2x2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::int2x2 defaultValue);

  DotNet::Unity::Mathematics::uint2x2 GetUInt2x2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::uint2x2 defaultValue);

  DotNet::Unity::Mathematics::float2x2 GetFloat2x2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::float2x2 defaultValue);

  DotNet::Unity::Mathematics::double2x2 GetDouble2x2(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::double2x2 defaultValue);

  DotNet::Unity::Mathematics::int3x3 GetInt3x3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::int3x3 defaultValue);

  DotNet::Unity::Mathematics::uint3x3 GetUInt3x3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::uint3x3 defaultValue);

  DotNet::Unity::Mathematics::float3x3 GetFloat3x3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::float3x3 defaultValue);

  DotNet::Unity::Mathematics::double3x3 GetDouble3x3(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::double3x3 defaultValue);

  DotNet::Unity::Mathematics::int4x4 GetInt4x4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::int4x4 defaultValue);

  DotNet::Unity::Mathematics::uint4x4 GetUInt4x4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::uint4x4 defaultValue);

  DotNet::Unity::Mathematics::float4x4 GetFloat4x4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::float4x4 defaultValue);

  DotNet::Unity::Mathematics::double4x4 GetDouble4x4(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      DotNet::Unity::Mathematics::double4x4 defaultValue);

  DotNet::System::String GetString(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID,
      const DotNet::System::String& defaultValue);

  DotNet::CesiumForUnity::CesiumPropertyArray GetArray(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID);

  DotNet::CesiumForUnity::CesiumMetadataValue GetValue(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID);

  DotNet::CesiumForUnity::CesiumMetadataValue GetRawValue(
      const DotNet::CesiumForUnity::CesiumPropertyTableProperty& property,
      std::int64_t featureID);

private:
  std::any _property;

  friend class CesiumFeaturesMetadataUtility;
  friend class TestGltfModelImpl;
};

} // namespace CesiumForUnityNative
