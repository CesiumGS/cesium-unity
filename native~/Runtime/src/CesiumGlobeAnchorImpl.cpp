#include "CesiumGlobeAnchorImpl.h"

#include "UnityTransforms.h"

#include <CesiumGeometry/Transforms.h>
#include <CesiumGeospatial/GlobeAnchor.h>
#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/Unity/Mathematics/quaternion.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/Vector3.h>
#include <glm/gtx/quaternion.hpp>

using namespace CesiumGeometry;
using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {

GlobeAnchor createOrUpdateNativeGlobeAnchorFromEcef(
    const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
    const ::DotNet::Unity::Mathematics::double4x4& newLocalToGlobeFixedMatrix) {
  if (!anchor._localToGlobeFixedMatrixIsValid()) {
    // Create a new anchor initialized at the new position, because there is no
    // old one.
    return GlobeAnchor(UnityTransforms::fromUnity(newLocalToGlobeFixedMatrix));
  } else {
    // Create an anchor at the old position and move it to the new one.
    GlobeAnchor cppAnchor(
        UnityTransforms::fromUnity(anchor._localToGlobeFixedMatrix()));
    cppAnchor.setAnchorToFixedTransform(
        UnityTransforms::fromUnity(newLocalToGlobeFixedMatrix),
        anchor.adjustOrientationForGlobeWhenMoving());
    return cppAnchor;
  }
}

GlobeAnchor createOrUpdateNativeGlobeAnchorFromLocal(
    const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
    const glm::dmat4& newModelToLocal) {
  CesiumForUnity::CesiumGeoreference georeference = anchor._georeference();
  assert(georeference != nullptr);
  const LocalHorizontalCoordinateSystem& local =
      georeference.NativeImplementation().getCoordinateSystem(georeference);

  if (!anchor._localToGlobeFixedMatrixIsValid()) {
    // Create a new anchor initialized at the new position, because there is no
    // old one.
    return GlobeAnchor::fromAnchorToLocalTransform(local, newModelToLocal);
  } else {
    // Create an anchor at the old position and move it to the new one.
    GlobeAnchor cppAnchor(
        UnityTransforms::fromUnity(anchor._localToGlobeFixedMatrix()));
    cppAnchor.setAnchorToLocalTransform(
        local,
        newModelToLocal,
        anchor.adjustOrientationForGlobeWhenMoving());
    return cppAnchor;
  }
}

void updateAnchorFromCpp(
    const CesiumForUnity::CesiumGlobeAnchor& anchor,
    const GlobeAnchor& cppAnchor) {
  anchor._localToGlobeFixedMatrix(UnityTransforms::toUnityMathematics(
      cppAnchor.getAnchorToFixedTransform()));
  anchor._localToGlobeFixedMatrixIsValid(true);

  // Update the Unity Transform
  CesiumForUnity::CesiumGeoreference georeference = anchor._georeference();
  if (georeference != nullptr) {
    glm::dmat4 anchorToLocal = cppAnchor.getAnchorToLocalTransform(
        georeference.NativeImplementation().getCoordinateSystem(georeference));

    glm::dvec3 translation;
    glm::dquat rotation;
    glm::dvec3 scale;
    Transforms::computeTranslationRotationScaleFromMatrix(
        anchorToLocal,
        &translation,
        &rotation,
        &scale);

    UnityEngine::Transform transform = anchor.transform();

    transform.localPosition(UnityTransforms::toUnity(translation));
    transform.localRotation(UnityTransforms::toUnity(rotation));
    transform.localScale(UnityTransforms::toUnity(scale));

    anchor._lastLocalToWorld(transform.localToWorldMatrix());
  }
}

LocalHorizontalCoordinateSystem createEastUpNorth(const GlobeAnchor& anchor) {
  glm::dvec3 ecefPosition;
  Transforms::computeTranslationRotationScaleFromMatrix(
      anchor.getAnchorToFixedTransform(),
      &ecefPosition,
      nullptr,
      nullptr);

  return LocalHorizontalCoordinateSystem(
      ecefPosition,
      LocalDirection::East,
      LocalDirection::Up,
      LocalDirection::North,
      1.0);
}

} // namespace

void CesiumGlobeAnchorImpl::SetNewLocalToGlobeFixedMatrix(
    const CesiumForUnity::CesiumGlobeAnchor& anchor,
    const Unity::Mathematics::double4x4& newLocalToGlobeFixedMatrix) {
  // Update with the new ECEF transform, also rotating based on the new position
  // if desired.
  GlobeAnchor cppAnchor = createOrUpdateNativeGlobeAnchorFromEcef(
      anchor,
      newLocalToGlobeFixedMatrix);
  updateAnchorFromCpp(anchor, cppAnchor);
}

void CesiumGlobeAnchorImpl::SetNewLocalToGlobeFixedMatrixFromTransform(
    const CesiumForUnity::CesiumGlobeAnchor& anchor) {
  // Update with the new local transform, also rotating based on the new
  // position if desired.
  UnityEngine::Transform transform = anchor.transform();
  glm::dmat4 modelToLocal = Transforms::createTranslationRotationScaleMatrix(
      UnityTransforms::fromUnity(transform.localPosition()),
      UnityTransforms::fromUnity(transform.localRotation()),
      UnityTransforms::fromUnity(transform.localScale()));
  GlobeAnchor cppAnchor =
      createOrUpdateNativeGlobeAnchorFromLocal(anchor, modelToLocal);
  updateAnchorFromCpp(anchor, cppAnchor);
}

Unity::Mathematics::quaternion
CesiumGlobeAnchorImpl::GetLocalToEastUpNorthRotation(
    const CesiumForUnity::CesiumGlobeAnchor& anchor) {
  GlobeAnchor cppAnchor(
      UnityTransforms::fromUnity(anchor._localToGlobeFixedMatrix()));

  LocalHorizontalCoordinateSystem eastUpNorth = createEastUpNorth(cppAnchor);

  glm::dmat4 modelToEastUpNorth =
      cppAnchor.getAnchorToLocalTransform(eastUpNorth);

  glm::dquat rotationToEastUpNorth;
  Transforms::computeTranslationRotationScaleFromMatrix(
      modelToEastUpNorth,
      nullptr,
      &rotationToEastUpNorth,
      nullptr);
  return UnityTransforms::toUnityMathematics(rotationToEastUpNorth);
}

void CesiumGlobeAnchorImpl::SetLocalToEastUpNorthRotation(
    const ::DotNet::CesiumForUnity::CesiumGlobeAnchor& anchor,
    const ::DotNet::Unity::Mathematics::quaternion& value) {
  GlobeAnchor cppAnchor(
      UnityTransforms::fromUnity(anchor._localToGlobeFixedMatrix()));

  LocalHorizontalCoordinateSystem eastUpNorth = createEastUpNorth(cppAnchor);

  glm::dmat4 modelToEastUpNorth =
      cppAnchor.getAnchorToLocalTransform(eastUpNorth);

  glm::dvec3 translation;
  glm::dvec3 scale;
  Transforms::computeTranslationRotationScaleFromMatrix(
      modelToEastUpNorth,
      &translation,
      nullptr,
      &scale);

  glm::dmat4 newModelToEastUpNorth =
      Transforms::createTranslationRotationScaleMatrix(
          translation,
          UnityTransforms::fromUnity(value),
          scale);

  cppAnchor.setAnchorToLocalTransform(
      eastUpNorth,
      newModelToEastUpNorth,
      false);

  updateAnchorFromCpp(anchor, cppAnchor);
}

} // namespace CesiumForUnityNative
