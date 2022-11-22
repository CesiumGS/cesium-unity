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
    : _coordinateSystem(createCoordinateSystem(georeference)) {}

CesiumGeoreferenceImpl::~CesiumGeoreferenceImpl() {}

void CesiumGeoreferenceImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {}

void CesiumGeoreferenceImpl::RecalculateOrigin(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  LocalHorizontalCoordinateSystem coordinateSystem =
      createCoordinateSystem(georeference);

  if (coordinateSystem.getLocalToEcefTransformation() ==
      this->_coordinateSystem.getLocalToEcefTransformation()) {
    // No change
    return;
  }

  // Update all globe anchors based on the new origin.
  std::swap(this->_coordinateSystem, coordinateSystem);

  glm::dmat3 oldToEcef =
      glm::dmat3(coordinateSystem.getLocalToEcefTransformation());
  glm::dmat3 ecefToNew =
      glm::dmat3(this->_coordinateSystem.getEcefToLocalTransformation());
  glm::dmat3 oldToNew = ecefToNew * oldToEcef;

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGlobeAnchor> anchors =
      georeference.gameObject()
          .GetComponentsInChildren<DotNet::CesiumForUnity::CesiumGlobeAnchor>(
              true);

  for (int32_t i = 0; i < anchors.Length(); ++i) {
    DotNet::CesiumForUnity::CesiumGlobeAnchor anchor = anchors[i];

    DotNet::UnityEngine::Transform transform = anchor.transform();
    glm::dmat3 modelToOld =
        glm::dmat3(UnityTransforms::fromUnity(transform.localToWorldMatrix()));
    glm::dmat3 modelToNew = oldToNew * modelToOld;
    RotationAndScale rotationAndScale =
        UnityTransforms::matrixToRotationAndScale(modelToNew);

    transform.rotation(UnityTransforms::toUnity(rotationAndScale.rotation));
    transform.localScale(UnityTransforms::toUnity(rotationAndScale.scale));

    // The meaning of Unity coordinates will change with the georeference
    // change, so switch to ECEF if necessary.
    DotNet::CesiumForUnity::CesiumGlobeAnchorPositionAuthority authority =
        anchor.positionAuthority();
    if (authority ==
        DotNet::CesiumForUnity::CesiumGlobeAnchorPositionAuthority::
            UnityWorldCoordinates) {
      authority = DotNet::CesiumForUnity::CesiumGlobeAnchorPositionAuthority::
          EarthCenteredEarthFixed;
    }

    // Re-assign the (probably unchanged) authority to recompute Unity
    // coordinates with the new georeference.
    anchor.positionAuthority(authority);
  }
}

void CesiumGeoreferenceImpl::InitializeOrigin(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  // Compute the initial coordinate system. Don't call RecalculateOrigin because
  // that will also rotate objects based on the new origin.
  this->_coordinateSystem = createCoordinateSystem(georeference);
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

DotNet::CesiumForUnity::CesiumVector3
CesiumGeoreferenceImpl::TransformUnityWorldDirectionToEarthCenteredEarthFixed(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::CesiumForUnity::CesiumVector3 unityWorldDirection) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.localDirectionToEcef(glm::dvec3(
      unityWorldDirection.x,
      unityWorldDirection.y,
      unityWorldDirection.z));
  return DotNet::CesiumForUnity::CesiumVector3{result.x, result.y, result.z};
}

DotNet::CesiumForUnity::CesiumVector3
CesiumGeoreferenceImpl::TransformEarthCenteredEarthFixedDirectionToUnityWorld(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixedDirection) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.ecefDirectionToLocal(glm::dvec3(
      earthCenteredEarthFixedDirection.x,
      earthCenteredEarthFixedDirection.y,
      earthCenteredEarthFixedDirection.z));
  return DotNet::CesiumForUnity::CesiumVector3{result.x, result.y, result.z};
}
