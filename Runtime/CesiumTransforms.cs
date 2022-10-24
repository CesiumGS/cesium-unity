using Reinterop;
using System;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumTransformsImpl", "CesiumTransformsImpl.h")]
    public static partial class CesiumTransforms
    {
        public static partial CesiumVector3 LongitudeLatitudeHeightToEarthCenteredEarthFixed(CesiumVector3 longitudeLatitudeHeight);

        public static partial CesiumVector3 EarthCenteredEarthFixedToLongitudeLatitudeHeight(CesiumVector3 earthCenteredEarthFixed);
    }
}
