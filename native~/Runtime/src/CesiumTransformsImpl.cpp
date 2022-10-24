#include "CesiumTransformsImpl.h"

#include <CesiumGeospatial/Ellipsoid.h>

using namespace CesiumGeospatial;
using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {

CesiumVector3
CesiumTransformsImpl::LongitudeLatitudeHeightToEarthCenteredEarthFixed(
    CesiumVector3 longitudeLatitudeHeight) {
  glm::dvec3 cartesian = Ellipsoid::WGS84.cartographicToCartesian(Cartographic(
      longitudeLatitudeHeight.x,
      longitudeLatitudeHeight.y,
      longitudeLatitudeHeight.z));
  return CesiumVector3{cartesian.x, cartesian.y, cartesian.z};
}

CesiumVector3
CesiumTransformsImpl::EarthCenteredEarthFixedToLongitudeLatitudeHeight(
    CesiumVector3 earthCenteredEarthFixed) {
  std::optional<Cartographic> result =
      Ellipsoid::WGS84.cartesianToCartographic(glm::dvec3(
          earthCenteredEarthFixed.x,
          earthCenteredEarthFixed.y,
          earthCenteredEarthFixed.z));
  if (result) {
    return CesiumVector3{result->longitude, result->latitude, result->height};
  } else {
    return CesiumVector3{0.0, 0.0, 0.0};
  }
}

} // namespace CesiumForUnityNative
