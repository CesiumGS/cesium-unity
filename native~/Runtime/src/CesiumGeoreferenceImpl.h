#pragma once

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/Unity/Mathematics/double3.h>
#include <DotNet/Unity/Mathematics/double3x3.h>

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

  void JustBeforeDelete(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);
  std::optional<DotNet::Unity::Mathematics::double3x3> RecalculateOrigin(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

  DotNet::Unity::Mathematics::double3
  TransformUnityPositionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 unityPosition);
  DotNet::Unity::Mathematics::double3
  TransformEarthCenteredEarthFixedPositionToUnity(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  DotNet::Unity::Mathematics::double3
  TransformUnityDirectionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 unityDirection);
  DotNet::Unity::Mathematics::double3
  TransformEarthCenteredEarthFixedDirectionToUnity(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixedDirection);

  const CesiumGeospatial::LocalHorizontalCoordinateSystem& getCoordinateSystem(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

private:
  std::optional<CesiumGeospatial::LocalHorizontalCoordinateSystem>
      _coordinateSystem;
};
} // namespace CesiumForUnityNative
