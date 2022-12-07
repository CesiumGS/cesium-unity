#pragma once

#include <DotNet/Unity/Mathematics/double3.h>

namespace DotNet::CesiumForUnity {
class CesiumTransforms;
}

namespace CesiumForUnityNative {

class CesiumTransformsImpl {
public:
  static DotNet::Unity::Mathematics::double3
  LongitudeLatitudeHeightToEarthCenteredEarthFixed(
      DotNet::Unity::Mathematics::double3 longitudeLatitudeHeight);
  static DotNet::Unity::Mathematics::double3
  EarthCenteredEarthFixedToLongitudeLatitudeHeight(
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
};

} // namespace CesiumForUnityNative
