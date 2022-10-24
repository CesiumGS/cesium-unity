#pragma once

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

#include <DotNet/CesiumForUnity/CesiumVector3.h>

namespace DotNet::CesiumForUnity {
class CesiumGeoreference;
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
  void
  OnValidate(const DotNet::CesiumForUnity::CesiumGeoreference& georeference);
  void Awake(const DotNet::CesiumForUnity::CesiumGeoreference& georeference);

  DotNet::CesiumForUnity::CesiumVector3
  TransformUnityWorldPositionToEarthCenteredEarthFixed(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::CesiumForUnity::CesiumVector3 unityWorldPosition);
  DotNet::CesiumForUnity::CesiumVector3
  TransformEarthCenteredEarthFixedPositionToUnityWorld(
      const DotNet::CesiumForUnity::CesiumGeoreference& georeference,
      DotNet::CesiumForUnity::CesiumVector3 earthCenteredEarthFixed);

  const CesiumGeospatial::LocalHorizontalCoordinateSystem&
  getCoordinateSystem() const {
    return this->_coordinateSystem;
  }

private:
  CesiumGeospatial::LocalHorizontalCoordinateSystem _coordinateSystem;
};
} // namespace CesiumForUnityNative
