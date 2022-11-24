#pragma once

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/CesiumForUnity/CesiumVector3.h>

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

  DotNet::CesiumForUnity::CesiumVector3
  TransformUnityWorldPositionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::CesiumForUnity::CesiumVector3 unityWorldPosition);
  DotNet::CesiumForUnity::CesiumVector3
  TransformUnityLocalPositionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      const DotNet::UnityEngine::Transform& parent,
      DotNet::CesiumForUnity::CesiumVector3 unityLocalPosition);
  DotNet::CesiumForUnity::CesiumVector3
  TransformEarthCenteredEarthFixedPositionToUnityWorld(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixed);
  DotNet::CesiumForUnity::CesiumVector3
  TransformEarthCenteredEarthFixedPositionToUnityLocal(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      const DotNet::UnityEngine::Transform& parent,
      DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixed);
  DotNet::CesiumForUnity::CesiumVector3
  TransformUnityWorldDirectionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::CesiumForUnity::CesiumVector3 unityWorldDirection);
  DotNet::CesiumForUnity::CesiumVector3
  TransformEarthCenteredEarthFixedDirectionToUnityWorld(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixedDirection);

  const CesiumGeospatial::LocalHorizontalCoordinateSystem&
  getCoordinateSystem() const {
    return this->_coordinateSystem;
  }

private:
  CesiumGeospatial::LocalHorizontalCoordinateSystem _coordinateSystem;
};
} // namespace CesiumForUnityNative
