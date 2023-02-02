#include "CesiumWebMapServiceRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <Cesium3DTilesSelection/WebMapServiceRasterOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumWebMapServiceRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumWebMapServiceRasterOverlayImpl::CesiumWebMapServiceRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumWebMapServiceRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumWebMapServiceRasterOverlayImpl::~CesiumWebMapServiceRasterOverlayImpl() {}

void CesiumWebMapServiceRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumWebMapServiceRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.baseUrl())) {
    // Don't create an overlay with an empty base URL.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  WebMapServiceRasterOverlayOptions wmsOptions;
  if (overlay.maximumLevel() > overlay.minimumLevel()) {
    wmsOptions.minimumLevel = overlay.minimumLevel();
    wmsOptions.maximumLevel = overlay.maximumLevel();
  }
  wmsOptions.layers = overlay.layers().ToStlString();
  wmsOptions.tileWidth = overlay.tileWidth();
  wmsOptions.tileHeight = overlay.tileHeight();

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new WebMapServiceRasterOverlay(
      overlay.name().ToStlString(),
      overlay.baseUrl().ToStlString(),
      std::vector<CesiumAsync::IAssetAccessor::THeader>(),
      wmsOptions,
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumWebMapServiceRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumWebMapServiceRasterOverlay& overlay,
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
