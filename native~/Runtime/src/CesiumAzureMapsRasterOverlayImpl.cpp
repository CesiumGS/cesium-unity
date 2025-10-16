#include "CesiumAzureMapsRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumRasterOverlays/AzureMapsRasterOverlay.h>

#include <DotNet/CesiumForUnity/AzureMapsTilesetId.h>
#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumAzureMapsRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {
std::string getTilesetId(::DotNet::CesiumForUnity::AzureMapsTilesetId tilesetId) {
  switch (tilesetId) {
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::BaseDarkGrey:
    return AzureMapsTilesetId::baseDarkGrey;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::BaseLabelsRoad:
    return AzureMapsTilesetId::baseLabelsRoad;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::BaseLabelsDarkGrey:
    return AzureMapsTilesetId::baseLabelsDarkGrey;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::BaseHybridRoad:
    return AzureMapsTilesetId::baseHybridRoad;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::BaseHybridDarkGrey:
    return AzureMapsTilesetId::baseHybridDarkGrey;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::Imagery:
    return AzureMapsTilesetId::imagery;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::Terra:
    return AzureMapsTilesetId::terra;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::WeatherRadar:
    return AzureMapsTilesetId::weatherRadar;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::WeatherInfrared:
    return AzureMapsTilesetId::weatherInfrared;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::TrafficAbsolute:
    return AzureMapsTilesetId::trafficAbsolute;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::TrafficRelativeMain:
    return AzureMapsTilesetId::trafficRelativeMain;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::TrafficRelativeDark:
    return AzureMapsTilesetId::trafficRelativeDark;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::TrafficDelay:
    return AzureMapsTilesetId::trafficDelay;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::TrafficReduced:
    return AzureMapsTilesetId::trafficReduced;
  case ::DotNet::CesiumForUnity::AzureMapsTilesetId::BaseRoad:
  default:
    return AzureMapsTilesetId::baseRoad;
  }
}
} // namespace

CesiumAzureMapsRasterOverlayImpl::CesiumAzureMapsRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumAzureMapsRasterOverlayImpl::~CesiumAzureMapsRasterOverlayImpl() {}

void CesiumAzureMapsRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.key())) {
    // Don't create an overlay with an empty map key.
    return;
  }



/*
  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;
  std::string mapStyle;
  switch (overlay.mapStyle()) {
  case CesiumForUnity::AzureMapsStyle::Aerial:
    mapStyle = AzureMapsStyle::AERIAL;
    break;
  case CesiumForUnity::AzureMapsStyle::AerialWithLabelsOnDemand:
    mapStyle = AzureMapsStyle::AERIAL_WITH_LABELS_ON_DEMAND;
    break;
  case CesiumForUnity::AzureMapsStyle::RoadOnDemand:
    mapStyle = AzureMapsStyle::ROAD_ON_DEMAND;
    break;
  case CesiumForUnity::AzureMapsStyle::CanvasDark:
    mapStyle = AzureMapsStyle::CANVAS_DARK;
    break;
  case CesiumForUnity::AzureMapsStyle::CanvasLight:
    mapStyle = AzureMapsStyle::CANVAS_LIGHT;
    break;
  case CesiumForUnity::AzureMapsStyle::CanvasGray:
    mapStyle = AzureMapsStyle::CANVAS_GRAY;
    break;
  case CesiumForUnity::AzureMapsStyle::OrdnanceSurvey:
    mapStyle = AzureMapsStyle::ORDNANCE_SURVEY;
    break;
  case CesiumForUnity::AzureMapsStyle::CollinsBart:
    mapStyle = AzureMapsStyle::COLLINS_BART;
    break;
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new AzureMapsRasterOverlay(
      overlay.materialKey().ToStlString(),
      "https://dev.virtualearth.net",
      overlay.AzureMapsKey().ToStlString(),
      mapStyle,
      "",
      options);

  pTileset->getOverlays().add(this->_pOverlay);
  */
}

void CesiumAzureMapsRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay == nullptr)
    return;

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  CesiumUtility::IntrusivePointer<CesiumRasterOverlays::RasterOverlay>
      pOverlay = this->_pOverlay.get();
  pTileset->getOverlays().remove(pOverlay);
  this->_pOverlay = nullptr;
}

} // namespace CesiumForUnityNative
