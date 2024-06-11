#pragma once

#include <CesiumGeospatial/Ellipsoid.h>

#include <DotNet/Unity/Mathematics/double3.h>

#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumEllipsoid;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class CesiumEllipsoidImpl {
public:
  CesiumEllipsoidImpl(
      const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid);
  ~CesiumEllipsoidImpl();

  DotNet::Unity::Mathematics::double3
  GetRadii(const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid);

  void SetRadii(
      const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
      const DotNet::Unity::Mathematics::double3& newRadii);

  std::optional<DotNet::Unity::Mathematics::double3> ScaleToGeodeticSurface(
      const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
      const DotNet::Unity::Mathematics::double3&
          ellipsoidCenteredEllipsoidFixed);

  DotNet::Unity::Mathematics::double3 GeodeticSurfaceNormal(
      const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
      const DotNet::Unity::Mathematics::double3&
          ellipsoidCenteredEllipsoidFixed);

  DotNet::Unity::Mathematics::double3
  LongitudeLatitudeHeightToEllipsoidCenteredEllipsoidFixed(
      const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
      const DotNet::Unity::Mathematics::double3& longitudeLatitudeHeight);

  DotNet::Unity::Mathematics::double3
  EllipsoidCenteredEllipsoidFixedToLongitudeLatitudeHeight(
      const DotNet::CesiumForUnity::CesiumEllipsoid& unityEllipsoid,
      const DotNet::Unity::Mathematics::double3&
          ellipsoidCenteredEllipsoidFixed);

  const CesiumGeospatial::Ellipsoid& GetEllipsoid() const {
    return this->_ellipsoid;
  }

private:
  CesiumGeospatial::Ellipsoid _ellipsoid;
};

} // namespace CesiumForUnityNative
