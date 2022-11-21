#include "CesiumTransformsImpl.h"

#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumUtility/Math.h>

using namespace CesiumGeospatial;
using namespace CesiumUtility;
using namespace DotNet::CesiumForUnity;

namespace CesiumForUnityNative {

CesiumVector3
CesiumTransformsImpl::LongitudeLatitudeHeightToEarthCenteredEarthFixed(
    const DotNet::CesiumForUnity::CesiumVector3& longitudeLatitudeHeight) {
  glm::dvec3 cartesian =
      Ellipsoid::WGS84.cartographicToCartesian(Cartographic::fromDegrees(
          longitudeLatitudeHeight.x(),
          longitudeLatitudeHeight.y(),
          longitudeLatitudeHeight.z()));
  return CesiumVector3{cartesian.x, cartesian.y, cartesian.z};
}

CesiumVector3
CesiumTransformsImpl::EarthCenteredEarthFixedToLongitudeLatitudeHeight(
    const DotNet::CesiumForUnity::CesiumVector3& earthCenteredEarthFixed) {
  std::optional<Cartographic> result =
      Ellipsoid::WGS84.cartesianToCartographic(glm::dvec3(
          earthCenteredEarthFixed.x(),
          earthCenteredEarthFixed.y(),
          earthCenteredEarthFixed.z()));
  if (result) {
    return CesiumVector3{
        Math::radiansToDegrees(result->longitude),
        Math::radiansToDegrees(result->latitude),
        result->height};
  } else {
    return CesiumVector3(0.0, 0.0, 0.0);
  }
}

CesiumVector3 CesiumTransformsImpl::ScaleCartesianToEllipsoidGeodeticSurface(
    const DotNet::CesiumForUnity::CesiumVector3& cartesian) {
  const Ellipsoid& ellipsoid = Ellipsoid::WGS84;
  const glm::dvec3 glmCartesian(cartesian.x(), cartesian.y(), cartesian.z());
  auto result = ellipsoid.scaleToGeodeticSurface(glmCartesian);
  if (result) {
    return CesiumVector3(result->x, result->y, result->z);
  }

  return nullptr;
}

} // namespace CesiumForUnityNative
