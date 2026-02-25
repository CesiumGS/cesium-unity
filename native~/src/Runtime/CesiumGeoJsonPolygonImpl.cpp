#include "CesiumGeoJsonPolygonImpl.h"

#include <DotNet/CesiumForUnity/CesiumGeoJsonLineString.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonPolygon.h>
#include <DotNet/System/Array1.h>
#include <DotNet/Unity/Mathematics/double3.h>

#include <cstdint>

using namespace DotNet;
using namespace CesiumVectorData;

namespace CesiumForUnityNative {

CesiumGeoJsonPolygonImpl::CesiumGeoJsonPolygonImpl(
    const CesiumForUnity::CesiumGeoJsonPolygon& polygon)
    : _pDocument(nullptr), _pRings(nullptr) {}

CesiumGeoJsonPolygonImpl::~CesiumGeoJsonPolygonImpl() {}

void CesiumGeoJsonPolygonImpl::setNativePolygonInDocument(
    std::shared_ptr<GeoJsonDocument> pDocument,
    std::vector<std::vector<glm::dvec3>>* pRings) {
  _pDocument = std::move(pDocument);
  _pRings = pRings;
}

System::Array1<CesiumForUnity::CesiumGeoJsonLineString>
CesiumGeoJsonPolygonImpl::GetPolygonRings(
    const CesiumForUnity::CesiumGeoJsonPolygon& polygon) {
  if (!_pRings) {
    return System::Array1<CesiumForUnity::CesiumGeoJsonLineString>(nullptr);
  }

  std::int32_t ringCount = static_cast<std::int32_t>(_pRings->size());
  System::Array1<CesiumForUnity::CesiumGeoJsonLineString> result(ringCount);

  for (std::int32_t i = 0; i < ringCount; ++i) {
    const std::vector<glm::dvec3>& ring = (*_pRings)[static_cast<size_t>(i)];
    std::int32_t pointCount = static_cast<std::int32_t>(ring.size());

    System::Array1<Unity::Mathematics::double3> points(pointCount);
    for (std::int32_t j = 0; j < pointCount; ++j) {
      const glm::dvec3& coord = ring[static_cast<size_t>(j)];
      points.Item(j, Unity::Mathematics::double3{coord.x, coord.y, coord.z});
    }

    CesiumForUnity::CesiumGeoJsonLineString lineString;
    lineString.Points(points);
    result.Item(i, lineString);
  }

  return result;
}

void CesiumGeoJsonPolygonImpl::DisposeNative(
    const CesiumForUnity::CesiumGeoJsonPolygon& polygon) {
  _pDocument = nullptr;
  _pRings = nullptr;
}

} // namespace CesiumForUnityNative
