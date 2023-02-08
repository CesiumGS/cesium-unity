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
  static ::DotNet::Unity::Mathematics::double4x4 AdjustOrientation(
      const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& globeAnchor,
      const ::DotNet::Unity::Mathematics::double3& oldPositionEcef,
      const ::DotNet::Unity::Mathematics::double3& newPositionEcef,
      const ::DotNet::Unity::Mathematics::double4x4& newModelToEcef);
};

} // namespace CesiumForUnityNative
