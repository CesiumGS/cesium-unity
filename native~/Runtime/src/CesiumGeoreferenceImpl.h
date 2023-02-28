#pragma once

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/Unity/Mathematics/double4x4.h>

#include <optional>

namespace DotNet::CesiumForUnity {
class CesiumGeoreference;
}

namespace DotNet::UnityEngine {
class Transform;
}

namespace CesiumForUnityNative {
class CesiumGeoreferenceImpl {
public:
  CesiumGeoreferenceImpl(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);
  ~CesiumGeoreferenceImpl();

  ::DotNet::Unity::Mathematics::double4x4
  ComputeLocalToEarthCenteredEarthFixedTransformation(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

  const CesiumGeospatial::LocalHorizontalCoordinateSystem& getCoordinateSystem(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

private:
  std::optional<CesiumGeospatial::LocalHorizontalCoordinateSystem>
      _coordinateSystem;
};
} // namespace CesiumForUnityNative
