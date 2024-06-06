#include "CesiumEllipsoidImpl.h"

#include "CesiumEllipsoidFunctions.h"

#include <DotNet/CesiumForUnity/CesiumEllipsoid.h>
#include <DotNet/Unity/Mathematics/double3.h>

using namespace DotNet::Unity::Mathematics;
using namespace DotNet::CesiumForUnity;

CesiumForUnityNative::CesiumEllipsoidImpl::CesiumEllipsoidImpl(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid)
    : _ellipsoid(
          unityEllipsoid.radii().x,
          unityEllipsoid.radii().y,
          unityEllipsoid.radii().z) {}

CesiumForUnityNative::CesiumEllipsoidImpl::~CesiumEllipsoidImpl() {}

DotNet::Unity::Mathematics::double3
CesiumForUnityNative::CesiumEllipsoidImpl::GetRadii(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid) {
  return CesiumEllipsoidFunctions::GetRadii(this->_ellipsoid);
}

void CesiumForUnityNative::CesiumEllipsoidImpl::SetRadii(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
    const DotNet::Unity::Mathematics::double3& newRadii) {
  this->_ellipsoid =
      CesiumGeospatial::Ellipsoid(newRadii.x, newRadii.y, newRadii.z);
}

std::optional<DotNet::Unity::Mathematics::double3>
CesiumForUnityNative::CesiumEllipsoidImpl::ScaleToGeodeticSurface(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
    const DotNet::Unity::Mathematics::double3& ellipsoidCenteredEllipsoidFixed) {
  return CesiumEllipsoidFunctions::ScaleToGeodeticSurface(
      this->_ellipsoid,
      ellipsoidCenteredEllipsoidFixed);
}

DotNet::Unity::Mathematics::double3
CesiumForUnityNative::CesiumEllipsoidImpl::GeodeticSurfaceNormal(
    const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
    const DotNet::Unity::Mathematics::double3& ellipsoidCenteredEllipsoidFixed) {
  return CesiumEllipsoidFunctions::GeodeticSurfaceNormal(
      this->_ellipsoid,
      ellipsoidCenteredEllipsoidFixed);
}

DotNet::Unity::Mathematics::double3 CesiumForUnityNative::CesiumEllipsoidImpl::
    LongitudeLatitudeHeightToEllipsoidCenteredEllipsoidFixed(
        const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
        const DotNet::Unity::Mathematics::double3& longitudeLatitudeHeight) {
  return CesiumEllipsoidFunctions::
      LongitudeLatitudeHeightToEllipsoidCenteredEllipsoidFixed(
          this->_ellipsoid,
          longitudeLatitudeHeight);
}

DotNet::Unity::Mathematics::double3 CesiumForUnityNative::CesiumEllipsoidImpl::
    EllipsoidCenteredEllipsoidFixedToLongitudeLatitudeHeight(
        const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
        const DotNet::Unity::Mathematics::double3& ellipsoidCenteredEllipsoidFixed) {
  return CesiumEllipsoidFunctions::
      EllipsoidCenteredEllipsoidFixedToLongitudeLatitudeHeight(
          this->_ellipsoid,
          ellipsoidCenteredEllipsoidFixed);
}
