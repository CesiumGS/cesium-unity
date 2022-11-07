#include "CesiumGlobeAnchorImpl.h"

#include "UnityTransforms.h"

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
#include <DotNet/CesiumForUnity/CesiumVector3.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Quaternion.h>
#include <DotNet/UnityEngine/Transform.h>
#include <glm/gtx/quaternion.hpp>

using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

/*static*/ void CesiumGlobeAnchorImpl::AdjustOrientation(
    const CesiumForUnity::CesiumGlobeAnchor& globeAnchor,
    const CesiumForUnity::CesiumVector3& oldPositionEcef,
    const CesiumForUnity::CesiumVector3& newPositionEcef) {
  CesiumForUnity::CesiumGeoreference georeference =
      globeAnchor.gameObject()
          .GetComponentInParent<CesiumForUnity::CesiumGeoreference>();
  if (georeference == nullptr)
    return;

  const LocalHorizontalCoordinateSystem& coordinateSystem =
      georeference.NativeImplementation().getCoordinateSystem();

  // Find the rotation from the old up to the new up.
  glm::dvec3 oldNormal = coordinateSystem.ecefDirectionToLocal(
      Ellipsoid::WGS84.geodeticSurfaceNormal(
          glm::dvec3(oldPositionEcef.x, oldPositionEcef.y, oldPositionEcef.z)));
  glm::dvec3 newNormal = coordinateSystem.ecefDirectionToLocal(
      Ellipsoid::WGS84.geodeticSurfaceNormal(
          glm::dvec3(newPositionEcef.x, newPositionEcef.y, newPositionEcef.z)));

  glm::dquat deltaRotation = glm::rotation(oldNormal, newNormal);

  // Rotate the game object by that same rotation
  UnityEngine::Transform transform = globeAnchor.transform();
  glm::dquat oldRotation = UnityTransforms::fromUnity(transform.rotation());
  glm::dquat newRotation = deltaRotation * oldRotation;
  transform.rotation(UnityTransforms::toUnity(newRotation));
}

} // namespace CesiumForUnityNative
