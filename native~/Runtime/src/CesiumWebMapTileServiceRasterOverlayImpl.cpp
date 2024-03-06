#include "CesiumWebMapTileServiceRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumRasterOverlays/WebMapTileServiceRasterOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumWebMapTileServiceRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumWebMapTileServiceRasterOverlayImpl::CesiumWebMapTileServiceRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumWebMapTileServiceRasterOverlayImpl::~CesiumWebMapTileServiceRasterOverlayImpl() {}

void CesiumWebMapTileServiceRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay) {
    // Overlay already added to a tileset, do nothing.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset) {
    // Tileset not valid, cannot add overlay.
    return;
  }

  WebMapTileServiceRasterOverlayOptions wmtsOptions;
  wmtsOptions.format = overlay.format().ToStlString();
  wmtsOptions.layer = overlay.layer().ToStlString();
  wmtsOptions.style = overlay.style().ToStlString();
  wmtsOptions.tileMatrixSetID = overlay.tileMatrixSetID().ToStlString();
  wmtsOptions.minimumLevel = overlay.minimumLevel();
  wmtsOptions.maximumLevel = overlay.maximumLevel();
  wmtsOptions.tileWidth = overlay.tileWidth();
  wmtsOptions.tileHeight = overlay.tileHeight();

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new WebMapTileServiceRasterOverlay(
      overlay.materialKey().ToStlString(),
      overlay.baseUrl().ToStlString(),
      std::vector<CesiumAsync::IAssetAccessor::THeader>(),
      wmtsOptions,
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumWebMapTileServiceRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumWebMapTileServiceRasterOverlay& overlay,
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
