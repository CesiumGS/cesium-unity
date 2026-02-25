#include "CesiumGeoJsonDocumentRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumGeoJsonDocumentImpl.h"
#include "CesiumRasterOverlayUtility.h"
#include "UnityExternals.h"

#include <DotNet/CesiumForUnity/CesiumGeoJsonDocument.h>

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
#include <DotNet/CesiumForUnity/CesiumColor32.h>
#include <DotNet/System/String.h>

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
  CesiumForUnity::CesiumColor32 color = lineStyle.color;
  CesiumVectorData::LineStyle native;
  native.color = CesiumUtility::Color(color.r, color.g, color.b, color.a);
  native.colorMode = toNativeColorMode(lineStyle.colorMode);
  native.width = lineStyle.width;
  native.widthMode = toNativeLineWidthMode(lineStyle.widthMode);
  return native;
}

CesiumVectorData::ColorStyle toNativeFillStyle(
    const CesiumForUnity::CesiumVectorPolygonFillStyle& fillStyle) {
  CesiumForUnity::CesiumColor32 color = fillStyle.color;
  CesiumVectorData::ColorStyle native;
  native.color = CesiumUtility::Color(color.r, color.g, color.b, color.a);
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
        spdlog::default_logger()->info(
            "GeoJSON document loader future resolved");

        if (documentResult.errors) {
          spdlog::default_logger()->error(
              "GeoJSON document has errors!");
          documentResult.errors.logError(
              spdlog::default_logger(),
              "Errors loading GeoJSON document: ");
          return nullptr;
        }

        if (!documentResult.value.has_value()) {
          spdlog::default_logger()->error(
              "GeoJSON document result has no value!");
          return nullptr;
        }

        spdlog::default_logger()->info(
            "GeoJSON document loaded successfully");

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
  spdlog::default_logger()->info("CesiumGeoJsonDocumentRasterOverlay::AddToTileset called");

  if (this->_pOverlay != nullptr) {
    spdlog::default_logger()->info("Overlay already added, returning");
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset) {
    spdlog::default_logger()->error("No tileset found!");
    return;
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  const CesiumGeospatial::Ellipsoid& ellipsoid = pTileset->getEllipsoid();

  CesiumVectorData::VectorStyle nativeStyle = toNativeVectorStyle(overlay.defaultStyle());

  spdlog::default_logger()->info(
      "GeoJSON style - Line color: ({}, {}, {}, {}), width: {}, Polygon fill: {}, outline: {}",
      nativeStyle.line.color.r,
      nativeStyle.line.color.g,
      nativeStyle.line.color.b,
      nativeStyle.line.color.a,
      nativeStyle.line.width,
      nativeStyle.polygon.fill.has_value(),
      nativeStyle.polygon.outline.has_value());

  if (nativeStyle.polygon.fill.has_value()) {
    spdlog::default_logger()->info(
        "GeoJSON polygon fill color: ({}, {}, {}, {})",
        nativeStyle.polygon.fill->color.r,
        nativeStyle.polygon.fill->color.g,
        nativeStyle.polygon.fill->color.b,
        nativeStyle.polygon.fill->color.a);
  }

  GeoJsonDocumentRasterOverlayOptions vectorOptions{
      nativeStyle,
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
  } else if (source ==
             CesiumForUnity::CesiumGeoJsonDocumentRasterOverlaySource::
                 FromDocument) {
    // FromDocument - use a pre-parsed and styled document
    CesiumForUnity::CesiumGeoJsonDocument doc = overlay.document();
    if (doc == nullptr) {
      spdlog::default_logger()->error(
          "GeoJSON document is null! Call SetGeoJsonDocument first.");
      return;
    }

    CesiumGeoJsonDocumentImpl& docImpl = doc.NativeImplementation();

    // Use the shared_ptr directly to preserve per-feature styles
    std::shared_ptr<GeoJsonDocument> pDocument = docImpl.getSharedDocument();
    if (!pDocument) {
      spdlog::default_logger()->error(
          "GeoJSON document shared_ptr is null!");
      return;
    }

    spdlog::default_logger()->info(
        "Loading GeoJSON from pre-parsed document with per-feature styles");

    this->_pOverlay = new GeoJsonDocumentRasterOverlay(
        overlay.materialKey().ToStlString(),
        getAsyncSystem().createResolvedFuture(std::move(pDocument)),
        vectorOptions,
        options);
  } else {
    // FromUrl
    System::String url = overlay.url();
    if (System::String::IsNullOrEmpty(url)) {
      spdlog::default_logger()->error("GeoJSON URL is empty!");
      return;
    }

    std::string urlStr = url.ToStlString();
    spdlog::default_logger()->info("Loading GeoJSON from URL: {}", urlStr);

    this->_pOverlay = new GeoJsonDocumentRasterOverlay(
        overlay.materialKey().ToStlString(),
        wrapLoaderFuture(GeoJsonDocument::fromUrl(
            getAsyncSystem(),
            getAssetAccessor(),
            urlStr)),
        vectorOptions,
        options);
  }

  spdlog::default_logger()->info(
      "GeoJSON overlay created with materialKey: {}",
      overlay.materialKey().ToStlString());

  pTileset->getOverlays().add(this->_pOverlay);
  spdlog::default_logger()->info("GeoJSON overlay added to tileset");
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
