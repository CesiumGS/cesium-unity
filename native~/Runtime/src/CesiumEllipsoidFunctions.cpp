#include "CesiumEllipsoidFunctions.h"

#include <CesiumUtility/Math.h>

using namespace DotNet::Unity::Mathematics;
using namespace CesiumGeospatial;
using namespace CesiumUtility;

double3 CesiumEllipsoidFunctions::GetRadii(
    const CesiumGeospatial::Ellipsoid& ellipsoid) {
  const glm::dvec3& radii = ellipsoid.getRadii();
  return double3{radii.x, radii.y, radii.z};
}

std::optional<double3> CesiumEllipsoidFunctions::ScaleToGeodeticSurface(
    const CesiumGeospatial::Ellipsoid& ellipsoid,
    double3 earthCenteredEarthFixed) {
  auto result = ellipsoid.scaleToGeodeticSurface(glm::dvec3(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z));
  if (result) {
    return double3{result->x, result->y, result->z};
  }

  return std::nullopt;
}

DotNet::Unity::Mathematics::double3
CesiumEllipsoidFunctions::GeodeticSurfaceNormal(
    const CesiumGeospatial::Ellipsoid& ellipsoid,
    DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed) {
  glm::dvec3 result = ellipsoid.geodeticSurfaceNormal(glm::dvec3(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z));

  return double3{result.x, result.y, result.z};
}

DotNet::Unity::Mathematics::double3 CesiumEllipsoidFunctions::
    LongitudeLatitudeHeightToCenteredFixed(
        const CesiumGeospatial::Ellipsoid& ellipsoid,
        DotNet::Unity::Mathematics::double3 longitudeLatitudeHeight) {
  glm::dvec3 cartesian =
      ellipsoid.cartographicToCartesian(Cartographic::fromDegrees(
          longitudeLatitudeHeight.x,
          longitudeLatitudeHeight.y,
          longitudeLatitudeHeight.z));
  return double3{cartesian.x, cartesian.y, cartesian.z};
}

DotNet::Unity::Mathematics::double3 CesiumEllipsoidFunctions::
    CenteredFixedToLongitudeLatitudeHeight(
        const CesiumGeospatial::Ellipsoid& ellipsoid,
        DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed) {
  std::optional<Cartographic> result =
      ellipsoid.cartesianToCartographic(glm::dvec3(
          earthCenteredEarthFixed.x,
          earthCenteredEarthFixed.y,
          earthCenteredEarthFixed.z));
  if (result) {
    return double3{
        Math::radiansToDegrees(result->longitude),
        Math::radiansToDegrees(result->latitude),
        result->height};
  } else {
    return double3{0.0, 0.0, 0.0};
  }
}
