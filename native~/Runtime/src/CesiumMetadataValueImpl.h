#pragma once

#include <DotNet/System/String.h>
#include <glm/glm.hpp>

#include <variant>

namespace DotNet::CesiumForUnity {
class CesiumMetadataValue;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
class String;
class Object;
} // namespace DotNet::System

namespace CesiumForUnityNative {

class CesiumMetadataValueImpl {
public:
#pragma region ValueType declaration
  // This definition excludes arrays because those are handled in C#.
  using ValueType = std::variant<
      std::monostate,
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
      std::string,
      glm::vec<2, int8_t>,
      glm::vec<2, uint8_t>,
      glm::vec<2, int16_t>,
      glm::vec<2, uint16_t>,
      glm::vec<2, int32_t>,
      glm::vec<2, uint32_t>,
      glm::vec<2, int64_t>,
      glm::vec<2, uint64_t>,
      glm::vec<2, float>,
      glm::vec<2, double>,
      glm::vec<3, int8_t>,
      glm::vec<3, uint8_t>,
      glm::vec<3, int16_t>,
      glm::vec<3, uint16_t>,
      glm::vec<3, int32_t>,
      glm::vec<3, uint32_t>,
      glm::vec<3, int64_t>,
      glm::vec<3, uint64_t>,
      glm::vec<3, float>,
      glm::vec<3, double>,
      glm::vec<4, int8_t>,
      glm::vec<4, uint8_t>,
      glm::vec<4, int16_t>,
      glm::vec<4, uint16_t>,
      glm::vec<4, int32_t>,
      glm::vec<4, uint32_t>,
      glm::vec<4, int64_t>,
      glm::vec<4, uint64_t>,
      glm::vec<4, float>,
      glm::vec<4, double>,
      glm::mat<2, 2, int8_t>,
      glm::mat<2, 2, uint8_t>,
      glm::mat<2, 2, int16_t>,
      glm::mat<2, 2, uint16_t>,
      glm::mat<2, 2, int32_t>,
      glm::mat<2, 2, uint32_t>,
      glm::mat<2, 2, int64_t>,
      glm::mat<2, 2, uint64_t>,
      glm::mat<2, 2, float>,
      glm::mat<2, 2, double>,
      glm::mat<3, 3, int8_t>,
      glm::mat<3, 3, uint8_t>,
      glm::mat<3, 3, int16_t>,
      glm::mat<3, 3, uint16_t>,
      glm::mat<3, 3, int32_t>,
      glm::mat<3, 3, uint32_t>,
      glm::mat<3, 3, int64_t>,
      glm::mat<3, 3, uint64_t>,
      glm::mat<3, 3, float>,
      glm::mat<3, 3, double>,
      glm::mat<4, 4, int8_t>,
      glm::mat<4, 4, uint8_t>,
      glm::mat<4, 4, int16_t>,
      glm::mat<4, 4, uint16_t>,
      glm::mat<4, 4, int32_t>,
      glm::mat<4, 4, uint32_t>,
      glm::mat<4, 4, int64_t>,
      glm::mat<4, 4, uint64_t>,
      glm::mat<4, 4, float>,
      glm::mat<4, 4, double>>;
#pragma endregion

  static bool ConvertToBoolean(
      const DotNet::CesiumForUnity::CesiumMetadataValue& value,
      bool defaultValue);

private:
  /**
   * Retrieves the value from the System.Object in the C#
   * class implementation as a C++-compatible type. std::monostate is used to
   * indicate a null value.
   */
  static ValueType
  getNativeValue(const DotNet::CesiumForUnity::CesiumMetadataValue& value);
};
} // namespace CesiumForUnityNative
