#include "CesiumGeoreferenceImpl.h"

#include <CesiumGeospatial/LocalHorizontalCoordinateSystem.h>
#include <CesiumUtility/Math.h>

#include <DotNet/CesiumForUnity/CesiumGeoreference.h>

using namespace CesiumForUnityNative;
using namespace CesiumGeospatial;
using namespace CesiumUtility;

namespace {

LocalHorizontalCoordinateSystem createCoordinateSystem(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  return LocalHorizontalCoordinateSystem(
      Cartographic::fromDegrees(
          georeference.longitude(),
          georeference.latitude(),
          georeference.height()),
      LocalDirection::East,
      LocalDirection::Up,
      LocalDirection::North);
}

} // namespace

CesiumGeoreferenceImpl::CesiumGeoreferenceImpl(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference)
    : _coordinateSystem(createCoordinateSystem(georeference)) {}

CesiumGeoreferenceImpl::~CesiumGeoreferenceImpl() {}

void CesiumGeoreferenceImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {}

void CesiumGeoreferenceImpl::RecalculateOrigin(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  this->_coordinateSystem = createCoordinateSystem(georeference);
}

void CesiumGeoreferenceImpl::OnValidate(
    const DotNet::CesiumForUnity::CesiumGeoreference& georeference) {
  georeference.UpdateOrigin();
}
