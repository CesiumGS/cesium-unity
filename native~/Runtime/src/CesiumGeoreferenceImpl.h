#pragma once

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/Unity/Mathematics/double3.h>

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
  void RecalculateOrigin(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference);
  void InitializeOrigin(
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

  const CesiumGeospatial::LocalHorizontalCoordinateSystem&
  getCoordinateSystem() const {
    return this->_coordinateSystem;
  }

private:
  CesiumGeospatial::LocalHorizontalCoordinateSystem _coordinateSystem;
};
} // namespace CesiumForUnityNative
