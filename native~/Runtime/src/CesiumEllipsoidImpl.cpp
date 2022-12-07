#pragma once

#include "CesiumEllipsoidImpl.h"

#include <CesiumGeospatial/Ellipsoid.h>

using namespace CesiumGeospatial;
using namespace DotNet::Unity::Mathematics;
using namespace DotNet::System;

namespace CesiumForUnityNative {
Nullable1<double3>
CesiumEllipsoidImpl::ScaleToGeodeticSurface(double3 earthCenteredEarthFixed) {
  const glm::dvec3 cartesian(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z);

  auto result = Ellipsoid::WGS84.scaleToGeodeticSurface(cartesian);
  if (result) {
    return Nullable1<double3>(double3{result->x, result->y, result->z});
  }

  return Nullable1<double3>();
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
