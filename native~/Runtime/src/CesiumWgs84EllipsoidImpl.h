#pragma once

#include <DotNet/Unity/Mathematics/double3.h>

#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumWgs84Ellipsoid;
}

namespace CesiumForUnityNative {

class CesiumWgs84EllipsoidImpl {
public:
  static DotNet::Unity::Mathematics::double3 GetRadii();
  static std::optional<DotNet::Unity::Mathematics::double3>
  ScaleToGeodeticSurface(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  static DotNet::Unity::Mathematics::double3 GeodeticSurfaceNormal(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  static DotNet::Unity::Mathematics::double3
  LongitudeLatitudeHeightToEarthCenteredEarthFixed(
      DotNet::Unity::Mathematics::double3 longitudeLatitudeHeight);
  static DotNet::Unity::Mathematics::double3
  EarthCenteredEarthFixedToLongitudeLatitudeHeight(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
};

} // namespace CesiumForUnityNative
