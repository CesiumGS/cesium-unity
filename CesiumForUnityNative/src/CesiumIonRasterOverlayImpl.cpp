#include "CesiumIonRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;

namespace CesiumForUnityNative {

CesiumIonRasterOverlayImpl::CesiumIonRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumIonRasterOverlayImpl::~CesiumIonRasterOverlayImpl() {}

void CesiumIonRasterOverlayImpl::JustBeforeDelete(
    const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay) {}

void CesiumIonRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  this->_pOverlay = new IonRasterOverlay(
      overlay.name().ToStlString(),
      overlay.ionAssetID(),
      overlay.ionAccessToken().ToStlString());

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumIonRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
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
