#include "CesiumPolygonRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/RasterizedPolygonsTileExcluder.h>
#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/CartographicPolygon.h>
#include <CesiumRasterOverlays/RasterizedPolygonsOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumCartographicPolygon.h>
#include <DotNet/CesiumForUnity/CesiumPolygonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/String.h>
#include <DotNet/Unity/Mathematics/double2.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Matrix4x4.h>
#include <DotNet/UnityEngine/Transform.h>
#include <glm/glm.hpp>

using namespace DotNet;
using namespace DotNet::System::Collections::Generic;
using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;

namespace CesiumForUnityNative {

CesiumPolygonRasterOverlayImpl::CesiumPolygonRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumPolygonRasterOverlay& overlay)
    : _pExcluder(nullptr) {}

CesiumPolygonRasterOverlayImpl::~CesiumPolygonRasterOverlayImpl() {}

/*static*/ CesiumGeospatial::CartographicPolygon
CesiumPolygonRasterOverlayImpl::CreateCartographicPolygon(
    DotNet::System::Collections::Generic::List1<
        DotNet::Unity::Mathematics::double2> cartographicPoints) {
  const int32_t pointsCount = cartographicPoints.Count();
  if (pointsCount < 3) {
    return CesiumGeospatial::CartographicPolygon({});
  }

  std::vector<glm::dvec2> polygonPoints(pointsCount);

  // The spline points should be located in the tileset *exactly where they
  // appear to be*. The way we do that is by getting their world position, and
  // then transforming that world position to a Cesium3DTileset local position.
  // That way if the tileset is transformed relative to the globe, the polygon
  // will still affect the tileset where the user thinks it should.

  for (int32_t i = 0; i < pointsCount; ++i) {
    Unity::Mathematics::double2 point = cartographicPoints[i];
    polygonPoints[i] = glm::dvec2(glm::radians(point.x), glm::radians(point.y));
  }

  return CesiumGeospatial::CartographicPolygon(polygonPoints);
}

void CesiumPolygonRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumPolygonRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  List1<CesiumForUnity::CesiumCartographicPolygon> unityPolygons =
      overlay.polygons();
  if (unityPolygons == nullptr) {
    return;
  }

  UnityEngine::Matrix4x4 worldToTileset =
      tileset.gameObject().transform().worldToLocalMatrix();

  const int32_t polygonCount = unityPolygons.Count();
  std::vector<CesiumGeospatial::CartographicPolygon> nativePolygons;
  nativePolygons.reserve(polygonCount);

  for (int32_t i = 0; i < polygonCount; i++) {
    CesiumForUnity::CesiumCartographicPolygon unityPolygon = unityPolygons[i];
    if (unityPolygon == nullptr) {
      continue;
    }

    if (!unityPolygon.enabled() ||
        !unityPolygon.gameObject().activeInHierarchy()) {
      continue;
    }

    CesiumGeospatial::CartographicPolygon nativePolygon =
        CreateCartographicPolygon(
            unityPolygon.GetCartographicPoints(worldToTileset));
    nativePolygons.emplace_back(std::move(nativePolygon));
  }

  this->_pOverlay = new RasterizedPolygonsOverlay(
      overlay.materialKey().ToStlString(),
      nativePolygons,
      overlay.invertSelection(),
      CesiumGeospatial::Ellipsoid::WGS84,
      CesiumGeospatial::GeographicProjection(),
      options);

  pTileset->getOverlays().add(this->_pOverlay);

  // If this overlay is used for culling, add it as an excluder too for
  // efficiency.
  if (overlay.excludeSelectedTiles()) {
    assert(this->_pExcluder == nullptr);
    this->_pExcluder =
        std::make_shared<RasterizedPolygonsTileExcluder>(this->_pOverlay);
    pTileset->getOptions().excluders.push_back(this->_pExcluder);
  }
}

void CesiumPolygonRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumPolygonRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  if (this->_pOverlay) {
    pTileset->getOverlays().remove(this->_pOverlay);
    this->_pOverlay = nullptr;
  }

  if (this->_pExcluder) {
    auto& excluders = pTileset->getOptions().excluders;
    auto it = std::find(excluders.begin(), excluders.end(), this->_pExcluder);
    if (it != excluders.end()) {
      excluders.erase(it);
    }

    this->_pExcluder = nullptr;
  }
}

} // namespace CesiumForUnityNative
