#include "CesiumGeoJsonDocumentRasterOverlayImpl.h"

#include "Cesium3DTilesetImpl.h"
#include "CesiumGeoJsonDocumentImpl.h"
#include "CesiumRasterOverlayUtility.h"
#include "CesiumVectorStyleConversions.h"
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
#include <DotNet/System/String.h>

using namespace Cesium3DTilesSelection;
using namespace CesiumRasterOverlays;
using namespace CesiumVectorData;
using namespace DotNet;

namespace {

CesiumAsync::Future<std::shared_ptr<CesiumVectorData::GeoJsonDocument>>
wrapLoaderFuture(
    CesiumAsync::Future<CesiumUtility::Result<CesiumVectorData::GeoJsonDocument>>&&
        future) {
  return std::move(future).thenImmediately(
      [](CesiumUtility::Result<CesiumVectorData::GeoJsonDocument>&&
             documentResult)
          -> std::shared_ptr<CesiumVectorData::GeoJsonDocument> {
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
    return;
  }

  Cesium3DTilesetImpl& tilesetImpl = tileset.NativeImplementation();
  Tileset* pTileset = tilesetImpl.getTileset();
  if (!pTileset) {
    return;
  }

  CesiumForUnity::CesiumRasterOverlay genericOverlay = overlay;
  RasterOverlayOptions options =
      CesiumRasterOverlayUtility::GetOverlayOptions(genericOverlay);

  const CesiumGeospatial::Ellipsoid& ellipsoid = pTileset->getEllipsoid();

  CesiumVectorData::VectorStyle nativeStyle = CesiumForUnityNative::fromUnityStyle(overlay.defaultStyle());

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

    if (!apiUrl.empty()) {
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
    }
  } else if (source ==
             CesiumForUnity::CesiumGeoJsonDocumentRasterOverlaySource::
                 FromDocument) {
    // FromDocument - use a pre-parsed and styled document
    CesiumForUnity::CesiumGeoJsonDocument doc = overlay.document();
    if (doc == nullptr) {
      return;
    }

    CesiumGeoJsonDocumentImpl& docImpl = doc.NativeImplementation();

    // Use the shared_ptr directly to preserve per-feature styles
    std::shared_ptr<GeoJsonDocument> pDocument = docImpl.getSharedDocument();
    if (!pDocument) {
      return;
    }

    this->_pOverlay = new GeoJsonDocumentRasterOverlay(
        overlay.materialKey().ToStlString(),
        getAsyncSystem().createResolvedFuture(std::move(pDocument)),
        vectorOptions,
        options);
  } else {
    // FromUrl
    System::String url = overlay.url();
    if (System::String::IsNullOrEmpty(url)) {
      return;
    }

    std::string urlStr = url.ToStlString();

    this->_pOverlay = new GeoJsonDocumentRasterOverlay(
        overlay.materialKey().ToStlString(),
        wrapLoaderFuture(GeoJsonDocument::fromUrl(
            getAsyncSystem(),
            getAssetAccessor(),
            urlStr)),
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
