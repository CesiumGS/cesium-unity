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
  TransformUnityWorldPositionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 unityWorldPosition);
  DotNet::Unity::Mathematics::double3
  TransformUnityLocalPositionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      const DotNet::UnityEngine::Transform& parent,
      DotNet::Unity::Mathematics::double3 unityLocalPosition);
  DotNet::Unity::Mathematics::double3
  TransformEarthCenteredEarthFixedPositionToUnityWorld(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  DotNet::Unity::Mathematics::double3
  TransformEarthCenteredEarthFixedPositionToUnityLocal(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      const DotNet::UnityEngine::Transform& parent,
      DotNet::Unity::Mathematics::double3 earthCenteredEarthFixed);
  DotNet::Unity::Mathematics::double3
  TransformUnityWorldDirectionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::Unity::Mathematics::double3 unityWorldDirection);
  DotNet::Unity::Mathematics::double3
  TransformEarthCenteredEarthFixedDirectionToUnityWorld(
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
