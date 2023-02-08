#include "CesiumGlobeAnchorImpl.h"

#include "UnityTransforms.h"

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Transform.h>
#include <glm/gtx/quaternion.hpp>

using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

/*static*/ ::DotNet::Unity::Mathematics::double4x4
CesiumGlobeAnchorImpl::AdjustOrientation(
    const CesiumForUnity::CesiumGlobeAnchor& globeAnchor,
    const Unity::Mathematics::double3& oldPositionEcef,
    const Unity::Mathematics::double3& newPositionEcef,
    const ::DotNet::Unity::Mathematics::double4x4& newModelToEcef) {
  // Find the rotation from the old up to the new up.
  glm::dvec3 oldNormal = Ellipsoid::WGS84.geodeticSurfaceNormal(
      glm::dvec3(oldPositionEcef.x, oldPositionEcef.y, oldPositionEcef.z));
  glm::dvec3 newNormal = Ellipsoid::WGS84.geodeticSurfaceNormal(
      glm::dvec3(newPositionEcef.x, newPositionEcef.y, newPositionEcef.z));

  glm::dquat deltaRotation = glm::rotation(oldNormal, newNormal);
  glm::dmat3 oldRotationAndScale =
      UnityTransforms::fromUnity3x3(newModelToEcef);
  glm::dmat3 newRotationAndScale =
      glm::mat3_cast(deltaRotation) * oldRotationAndScale;
  return UnityTransforms::toUnityMathematics(glm::dmat4(
      glm::dvec4(newRotationAndScale[0], 0.0),
      glm::dvec4(newRotationAndScale[1], 0.0),
      glm::dvec4(newRotationAndScale[2], 0.0),
      UnityTransforms::fromUnity(newModelToEcef.c3)));
}

} // namespace CesiumForUnityNative
