#pragma once

namespace DotNet::CesiumForUnity {
class CesiumGlobeAnchor;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
struct double4x4;
struct quaternion;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumGlobeAnchorImpl {
public:
  static void SetNewEcef(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
      const ::DotNet::Unity::Mathematics::double4x4& newModelToEcef);

  static void SetNewEcefFromTransform(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor);

  static ::DotNet::Unity::Mathematics::quaternion GetModelToEastUpNorthRotation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor);

  static void SetModelToEastUpNorthRotation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
      const ::DotNet::Unity::Mathematics::quaternion& value);
};

} // namespace CesiumForUnityNative
