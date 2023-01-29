#include "CesiumBingMapsRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/BingMapsRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/Ellipsoid.h>

#include <DotNet/CesiumForUnity/BingMapsStyle.h>
#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumBingMapsRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumBingMapsRasterOverlayImpl::CesiumBingMapsRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumBingMapsRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumBingMapsRasterOverlayImpl::~CesiumBingMapsRasterOverlayImpl() {}

void CesiumBingMapsRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumBingMapsRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.bingMapsKey())) {
    // Don't create an overlay with an empty map key.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  std::string mapStyle;
  switch (overlay.mapStyle()) {
  case CesiumForUnity::BingMapsStyle::Aerial:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::AERIAL;
    break;
  case CesiumForUnity::BingMapsStyle::AerialWithLabelsOnDemand:
    mapStyle =
        Cesium3DTilesSelection::BingMapsStyle::AERIAL_WITH_LABELS_ON_DEMAND;
    break;
  case CesiumForUnity::BingMapsStyle::RoadOnDemand:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::ROAD_ON_DEMAND;
    break;
  case CesiumForUnity::BingMapsStyle::CanvasDark:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::CANVAS_DARK;
    break;
  case CesiumForUnity::BingMapsStyle::CanvasLight:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::CANVAS_LIGHT;
    break;
  case CesiumForUnity::BingMapsStyle::CanvasGray:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::CANVAS_GRAY;
    break;
  case CesiumForUnity::BingMapsStyle::OrdnanceSurvey:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::ORDNANCE_SURVEY;
    break;
  case CesiumForUnity::BingMapsStyle::CollinsBart:
    mapStyle = Cesium3DTilesSelection::BingMapsStyle::COLLINS_BART;
    break;
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new BingMapsRasterOverlay(
      overlay.name().ToStlString(),
      "https://dev.virtualearth.net",
      overlay.bingMapsKey().ToStlString(),
      mapStyle,
      "",
      CesiumGeospatial::Ellipsoid::WGS84,
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumBingMapsRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumBingMapsRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay == nullptr)
    return;

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  pTileset->getOverlays().remove(this->_pOverlay);
  this->_pOverlay = nullptr;
}

} // namespace CesiumForUnityNative
