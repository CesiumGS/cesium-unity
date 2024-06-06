#include "CesiumWgs84EllipsoidImpl.h"

#include "CesiumEllipsoidFunctions.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumUtility/Math.h>

using namespace CesiumGeospatial;
using namespace CesiumUtility;

using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

double3 CesiumWgs84EllipsoidImpl::GetRadii() {
  return CesiumEllipsoidFunctions::GetRadii(Ellipsoid::WGS84);
}

std::optional<double3> CesiumWgs84EllipsoidImpl::ScaleToGeodeticSurface(
    double3 earthCenteredEarthFixed) {
  return CesiumEllipsoidFunctions::ScaleToGeodeticSurface(
      Ellipsoid::WGS84,
      earthCenteredEarthFixed);
}

double3 CesiumWgs84EllipsoidImpl::GeodeticSurfaceNormal(
    double3 earthCenteredEarthFixed) {
  return CesiumEllipsoidFunctions::GeodeticSurfaceNormal(
      Ellipsoid::WGS84,
      earthCenteredEarthFixed);
}

double3
CesiumWgs84EllipsoidImpl::LongitudeLatitudeHeightToEarthCenteredEarthFixed(
    double3 longitudeLatitudeHeight) {
  return CesiumEllipsoidFunctions::
      LongitudeLatitudeHeightToEllipsoidCenteredEllipsoidFixed(
          Ellipsoid::WGS84,
          longitudeLatitudeHeight);
}

double3
CesiumWgs84EllipsoidImpl::EarthCenteredEarthFixedToLongitudeLatitudeHeight(
    double3 earthCenteredEarthFixed) {
  return CesiumEllipsoidFunctions::
      EllipsoidCenteredEllipsoidFixedToLongitudeLatitudeHeight(
          Ellipsoid::WGS84,
          earthCenteredEarthFixed);
}

} // namespace CesiumForUnityNative
