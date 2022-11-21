#pragma once

#include <DotNet/CesiumForUnity/CesiumVector3.h>

namespace DotNet::CesiumForUnity {
class CesiumTransforms;
}

namespace CesiumForUnityNative {

class CesiumTransformsImpl {
public:
  static DotNet::CesiumForUnity::CesiumVector3
  LongitudeLatitudeHeightToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumVector3& longitudeLatitudeHeight);
  static DotNet::CesiumForUnity::CesiumVector3
  EarthCenteredEarthFixedToLongitudeLatitudeHeight(
      const DotNet::CesiumForUnity::CesiumVector3& earthCenteredEarthFixed);
  static DotNet::CesiumForUnity::CesiumVector3
  ScaleCartesianToEllipsoidGeodeticSurface(
      const DotNet::CesiumForUnity::CesiumVector3& cartesian);
};

} // namespace CesiumForUnityNative
