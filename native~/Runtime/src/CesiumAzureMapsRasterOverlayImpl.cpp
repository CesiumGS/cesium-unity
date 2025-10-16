#include "CesiumAzureMapsRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumRasterOverlays/AzureMapsRasterOverlay.h>

// #include <DotNet/CesiumForUnity/AzureMapsStyle.h>
#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumAzureMapsRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumAzureMapsRasterOverlayImpl::CesiumAzureMapsRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumAzureMapsRasterOverlayImpl::~CesiumAzureMapsRasterOverlayImpl() {}

void CesiumAzureMapsRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumAzureMapsRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
/*
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }
  if (System::String::IsNullOrEmpty(overlay.AzureMapsKey())) {
    // Don't create an overlay with an empty map key.
    return;
  }

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
/*
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
  */
}

} // namespace CesiumForUnityNative
