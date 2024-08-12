#pragma once

#include <CesiumGeospatial/Ellipsoid.h>

#include <DotNet/Unity/Mathematics/double3.h>

#include <optional>

class CesiumEllipsoidFunctions {
public:
  static DotNet::Unity::Mathematics::double3
  GetRadii(const CesiumGeospatial::Ellipsoid& ellipsoid);
  static std::optional<DotNet::Unity::Mathematics::double3>
  ScaleToGeodeticSurface(
      const CesiumGeospatial::Ellipsoid& ellipsoid,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  static DotNet::Unity::Mathematics::double3 GeodeticSurfaceNormal(
      const CesiumGeospatial::Ellipsoid& ellipsoid,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  static DotNet::Unity::Mathematics::double3
  LongitudeLatitudeHeightToCenteredFixed(
      const CesiumGeospatial::Ellipsoid& ellipsoid,
      DotNet::Unity::Mathematics::double3 longitudeLatitudeHeight);
  static DotNet::Unity::Mathematics::double3
  CenteredFixedToLongitudeLatitudeHeight(
      const CesiumGeospatial::Ellipsoid& ellipsoid,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
};
