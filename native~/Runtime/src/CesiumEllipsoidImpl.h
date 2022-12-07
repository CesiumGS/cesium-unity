#pragma once

#include <DotNet/System/Nullable1.h>
#include <DotNet/Unity/Mathematics/double3.h>

namespace DotNet::CesiumForUnity {
class CesiumEllipsoid;
}

namespace CesiumForUnityNative {

class CesiumEllipsoidImpl {
public:
  static DotNet::System::Nullable1<DotNet::Unity::Mathematics::double3>
      ScaleToGeodeticSurface(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  static DotNet::Unity::Mathematics::double3 GeodeticSurfaceNormal(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
};

} // namespace CesiumForUnityNative
