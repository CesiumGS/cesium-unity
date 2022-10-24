#pragma once

#include <DotNet/CesiumForUnity/CesiumVector3.h>

namespace DotNet::CesiumForUnity {
class CesiumTransforms;
}

namespace CesiumForUnityNative {

class CesiumTransformsImpl {
public:
  static DotNet::CesiumForUnity::CesiumVector3 LongitudeLatitudeHeightToEarthCenteredEarthFixed(DotNet::CesiumForUnity::CesiumVector3 longitudeLatitudeHeight);
  static DotNet::CesiumForUnity::CesiumVector3 EarthCenteredEarthFixedToLongitudeLatitudeHeight(DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixed);
};

} // namespace CesiumForUnityNative
