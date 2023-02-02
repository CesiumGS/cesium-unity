#include "CesiumGeoreferenceImpl.h"

#include "UnityTransforms.h"

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGeoreferenceOriginAuthority.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchorPositionAuthority.h>
#include <DotNet/System/Array1.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>

using namespace CesiumForUnityNative;
using namespace CesiumGeospatial;
using namespace CesiumUtility;

namespace {

LocalHorizontalCoordinateSystem createCoordinateSystem(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  if (georeference.originAuthority() ==
      DotNet::CesiumForUnity::CesiumGeoreferenceOriginAuthority::
          LongitudeLatitudeHeight) {
    return LocalHorizontalCoordinateSystem(
        Cartographic::fromDegrees(
            georeference.longitude(),
            georeference.latitude(),
            georeference.height()),
        LocalDirection::East,
        LocalDirection::Up,
        LocalDirection::North);
  } else {
    return LocalHorizontalCoordinateSystem(
        glm::dvec3(
            georeference.ecefX(),
            georeference.ecefY(),
            georeference.ecefZ()),
        LocalDirection::East,
        LocalDirection::Up,
        LocalDirection::North);
  }
}

} // namespace

CesiumGeoreferenceImpl::CesiumGeoreferenceImpl(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _coordinateSystem() {}

CesiumGeoreferenceImpl::~CesiumGeoreferenceImpl() {}

std::optional<DotNet::Unity::Mathematics::double3x3>
CesiumGeoreferenceImpl::RecalculateOrigin(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  // If there is no previous coordinate system, create the new one but return
  // without computing a delta rotation.
  if (!this->_coordinateSystem) {
    this->_coordinateSystem = createCoordinateSystem(georeference);
    return std::nullopt;
  }

  LocalHorizontalCoordinateSystem coordinateSystem =
      createCoordinateSystem(georeference);

  if (coordinateSystem.getLocalToEcefTransformation() ==
      this->_coordinateSystem->getLocalToEcefTransformation()) {
    // No change
    return std::nullopt;
  }

  // Update all globe anchors based on the new origin.
  std::swap(*this->_coordinateSystem, coordinateSystem);

  glm::dmat3 oldLocalToEcef =
      glm::dmat3(coordinateSystem.getLocalToEcefTransformation());
  glm::dmat3 ecefToNewLocal =
      glm::dmat3(this->_coordinateSystem->getEcefToLocalTransformation());
  glm::dmat3 oldLocalToNewLocal = ecefToNewLocal * oldLocalToEcef;

  return UnityTransforms::toUnityMathematics(oldLocalToNewLocal);
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformUnityPositionToEarthCenteredEarthFixed(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 unityPosition) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem(georeference);
  glm::dvec3 result = coordinateSystem.localPositionToEcef(
      glm::dvec3(unityPosition.x, unityPosition.y, unityPosition.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformEarthCenteredEarthFixedPositionToUnity(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem(georeference);
  glm::dvec3 result = coordinateSystem.ecefPositionToLocal(glm::dvec3(
      earthCenteredEarthFixed.x,
      earthCenteredEarthFixed.y,
      earthCenteredEarthFixed.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformUnityDirectionToEarthCenteredEarthFixed(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 unityDirection) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem(georeference);
  glm::dvec3 result = coordinateSystem.localDirectionToEcef(
      glm::dvec3(unityDirection.x, unityDirection.y, unityDirection.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformEarthCenteredEarthFixedDirectionToUnity(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 earthCenteredEarthFixedDirection) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem(georeference);
  glm::dvec3 result = coordinateSystem.ecefDirectionToLocal(glm::dvec3(
      earthCenteredEarthFixedDirection.x,
      earthCenteredEarthFixedDirection.y,
      earthCenteredEarthFixedDirection.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}

const CesiumGeospatial::LocalHorizontalCoordinateSystem&
CesiumGeoreferenceImpl::getCoordinateSystem(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  if (!this->_coordinateSystem) {
    this->RecalculateOrigin(georeference);
  }
  return *this->_coordinateSystem;
}
