#pragma once
#include <cstdint>

namespace DotNet::CesiumForUnity {
class MetadataValue;
}

namespace DotNet::System {
  class String;
}

namespace CesiumForUnityNative {

class MetadataValueImpl {
public:
  ~MetadataValueImpl(){};
  MetadataValueImpl(const DotNet::CesiumForUnity::MetadataValue& value){};
  void JustBeforeDelete(const DotNet::CesiumForUnity::MetadataValue& value){};
  bool IsInt8(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsUint8(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsInt16(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsUint16(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsInt32(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsUint32(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsInt64(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsUint64(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsFloat32(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsFloat64(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsBoolean(const DotNet::CesiumForUnity::MetadataValue& value);
  bool IsString(const DotNet::CesiumForUnity::MetadataValue& value);
  std::int8_t GetInt8(const DotNet::CesiumForUnity::MetadataValue& value);
  std::uint8_t GetUint8(const DotNet::CesiumForUnity::MetadataValue& value);
  std::int16_t GetInt16(const DotNet::CesiumForUnity::MetadataValue& value);
  std::uint16_t GetUint16(const DotNet::CesiumForUnity::MetadataValue& value);
  std::int32_t GetInt32(const DotNet::CesiumForUnity::MetadataValue& value);
  std::uint32_t GetUint32(const DotNet::CesiumForUnity::MetadataValue& value);
  std::int64_t GetInt64(const DotNet::CesiumForUnity::MetadataValue& value);
  std::uint64_t GetUint64(const DotNet::CesiumForUnity::MetadataValue& value);
  float GetFloat32(const DotNet::CesiumForUnity::MetadataValue& value);
  double GetFloat64(const DotNet::CesiumForUnity::MetadataValue& value);
  bool GetBoolean(const DotNet::CesiumForUnity::MetadataValue& value);
  DotNet::System::String GetString(const DotNet::CesiumForUnity::MetadataValue& value);
};
} // namespace CesiumForUnityNative
