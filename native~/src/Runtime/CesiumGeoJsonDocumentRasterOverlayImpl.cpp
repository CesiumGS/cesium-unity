#include "CesiumGeoJsonDocumentRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumRasterOverlayUtility.h"
#include "UnityExternals.h"

#include <Cesium3DTilesSelection/Tileset.h>
#include <CesiumRasterOverlays/GeoJsonDocumentRasterOverlay.h>
#include <CesiumUtility/Color.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/VectorStyle.h>

#include <spdlog/spdlog.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonDocumentRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonDocumentRasterOverlaySource.h>
#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumVectorColorMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorLineWidthMode.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonFillStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorPolygonStyle.h>
#include <DotNet/CesiumForUnity/CesiumVectorStyle.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Color32.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace CesiumVectorData;
using namespace DotNet;

namespace {

CesiumVectorData::ColorMode
toNativeColorMode(CesiumForUnity::CesiumVectorColorMode mode) {
  return static_cast<CesiumVectorData::ColorMode>(
      static_cast<uint8_t>(mode));
}

CesiumVectorData::LineWidthMode
toNativeLineWidthMode(CesiumForUnity::CesiumVectorLineWidthMode mode) {
  return static_cast<CesiumVectorData::LineWidthMode>(
      static_cast<uint8_t>(mode));
}

CesiumVectorData::LineStyle
toNativeLineStyle(const CesiumForUnity::CesiumVectorLineStyle& lineStyle) {
  UnityEngine::Color32 color = lineStyle.color;
  CesiumVectorData::LineStyle native;
  native.color = CesiumUtility::Color(
      color.r / 255.0f,
      color.g / 255.0f,
      color.b / 255.0f,
      color.a / 255.0f);
  native.colorMode = toNativeColorMode(lineStyle.colorMode);
  native.width = lineStyle.width;
  native.widthMode = toNativeLineWidthMode(lineStyle.widthMode);
  return native;
}

CesiumVectorData::ColorStyle toNativeFillStyle(
    const CesiumForUnity::CesiumVectorPolygonFillStyle& fillStyle) {
  UnityEngine::Color32 color = fillStyle.color;
  CesiumVectorData::ColorStyle native;
  native.color = CesiumUtility::Color(
      color.r / 255.0f,
      color.g / 255.0f,
      color.b / 255.0f,
      color.a / 255.0f);
  native.colorMode = toNativeColorMode(fillStyle.colorMode);
  return native;
}

CesiumVectorData::VectorStyle
toNativeVectorStyle(const CesiumForUnity::CesiumVectorStyle& style) {
  CesiumForUnity::CesiumVectorLineStyle lineStyle = style.lineStyle;
  CesiumForUnity::CesiumVectorPolygonStyle polygonStyle = style.polygonStyle;

  CesiumVectorData::VectorStyle native;
  native.line = toNativeLineStyle(lineStyle);

  if (polygonStyle.fill) {
    native.polygon.fill = toNativeFillStyle(polygonStyle.fillStyle);
  }

  if (polygonStyle.outline) {
    native.polygon.outline = toNativeLineStyle(polygonStyle.outlineStyle);
  }

  return native;
}

CesiumAsync::Future<std::shared_ptr<CesiumVectorData::GeoJsonDocument>>
wrapLoaderFuture(
    CesiumAsync::Future<CesiumUtility::Result<CesiumVectorData::GeoJsonDocument>>&&
        future) {
  return std::move(future).thenImmediately(
      [](CesiumUtility::Result<CesiumVectorData::GeoJsonDocument>&&
             documentResult)
          -> std::shared_ptr<CesiumVectorData::GeoJsonDocument> {
        if (documentResult.errors) {
          documentResult.errors.logError(
              spdlog::default_logger(),
              "Errors loading GeoJSON document: ");
          return nullptr;
        }

        return std::make_shared<CesiumVectorData::GeoJsonDocument>(
            std::move(*documentResult.value));
      });
}

} // namespace

namespace CesiumForUnityNative {

CesiumGeoJsonDocumentRasterOverlayImpl::CesiumGeoJsonDocumentRasterOverlayImpl(
    const DotNet::CesiumForUnity::CesiumGeoJsonDocumentRasterOverlay& overlay)
    : _pOverlay(nullptr) {}

CesiumGeoJsonDocumentRasterOverlayImpl::
    ~CesiumGeoJsonDocumentRasterOverlayImpl() {}

void CesiumGeoJsonDocumentRasterOverlayImpl::AddToTileset(
    const ::DotNet::CesiumForUnity::CesiumGeoJsonDocumentRasterOverlay& overlay,
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

  const CesiumGeospatial::Ellipsoid& ellipsoid = pTileset->getEllipsoid();

  GeoJsonDocumentRasterOverlayOptions vectorOptions{
      toNativeVectorStyle(overlay.defaultStyle()),
      ellipsoid,
      static_cast<uint32_t>(overlay.mipLevels())};

  CesiumForUnity::CesiumGeoJsonDocumentRasterOverlaySource source =
      overlay.source();

  if (source ==
      CesiumForUnity::CesiumGeoJsonDocumentRasterOverlaySource::FromCesiumIon) {
    if (overlay.ionAssetID() <= 0) {
      // Don't create an overlay for an invalid asset ID.
      return;
    }

    System::String ionAccessToken = overlay.ionAccessToken();
    if (System::String::IsNullOrEmpty(ionAccessToken)) {
      ionAccessToken = overlay.ionServer().defaultIonAccessToken();
    }

    std::string apiUrl = overlay.ionServer().apiUrl().ToStlString();
    if (!apiUrl.empty() && *apiUrl.rbegin() != '/')
      apiUrl += '/';

    this->_pOverlay = new GeoJsonDocumentRasterOverlay(
        overlay.materialKey().ToStlString(),
        wrapLoaderFuture(GeoJsonDocument::fromCesiumIonAsset(
            getAsyncSystem(),
            getAssetAccessor(),
            overlay.ionAssetID(),
            ionAccessToken.ToStlString(),
            apiUrl)),
        vectorOptions,
        options);
  } else {
    // FromUrl
    System::String url = overlay.url();
    if (System::String::IsNullOrEmpty(url)) {
      // Don't create an overlay without a URL.
      return;
    }

    this->_pOverlay = new GeoJsonDocumentRasterOverlay(
        overlay.materialKey().ToStlString(),
        wrapLoaderFuture(GeoJsonDocument::fromUrl(
            getAsyncSystem(),
            getAssetAccessor(),
            url.ToStlString())),
        vectorOptions,
        options);
  }

  pTileset->getOverlays().add(this->_pOverlay);
}

void CesiumGeoJsonDocumentRasterOverlayImpl::RemoveFromTileset(
    const ::DotNet::CesiumForUnity::CesiumGeoJsonDocumentRasterOverlay& overlay,
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
