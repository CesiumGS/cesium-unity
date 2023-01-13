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

  glm::dmat3 oldLocalToEcef =
      glm::dmat3(coordinateSystem.getLocalToEcefTransformation());
  glm::dmat3 ecefToNewLocal =
      glm::dmat3(this->_coordinateSystem.getEcefToLocalTransformation());
  glm::dmat3 oldLocalToNewLocal = ecefToNewLocal * oldLocalToEcef;

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGlobeAnchor> anchors =
      georeference.gameObject()
          .GetComponentsInChildren<DotNet::CesiumForUnity::CesiumGlobeAnchor>(
              true);

  for (int32_t i = 0; i < anchors.Length(); ++i) {
    DotNet::CesiumForUnity::CesiumGlobeAnchor anchor = anchors[i];

    DotNet::UnityEngine::Transform transform = anchor.transform();
    glm::dmat3 worldToOldLocal = glm::dmat3(
        UnityTransforms::fromUnity(transform.parent().worldToLocalMatrix()));
    glm::dmat3 modelToOldWorld =
        glm::dmat3(UnityTransforms::fromUnity(transform.localToWorldMatrix()));
    glm::dmat3 modelToNew =
        oldLocalToNewLocal * worldToOldLocal * modelToOldWorld;
    RotationAndScale rotationAndScale =
        UnityTransforms::matrixToRotationAndScale(modelToNew);

    transform.localRotation(
        UnityTransforms::toUnity(rotationAndScale.rotation));
    transform.localScale(UnityTransforms::toUnity(rotationAndScale.scale));

    // The meaning of Unity coordinates will change with the georeference
    // change, so switch to ECEF if necessary.
    DotNet::CesiumForUnity::CesiumGlobeAnchorPositionAuthority authority =
        anchor.positionAuthority();
    if (authority == DotNet::CesiumForUnity::
                         CesiumGlobeAnchorPositionAuthority::UnityCoordinates) {
      authority = DotNet::CesiumForUnity::CesiumGlobeAnchorPositionAuthority::
          EarthCenteredEarthFixed;
    }

    // Re-assign the (probably unchanged) authority to recompute Unity
    // coordinates with the new georeference. Unless it's still None,
    // because in that case setting the authority now could lock in the
    // globe location too early (e.g. before the CesiumGeoreference has
    // its final origin values).
    if (authority !=
        DotNet::CesiumForUnity::CesiumGlobeAnchorPositionAuthority::None)
      anchor.positionAuthority(authority);
  }
}

void CesiumGeoreferenceImpl::InitializeOrigin(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  // Compute the initial coordinate system. Don't call RecalculateOrigin because
  // that will also rotate objects based on the new origin.
  this->_coordinateSystem = createCoordinateSystem(georeference);
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformUnityPositionToEarthCenteredEarthFixed(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 unityPosition) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.localPositionToEcef(
      glm::dvec3(unityPosition.x, unityPosition.y, unityPosition.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformEarthCenteredEarthFixedPositionToUnity(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
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
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.localDirectionToEcef(
      glm::dvec3(unityDirection.x, unityDirection.y, unityDirection.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}

DotNet::Unity::Mathematics::double3
CesiumGeoreferenceImpl::TransformEarthCenteredEarthFixedDirectionToUnity(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
    DotNet::Unity::Mathematics::double3 earthCenteredEarthFixedDirection) {
  const LocalHorizontalCoordinateSystem& coordinateSystem =
      this->getCoordinateSystem();
  glm::dvec3 result = coordinateSystem.ecefDirectionToLocal(glm::dvec3(
      earthCenteredEarthFixedDirection.x,
      earthCenteredEarthFixedDirection.y,
      earthCenteredEarthFixedDirection.z));
  return DotNet::Unity::Mathematics::double3{result.x, result.y, result.z};
}
