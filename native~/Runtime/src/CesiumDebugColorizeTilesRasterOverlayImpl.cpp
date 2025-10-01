#include "CesiumDebugColorizeTilesRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumRasterOverlays/DebugColorizeTilesRasterOverlay.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumDebugColorizeTilesRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace DotNet;

namespace CesiumForUnityNative {

CesiumDebugColorizeTilesRasterOverlayImpl::
    CesiumDebugColorizeTilesRasterOverlayImpl(
        const DotNet::CesiumForUnity::CesiumDebugColorizeTilesRasterOverlay&
            overlay)
    : _pOverlay(nullptr) {}

CesiumDebugColorizeTilesRasterOverlayImpl::
    ~CesiumDebugColorizeTilesRasterOverlayImpl() {}

void CesiumDebugColorizeTilesRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumDebugColorizeTilesRasterOverlay&
        overlay,
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

  this->_pOverlay = new DebugColorizeTilesRasterOverlay(
      overlay.materialKey().ToStlString(),
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumDebugColorizeTilesRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumDebugColorizeTilesRasterOverlay&
        overlay,
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
