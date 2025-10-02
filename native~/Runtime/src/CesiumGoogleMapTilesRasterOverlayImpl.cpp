#include "CesiumGoogleMapTilesRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumGeospatial/Ellipsoid.h>
#include <CesiumJsonReader/JsonObjectJsonHandler.h>
#include <CesiumJsonReader/JsonReader.h>
#include <CesiumRasterOverlays/GoogleMapTilesRasterOverlay.h>
#include <CesiumUtility/JsonValue.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumGoogleMapTilesRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/GoogleMapTilesLayerType.h>
#include <DotNet/CesiumForUnity/GoogleMapTilesMapType.h>
#include <DotNet/CesiumForUnity/GoogleMapTilesScale.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Debug.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumJsonReader;
using namespace CesiumRasterOverlays;
using namespace CesiumUtility;
using namespace DotNet;

namespace CesiumForUnityNative {

namespace {
std::string getMapType(CesiumForUnity::GoogleMapTilesMapType mapType) {
  switch (mapType) {
  case CesiumForUnity::GoogleMapTilesMapType::Roadmap:
    return CesiumRasterOverlays::GoogleMapTilesMapType::roadmap;
  case CesiumForUnity::GoogleMapTilesMapType::Terrain:
    return CesiumRasterOverlays::GoogleMapTilesMapType::terrain;
  case CesiumForUnity::GoogleMapTilesMapType::Satellite:
  default:
    return CesiumRasterOverlays::GoogleMapTilesMapType::satellite;
  }
}

std::string getScale(CesiumForUnity::GoogleMapTilesScale scale) {
  switch (scale) {
  case CesiumForUnity::GoogleMapTilesScale::ScaleFactor4x:
    return CesiumRasterOverlays::GoogleMapTilesScale::scaleFactor4x;
  case CesiumForUnity::GoogleMapTilesScale::ScaleFactor2x:
    return CesiumRasterOverlays::GoogleMapTilesScale::scaleFactor2x;
  case CesiumForUnity::GoogleMapTilesScale::ScaleFactor1x:
  default:
    return CesiumRasterOverlays::GoogleMapTilesScale::scaleFactor1x;
  }
}

std::optional<std::vector<std::string>> getLayerTypes(
    const DotNet::System::Collections::Generic::List1<
        CesiumForUnity::GoogleMapTilesLayerType>& layerTypes,
    CesiumForUnity::GoogleMapTilesMapType mapType) {
  std::vector<std::string> result;

  bool hasRoadmap = false;

  int32_t count = layerTypes.Count();
  if (count > 0) {
    result.reserve(layerTypes.Count());

    for (int32_t i = 0; i < count; i++) {
      CesiumForUnity::GoogleMapTilesLayerType layerType = layerTypes[i];
      switch (layerType) {
      case CesiumForUnity::GoogleMapTilesLayerType::Roadmap:
        hasRoadmap = true;
        result.emplace_back(GoogleMapTilesLayerType::layerRoadmap);
        break;
      case CesiumForUnity::GoogleMapTilesLayerType::Streetview:
        result.emplace_back(GoogleMapTilesLayerType::layerStreetview);
        break;
      case CesiumForUnity::GoogleMapTilesLayerType::Traffic:
        result.emplace_back(GoogleMapTilesLayerType::layerTraffic);
        break;
      }
    }
  }

  if (mapType == CesiumForUnity::GoogleMapTilesMapType::Terrain &&
      !hasRoadmap) {
    UnityEngine::Debug::LogWarning(System::String(
        "When the mapType is set to Terrain on "
        "CesiumGoogleMapTilesRasterOverlay, layerTypes must contain "
        "Roadmap."));
  }

  return result;
}

JsonValue::Array getStyles(
    const DotNet::System::Collections::Generic::List1<DotNet::System::String>&
        styles) {
  int32_t count = styles.Count();

  JsonValue::Array result;
  result.reserve(count);

  JsonObjectJsonHandler handler{};

  for (int32_t i = 0; i < count; ++i) {
    const System::String& style = styles[i];
    std::string styleUtf8 = style.ToStlString();
    ReadJsonResult<JsonValue> response = JsonReader::readJson(
        std::span<const std::byte>(
            reinterpret_cast<const std::byte*>(styleUtf8.data()),
            styleUtf8.size()),
        handler);

    ErrorList errorList;
    errorList.errors = std::move(response.errors);
    errorList.warnings = std::move(response.warnings);
    errorList.log(
        spdlog::default_logger(),
        fmt::format("Problems parsing JSON in element {} of Styles:", i));

    if (response.value) {
      result.emplace_back(std::move(*response.value));
    }
  }

  return result;
}

} // namespace

CesiumGoogleMapTilesRasterOverlayImpl::CesiumGoogleMapTilesRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumGoogleMapTilesRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumGoogleMapTilesRasterOverlayImpl::
    ~CesiumGoogleMapTilesRasterOverlayImpl() {}

void CesiumGoogleMapTilesRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumGoogleMapTilesRasterOverlay& overlay,
    const ::DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  if (this->_pOverlay != nullptr) {
    // Overlay already added.
    return;
  }

  if (System::String::IsNullOrEmpty(overlay.apiKey())) {
    // Don't create an overlay with an empty API key.
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset)
    return;

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  this->_pOverlay = new GoogleMapTilesRasterOverlay(
      overlay.materialKey().ToStlString(),
      CesiumRasterOverlays::GoogleMapTilesNewSessionParameters{
          .key = overlay.apiKey().ToStlString(),
          .mapType = getMapType(overlay.mapType()),
          .language = overlay.language().ToStlString(),
          .region = overlay.region().ToStlString(),
          .scale = getScale(overlay.scale()),
          .highDpi = overlay.highDpi(),
          .layerTypes = getLayerTypes(overlay.layerTypes(), overlay.mapType()),
          .styles = getStyles(overlay.styles()),
          .overlay = overlay.overlay()},
      options);

  pTileset->getOverlays().add(this->_pOverlay);
}
void CesiumGoogleMapTilesRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumGoogleMapTilesRasterOverlay& overlay,
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
