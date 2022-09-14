#pragma once

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>

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

  const CesiumGeospatial::LocalHorizontalCoordinateSystem&
  getCoordinateSystem() const {
    return this->_coordinateSystem;
  }

private:
  CesiumGeospatial::LocalHorizontalCoordinateSystem _coordinateSystem;
};
} // namespace CesiumForUnityNative
