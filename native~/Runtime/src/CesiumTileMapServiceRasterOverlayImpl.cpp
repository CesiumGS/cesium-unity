#include "CesiumTileMapServiceRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/TileMapServiceRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumTileMapServiceRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumTileMapServiceRasterOverlayImpl::CesiumTileMapServiceRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumTileMapServiceRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumTileMapServiceRasterOverlayImpl::
    ~CesiumTileMapServiceRasterOverlayImpl() {}

void CesiumTileMapServiceRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumTileMapServiceRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.url())) {
    // Don't create an overlay with an empty URL.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  TileMapServiceRasterOverlayOptions tmsOptions;
  if (overlay.maximumLevel() > overlay.minimumLevel() &&
      overlay.specifyZoomLevels()) {
    tmsOptions.minimumLevel = overlay.minimumLevel();
    tmsOptions.maximumLevel = overlay.maximumLevel();
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new TileMapServiceRasterOverlay(
      overlay.name().ToStlString(),
      overlay.url().ToStlString(),
      std::vector<CesiumAsync::IAssetAccessor::THeader>(),
      tmsOptions,
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumTileMapServiceRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumTileMapServiceRasterOverlay& overlay,
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
