#pragma once

#include "CesiumImpl.h"

#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObjectTypes.h>

#include <glm/vec3.hpp>

#include <memory>
#include <vector>

namespace DotNet::CesiumForUnity {
class CesiumGeoJsonLineString;
class CesiumGeoJsonPolygon;
} // namespace DotNet::CesiumForUnity

namespace DotNet::System {
template <typename T> class Array1;
} // namespace DotNet::System

namespace CesiumForUnityNative {

class CesiumGeoJsonPolygonImpl : public CesiumImpl<CesiumGeoJsonPolygonImpl> {
public:
  CesiumGeoJsonPolygonImpl(
      const DotNet::CesiumForUnity::CesiumGeoJsonPolygon& polygon);
  ~CesiumGeoJsonPolygonImpl();

  void setNativePolygonInDocument(
      std::shared_ptr<CesiumVectorData::GeoJsonDocument> pDocument,
      std::vector<std::vector<glm::dvec3>>* pRings);

  DotNet::System::Array1<DotNet::CesiumForUnity::CesiumGeoJsonLineString>
  GetPolygonRings(
      const DotNet::CesiumForUnity::CesiumGeoJsonPolygon& polygon);

  void DisposeNative(
      const DotNet::CesiumForUnity::CesiumGeoJsonPolygon& polygon);

private:
  std::shared_ptr<CesiumVectorData::GeoJsonDocument> _pDocument;
  std::vector<std::vector<glm::dvec3>>* _pRings;
};

} // namespace CesiumForUnityNative
