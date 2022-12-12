#pragma once

#include "CesiumEllipsoidImpl.h"

#include <CesiumGeospatial/Ellipsoid.h>

using namespace CesiumGeospatial;
using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

double3 CesiumEllipsoidImpl::GetRadii() {
  const glm::dvec3 radii = Ellipsoid::WGS84.getRadii();
  return double3{radii.x, radii.y, radii.z};
}

std::optional<double3>
CesiumEllipsoidImpl::ScaleToGeodeticSurface(double3 earthCenteredEarthFixed) {
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

double3
CesiumEllipsoidImpl::GeodeticSurfaceNormal(double3 earthCenteredEarthFixed) {
  const glm::dvec3 cartesian(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z);

  glm::dvec3 result = Ellipsoid::WGS84.geodeticSurfaceNormal(cartesian);

  return double3{result.x, result.y, result.z};
}

} // namespace CesiumForUnityNative
