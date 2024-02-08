#include "CesiumIonRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumIonServerHelper.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumAsync/IAssetResponse.h>
#include <CesiumRasterOverlays/IonRasterOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumIonRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/Collections/IEnumerator.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Coroutine.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
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
    ionAccessToken = overlay.ionServer().defaultIonAccessToken();
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  std::string apiUrl = overlay.ionServer().apiUrl().ToStlString();
  if (!apiUrl.empty() && *apiUrl.rbegin() != '/')
    apiUrl += '/';

  if (!apiUrl.empty()) {
    this->_pOverlay = new IonRasterOverlay(
        overlay.materialKey().ToStlString(),
        overlay.ionAssetID(),
        ionAccessToken.ToStlString(),
        options,
        apiUrl);

    pTileset->getOverlays().add(this->_pOverlay);
  } else {
    // Resolve the API URL if it's not already in progress.
    resolveCesiumIonApiUrl(overlay.ionServer());
    overlay.StartCoroutine(overlay.AddToTilesetLater(tileset));
  }
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
