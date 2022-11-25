#include "CesiumTransformsImpl.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumUtility/Math.h>

using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace DotNet::CesiumForUnity;
using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

double3 CesiumTransformsImpl::LongitudeLatitudeHeightToEarthCenteredEarthFixed(
    double3 longitudeLatitudeHeight) {
  glm::dvec3 cartesian =
      Ellipsoid::WGS84.cartographicToCartesian(Cartographic::fromDegrees(
          longitudeLatitudeHeight.x,
          longitudeLatitudeHeight.y,
          longitudeLatitudeHeight.z));
  return double3{cartesian.x, cartesian.y, cartesian.z};
}

double3 CesiumTransformsImpl::EarthCenteredEarthFixedToLongitudeLatitudeHeight(
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
