#pragma once

#include <DotNet/Unity/Mathematics/double3.h>

#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumEllipsoid;
}

namespace CesiumForUnityNative {

class CesiumEllipsoidImpl {
public:
  static std::optional<DotNet::Unity::Mathematics::double3>
  ScaleToGeodeticSurface(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  static DotNet::Unity::Mathematics::double3 GeodeticSurfaceNormal(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
};

} // namespace CesiumForUnityNative
