#pragma once

namespace DotNet::CesiumForUnity {
class CesiumGlobeAnchor;
} // namespace DotNet::CesiumForUnity

namespace DotNet::Unity::Mathematics {
struct double3;
} // namespace DotNet::Unity::Mathematics

namespace CesiumForUnityNative {

class CesiumGlobeAnchorImpl {
public:
  static void AdjustOrientation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& globeAnchor,
      const ::DotNet::Unity::Mathematics::double3& oldPositionEcef,
      const ::DotNet::Unity::Mathematics::double3& newPositionEcef);
};

} // namespace CesiumForUnityNative
