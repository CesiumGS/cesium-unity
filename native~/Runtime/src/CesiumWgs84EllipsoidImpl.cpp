#pragma once

#include "CesiumWgs84EllipsoidImpl.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumUtility/Math.h>

using namespace CesiumGeospatial;
using namespace CesiumUtility;

using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

double3 CesiumWgs84EllipsoidImpl::GetRadii() {
  const glm::dvec3 radii = Ellipsoid::WGS84.getRadii();
  return double3{radii.x, radii.y, radii.z};
}

std::optional<double3> CesiumWgs84EllipsoidImpl::ScaleToGeodeticSurface(
    double3 earthCenteredEarthFixed) {
  const glm::dvec3 cartesian(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z);

  auto result = Ellipsoid::WGS84.scaleToGeodeticSurface(cartesian);
  if (result) {
    return double3{result->x, result->y, result->z};
  }

  return std::nullopt;
}

double3 CesiumWgs84EllipsoidImpl::GeodeticSurfaceNormal(
    double3 earthCenteredEarthFixed) {
  const glm::dvec3 cartesian(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z);

  glm::dvec3 result = Ellipsoid::WGS84.geodeticSurfaceNormal(cartesian);

  return double3{result.x, result.y, result.z};
}

double3
CesiumWgs84EllipsoidImpl::LongitudeLatitudeHeightToEarthCenteredEarthFixed(
    double3 longitudeLatitudeHeight) {
  glm::dvec3 cartesian =
      Ellipsoid::WGS84.cartographicToCartesian(Cartographic::fromDegrees(
          longitudeLatitudeHeight.x,
          longitudeLatitudeHeight.y,
          longitudeLatitudeHeight.z));
  return double3{cartesian.x, cartesian.y, cartesian.z};
}

double3
CesiumWgs84EllipsoidImpl::EarthCenteredEarthFixedToLongitudeLatitudeHeight(
    double3 earthCenteredEarthFixed) {
  std::optional<Cartographic> result =
      Ellipsoid::WGS84.cartesianToCartographic(glm::dvec3(
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

} // namespace CesiumForUnityNative
