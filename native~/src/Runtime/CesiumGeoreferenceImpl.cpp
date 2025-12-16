#include "CesiumGeoreferenceImpl.h"

#include "UnityTransforms.h"

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumGeoreferenceOriginAuthority.h>
#include <DotNet/CesiumForUnity/CesiumGeoreferenceOriginPlacement.h>
#include <DotNet/CesiumForUnity/CesiumGlobeAnchor.h>
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
  double scale = 1.0 / georeference.scale();
  if (georeference.originPlacement() ==
      DotNet::CesiumForUnity::CesiumGeoreferenceOriginPlacement::TrueOrigin) {
    // In True Origin mode, we want a coordinate system that:
    // 1. Is at the origin,
    // 2. Inverts Y to create a left-handed coordinate system,
    // 3. Converts from Z-up to Y-up, and
    // 4. Uses the georeference's scale
    glm::dmat4 localToEcef(
        glm::dvec4(scale, 0.0, 0.0, 0.0),
        glm::dvec4(0.0, 0.0, scale, 0.0),
        glm::dvec4(0.0, scale, 0.0, 0.0),
        glm::dvec4(0.0, 0.0, 0.0, 1.0));
    return LocalHorizontalCoordinateSystem(localToEcef);
  }

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
        LocalDirection::North,
        scale,
        georeference.ellipsoid().NativeImplementation().GetEllipsoid());
  } else {
    return LocalHorizontalCoordinateSystem(
        glm::dvec3(
            georeference.ecefX(),
            georeference.ecefY(),
            georeference.ecefZ()),
        LocalDirection::East,
        LocalDirection::Up,
        LocalDirection::North,
        scale,
        georeference.ellipsoid().NativeImplementation().GetEllipsoid());
  }
}

} // namespace

CesiumGeoreferenceImpl::CesiumGeoreferenceImpl(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _coordinateSystem() {}

CesiumGeoreferenceImpl::~CesiumGeoreferenceImpl() {}

::DotNet::Unity::Mathematics::double4x4
CesiumGeoreferenceImpl::ComputeLocalToEarthCenteredEarthFixedTransformation(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  this->_coordinateSystem = createCoordinateSystem(georeference);
  return UnityTransforms::toUnityMathematics(
      this->_coordinateSystem->getLocalToEcefTransformation());
}

const CesiumGeospatial::LocalHorizontalCoordinateSystem&
CesiumGeoreferenceImpl::getCoordinateSystem(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  if (!this->_coordinateSystem) {
    this->ComputeLocalToEarthCenteredEarthFixedTransformation(georeference);
  }
  return *this->_coordinateSystem;
}
