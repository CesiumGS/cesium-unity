#include "CesiumIonRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumAsync/IAssetResponse.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumIonRasterOverlayImpl::CesiumIonRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumIonRasterOverlayImpl::~CesiumIonRasterOverlayImpl() {}

void CesiumIonRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumIonRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (overlay.ionAssetID() <= 0) {
    // Don't create an overlay for an invalid asset ID.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  System::String ionAccessToken = overlay.ionAccessToken();
  if (System::String::IsNullOrEmpty(ionAccessToken)) {
    ionAccessToken =
        CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken();
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new IonRasterOverlay(
      overlay.name().ToStlString(),
      overlay.ionAssetID(),
      ionAccessToken.ToStlString(),
      options);

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
