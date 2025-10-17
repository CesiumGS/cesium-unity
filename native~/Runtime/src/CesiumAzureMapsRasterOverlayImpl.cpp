#include "CesiumAzureMapsRasterOverlayImpl.h"

#include "../../Editor/generated-Editor/include/DotNet/UnityEngine/Debug.h"
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
std::string
getTilesetId(::DotNet::CesiumForUnity::AzureMapsTilesetId tilesetId) {
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

  if (this->_pOverlay) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.key())) {
    // Don't create an overlay with an empty map key.
    return;
  }

  const CesiumForUnity::CesiumRasterOverlay& genericOverlay = overlay;
  const auto& options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  auto& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset) {
    return;
  }

  AzureMapsSessionParameters sessionParameters{
      .key = overlay.key().ToStlString(),
      .apiVersion = overlay.apiVersion().ToStlString(),
      .tilesetId = getTilesetId(overlay.tilesetId()),
      .language = overlay.language().ToStlString(),
      .view = overlay.view().ToStlString(),
  };
  this->_pOverlay = new AzureMapsRasterOverlay(
      overlay.materialKey().ToStlString(),
      sessionParameters,
      options);
  pTileset->getOverlays().add(this->_pOverlay);
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
