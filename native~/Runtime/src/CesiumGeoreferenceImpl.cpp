#include "CesiumGeoreferenceImpl.h"

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>

using namespace CesiumForUnityNative;
using namespace CesiumGeospatial;
using namespace CesiumUtility;

namespace {

LocalHorizontalCoordinateSystem createCoordinateSystem(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  return LocalHorizontalCoordinateSystem(
      Cartographic::fromDegrees(
          georeference.longitude(),
          georeference.latitude(),
          georeference.height()),
      LocalDirection::East,
      LocalDirection::Up,
      LocalDirection::North);
}

} // namespace

CesiumGeoreferenceImpl::CesiumGeoreferenceImpl(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _coordinateSystem(createCoordinateSystem(georeference)) {}

CesiumGeoreferenceImpl::~CesiumGeoreferenceImpl() {}

void CesiumGeoreferenceImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {}

void CesiumGeoreferenceImpl::RecalculateOrigin(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  this->_coordinateSystem = createCoordinateSystem(georeference);
}

void CesiumGeoreferenceImpl::OnValidate(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  georeference.UpdateOrigin();
}

void CesiumGeoreferenceImpl::Awake(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  georeference.UpdateOrigin();
}

DotNet::CesiumForUnity::CesiumVector3
CesiumGeoreferenceImpl::TransformUnityWorldPositionToEarthCenteredEarthFixed(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::CesiumForUnity::CesiumVector3 unityWorldPosition) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.localPositionToEcef(glm::dvec3(
      unityWorldPosition.x,
      unityWorldPosition.y,
      unityWorldPosition.z));
  return DotNet::CesiumForUnity::CesiumVector3{result.x, result.y, result.z};
}

DotNet::CesiumForUnity::CesiumVector3
CesiumGeoreferenceImpl::TransformEarthCenteredEarthFixedPositionToUnityWorld(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixed) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.ecefPositionToLocal(glm::dvec3(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z));
  return DotNet::CesiumForUnity::CesiumVector3{result.x, result.y, result.z};
}
