#pragma once

#include <DotNet/System/String.h>
#include <glm/glm.hpp>

namespace DotNet::CesiumForUnity {
class CesiumMetadataValue;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
class Object;
} // namespace DotNet::System

namespace DotNet::Unity::Mathematics {
struct int2;
struct int2x2;
struct int3;
struct int3x3;
struct int4;
struct int4x4;
struct uint2;
struct uint2x2;
struct uint3;
struct uint3x3;
struct uint4;
struct uint4x4;
struct float2;
struct float2x2;
struct float3;
struct float3x3;
struct float4;
struct float4x4;
struct double2;
struct double2x2;
struct double3;
struct double3x3;
struct double4;
struct double4x4;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumMetadataValueImpl {
public:
  static bool ConvertToBoolean(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      bool defaultValue);

  static int8_t ConvertToSByte(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      int8_t defaultValue);

  static uint8_t ConvertToByte(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      uint8_t defaultValue);

  static int16_t ConvertToInt16(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      int16_t defaultValue);

  static uint16_t ConvertToUInt16(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      uint16_t defaultValue);

  static int32_t ConvertToInt32(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      int32_t defaultValue);

  static uint32_t ConvertToUInt32(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      uint32_t defaultValue);

  static int64_t ConvertToInt64(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      int64_t defaultValue);

  static uint64_t ConvertToUInt64(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      uint64_t defaultValue);

  static float ConvertToFloat(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      float defaultValue);

  static double ConvertToDouble(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      double defaultValue);

  static DotNet::Unity::Mathematics::int2 ConvertToInt2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::int2 defaultValue);

  static DotNet::Unity::Mathematics::uint2 ConvertToUInt2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::uint2 defaultValue);

  static DotNet::Unity::Mathematics::float2 ConvertToFloat2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::float2 defaultValue);

  static DotNet::Unity::Mathematics::double2 ConvertToDouble2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::double2 defaultValue);

  static DotNet::Unity::Mathematics::int3 ConvertToInt3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::int3 defaultValue);

  static DotNet::Unity::Mathematics::uint3 ConvertToUInt3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::uint3 defaultValue);

  static DotNet::Unity::Mathematics::float3 ConvertToFloat3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::float3 defaultValue);

  static DotNet::Unity::Mathematics::double3 ConvertToDouble3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::double3 defaultValue);

  static DotNet::Unity::Mathematics::int4 ConvertToInt4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::int4 defaultValue);

  static DotNet::Unity::Mathematics::uint4 ConvertToUInt4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::uint4 defaultValue);

  static DotNet::Unity::Mathematics::float4 ConvertToFloat4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::float4 defaultValue);

  static DotNet::Unity::Mathematics::double4 ConvertToDouble4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::double4 defaultValue);

  static DotNet::Unity::Mathematics::int2x2 ConvertToInt2x2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::int2x2 defaultValue);

  static DotNet::Unity::Mathematics::uint2x2 ConvertToUInt2x2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::uint2x2 defaultValue);

  static DotNet::Unity::Mathematics::float2x2 ConvertToFloat2x2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::float2x2 defaultValue);

  static DotNet::Unity::Mathematics::double2x2 ConvertToDouble2x2(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::double2x2 defaultValue);

  static DotNet::Unity::Mathematics::int3x3 ConvertToInt3x3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::int3x3 defaultValue);

  static DotNet::Unity::Mathematics::uint3x3 ConvertToUInt3x3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::uint3x3 defaultValue);

  static DotNet::Unity::Mathematics::float3x3 ConvertToFloat3x3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::float3x3 defaultValue);

  static DotNet::Unity::Mathematics::double3x3 ConvertToDouble3x3(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::double3x3 defaultValue);

  static DotNet::Unity::Mathematics::int4x4 ConvertToInt4x4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::int4x4 defaultValue);

  static DotNet::Unity::Mathematics::uint4x4 ConvertToUInt4x4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::uint4x4 defaultValue);

  static DotNet::Unity::Mathematics::float4x4 ConvertToFloat4x4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::float4x4 defaultValue);

  static DotNet::Unity::Mathematics::double4x4 ConvertToDouble4x4(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::Unity::Mathematics::double4x4 defaultValue);

  static DotNet::System::String ConvertToString(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      DotNet::System::String defaultValue);
};
} // namespace CesiumForUnityNative
