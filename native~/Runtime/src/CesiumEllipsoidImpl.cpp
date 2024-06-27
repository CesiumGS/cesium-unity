#include "CesiumEllipsoidImpl.h"

#include "CesiumEllipsoidFunctions.h"

#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/UnityEngine/Debug.h>
#include <DotNet/System/Object.h>

#include <limits>

using namespace DotNet::Unity::Mathematics;
using namespace DotNet::CesiumForUnity;

static constexpr double MinRadiiValue = std::numeric_limits<double>::epsilon();

CesiumForUnityNative::CesiumEllipsoidImpl::CesiumEllipsoidImpl(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid)
    : _ellipsoid(1.0, 1.0, 1.0) {
  this->SetRadii(unityEllipsoid, unityEllipsoid.radii());
}

CesiumForUnityNative::CesiumEllipsoidImpl::~CesiumEllipsoidImpl() {}

DotNet::Unity::Mathematics::double3
CesiumForUnityNative::CesiumEllipsoidImpl::GetRadii(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid) {
  return CesiumEllipsoidFunctions::GetRadii(this->_ellipsoid);
}

void CesiumForUnityNative::CesiumEllipsoidImpl::SetRadii(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
    const DotNet::Unity::Mathematics::double3& newRadii) {
  if (newRadii.x < MinRadiiValue || newRadii.y < MinRadiiValue ||
      newRadii.z < MinRadiiValue) {
    DotNet::UnityEngine::Debug::LogError(
        DotNet::System::String("Ellipsoid radii must be greater than 0 - "
                               "clamping to minimum value to avoid crashes."));
  }

  double3 clampedRadii = double3::Construct(
      std::max(newRadii.x, MinRadiiValue),
      std::max(newRadii.y, MinRadiiValue),
      std::max(newRadii.z, MinRadiiValue));

  this->_ellipsoid = CesiumGeospatial::Ellipsoid(
      clampedRadii.x,
      clampedRadii.y,
      clampedRadii.z);
  unityEllipsoid.radii(clampedRadii);
}

std::optional<DotNet::Unity::Mathematics::double3>
CesiumForUnityNative::CesiumEllipsoidImpl::ScaleToGeodeticSurface(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
    const DotNet::Unity::Mathematics::double3&
        ellipsoidCenteredEllipsoidFixed) {
  return CesiumEllipsoidFunctions::ScaleToGeodeticSurface(
      this->_ellipsoid,
      ellipsoidCenteredEllipsoidFixed);
}

DotNet::Unity::Mathematics::double3
CesiumForUnityNative::CesiumEllipsoidImpl::GeodeticSurfaceNormal(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
    const DotNet::Unity::Mathematics::double3&
        ellipsoidCenteredEllipsoidFixed) {
  return CesiumEllipsoidFunctions::GeodeticSurfaceNormal(
      this->_ellipsoid,
      ellipsoidCenteredEllipsoidFixed);
}

DotNet::Unity::Mathematics::double3 CesiumForUnityNative::CesiumEllipsoidImpl::
    LongitudeLatitudeHeightToCenteredFixed(
        const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
        const DotNet::Unity::Mathematics::double3& longitudeLatitudeHeight) {
  return CesiumEllipsoidFunctions::LongitudeLatitudeHeightToCenteredFixed(
      this->_ellipsoid,
      longitudeLatitudeHeight);
}

DotNet::Unity::Mathematics::double3 CesiumForUnityNative::CesiumEllipsoidImpl::
    CenteredFixedToLongitudeLatitudeHeight(
        const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
        const DotNet::Unity::Mathematics::double3&
            ellipsoidCenteredEllipsoidFixed) {
  return CesiumEllipsoidFunctions::CenteredFixedToLongitudeLatitudeHeight(
      this->_ellipsoid,
      ellipsoidCenteredEllipsoidFixed);
}
