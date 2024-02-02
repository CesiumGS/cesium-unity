#pragma once

#include "CesiumCartographicPolygonImpl.h"

#include <DotNet/CesiumForUnity/CesiumCartographicPolygon.h>
#include <DotNet/Unity/Mathematics/double2.h>
#include <glm/glm.hpp>

using namespace DotNet;
using namespace DotNet::Unity::Mathematics;

namespace CesiumForUnityNative {

CesiumCartographicPolygonImpl::CesiumCartographicPolygonImpl(
    const DotNet::CesiumForUnity::CesiumCartographicPolygon& polygon) {}
CesiumCartographicPolygonImpl ::~CesiumCartographicPolygonImpl() {}

void CesiumCartographicPolygonImpl::UpdatePolygon(
    const DotNet::CesiumForUnity::CesiumCartographicPolygon& polygon,
    const DotNet::System::Array1<DotNet::Unity::Mathematics::double2>&
        cartographicPoints) {
  int32_t pointsCount = cartographicPoints.Length();
  if (pointsCount < 3) {
    this->_polygon = CesiumGeospatial::CartographicPolygon({});
    return;
  }

  std::vector<glm::dvec2> polygonPoints(pointsCount);

  // The spline points should be located in the tileset *exactly where they
  // appear to be*. The way we do that is by getting their world position, and
  // then transforming that world position to a Cesium3DTileset local position.
  // That way if the tileset is transformed relative to the globe, the polygon
  // will still affect the tileset where the user thinks it should.

  for (int32_t i = 0; i < pointsCount; ++i) {
    double2 point = cartographicPoints[i];
    polygonPoints[i] = glm::dvec2(glm::radians(point.x), glm::radians(point.y));
  }

  this->_polygon = CesiumGeospatial::CartographicPolygon(polygonPoints);
}

} // namespace CesiumForUnityNative
