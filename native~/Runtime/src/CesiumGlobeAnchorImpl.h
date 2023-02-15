#pragma once

namespace DotNet::CesiumForUnity {
class CesiumGlobeAnchor;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
struct double4x4;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumGlobeAnchorImpl {
public:
  static void SetNewEcef(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
      const ::DotNet::Unity::Mathematics::double4x4& newModelToEcef);

  static void SetNewEcefFromTransform(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor);
};

} // namespace CesiumForUnityNative
