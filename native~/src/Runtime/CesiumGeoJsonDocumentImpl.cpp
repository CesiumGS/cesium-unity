#include "CesiumGeoJsonDocumentImpl.h"
#include "CesiumGeoJsonObjectImpl.h"
#include "UnityTilesetExternals.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/IAssetAccessor.h>
#include <CesiumUtility/Result.h>
#include <CesiumVectorData/GeoJsonDocument.h>
#include <CesiumVectorData/GeoJsonObject.h>

#include <DotNet/CesiumForUnity/CesiumGeoJsonDocument.h>
#include <DotNet/CesiumForUnity/CesiumGeoJsonObject.h>
#include <DotNet/System/Action1.h>
#include <DotNet/System/String.h>

#include <cstring>
#include <memory>
#include <span>
#include <vector>

using namespace DotNet;
using namespace CesiumAsync;
using namespace CesiumVectorData;
using namespace CesiumUtility;

namespace CesiumForUnityNative {

CesiumGeoJsonDocumentImpl::CesiumGeoJsonDocumentImpl(
    const CesiumForUnity::CesiumGeoJsonDocument& document)
    : _pDocument(nullptr), _isValid(false) {}

CesiumGeoJsonDocumentImpl::~CesiumGeoJsonDocumentImpl() {
  _pDocument = nullptr;
  _isValid = false;
}

void CesiumGeoJsonDocumentImpl::setNativeDocument(GeoJsonDocument&& document) {
  _pDocument = std::make_shared<GeoJsonDocument>(std::move(document));
  _isValid = true;
}

bool CesiumGeoJsonDocumentImpl::IsValid(
    const CesiumForUnity::CesiumGeoJsonDocument& document) {
  return _isValid && _pDocument != nullptr;
}

CesiumForUnity::CesiumGeoJsonObject CesiumGeoJsonDocumentImpl::GetRootObject(
    const CesiumForUnity::CesiumGeoJsonDocument& document) {
  if (!_isValid || !_pDocument) {
    return CesiumForUnity::CesiumGeoJsonObject(nullptr);
  }

  CesiumForUnity::CesiumGeoJsonObject result;
  // Pass a pointer to the root object within the document, not a copy
  result.NativeImplementation().setNativeObjectInDocument(
      _pDocument,
      &_pDocument->rootObject);
  return result;
}

bool CesiumGeoJsonDocumentImpl::ParseInternal(
    const CesiumForUnity::CesiumGeoJsonDocument& document,
    System::String geoJsonString) {
  std::string str = geoJsonString.ToStlString();

  std::vector<std::byte> bytes(str.size());
  std::memcpy(bytes.data(), str.data(), str.size());

  Result<GeoJsonDocument> result =
      GeoJsonDocument::fromGeoJson(std::span<const std::byte>(bytes));

  if (!result.value.has_value()) {
    return false;
  }

  _pDocument = std::make_shared<GeoJsonDocument>(std::move(*result.value));
  _isValid = true;
  return true;
}

void CesiumGeoJsonDocumentImpl::LoadFromUrl(
    System::String url,
    System::Action1<CesiumForUnity::CesiumGeoJsonDocument> callback) {
  std::string urlStr = url.ToStlString();

  const AsyncSystem& asyncSystem = getAsyncSystem();
  std::shared_ptr<IAssetAccessor> pAssetAccessor = getAssetAccessor();

  GeoJsonDocument::fromUrl(asyncSystem, pAssetAccessor, urlStr)
      .thenInMainThread(
          [callback](Result<GeoJsonDocument>&& result) {
            if (!result.value.has_value()) {
              callback.Invoke(CesiumForUnity::CesiumGeoJsonDocument(nullptr));
              return;
            }

            CesiumForUnity::CesiumGeoJsonDocument doc;
            doc.NativeImplementation().setNativeDocument(
                std::move(*result.value));
            callback.Invoke(doc);
          });
}

void CesiumGeoJsonDocumentImpl::LoadFromCesiumIon(
    std::int64_t ionAssetId,
    System::String ionAccessToken,
    System::String ionApiUrl,
    System::Action1<CesiumForUnity::CesiumGeoJsonDocument> callback) {
  std::string accessToken = ionAccessToken.ToStlString();
  std::string apiUrl = ionApiUrl.ToStlString();

  const AsyncSystem& asyncSystem = getAsyncSystem();
  std::shared_ptr<IAssetAccessor> pAssetAccessor = getAssetAccessor();

  GeoJsonDocument::fromCesiumIonAsset(
      asyncSystem,
      pAssetAccessor,
      ionAssetId,
      accessToken,
      apiUrl)
      .thenInMainThread(
          [callback](Result<GeoJsonDocument>&& result) {
            if (!result.value.has_value()) {
              callback.Invoke(CesiumForUnity::CesiumGeoJsonDocument(nullptr));
              return;
            }

            CesiumForUnity::CesiumGeoJsonDocument doc;
            doc.NativeImplementation().setNativeDocument(
                std::move(*result.value));
            callback.Invoke(doc);
          });
}

} // namespace CesiumForUnityNative
