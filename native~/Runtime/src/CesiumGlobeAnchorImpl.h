#pragma once

namespace DotNet::CesiumForUnity {
class CesiumGlobeAnchor;
struct CesiumVector3;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class CesiumGlobeAnchorImpl {
public:
  static void AdjustOrientation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& globeAnchor,
      const ::DotNet::CesiumForUnity::CesiumVector3& oldPositionEcef,
      const ::DotNet::CesiumForUnity::CesiumVector3& newPositionEcef);
};

} // namespace CesiumForUnityNative
