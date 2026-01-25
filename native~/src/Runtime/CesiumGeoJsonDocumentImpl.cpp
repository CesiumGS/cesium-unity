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
    : _document(), _isValid(false) {}

CesiumGeoJsonDocumentImpl::~CesiumGeoJsonDocumentImpl() {}

void CesiumGeoJsonDocumentImpl::setNativeDocument(GeoJsonDocument&& document) {
  _document = std::move(document);
  _isValid = true;
}

bool CesiumGeoJsonDocumentImpl::IsValid(
    const CesiumForUnity::CesiumGeoJsonDocument& document) {
  return _isValid;
}

CesiumForUnity::CesiumGeoJsonObject CesiumGeoJsonDocumentImpl::GetRootObject(
    const CesiumForUnity::CesiumGeoJsonDocument& document) {
  if (!_isValid) {
    return CesiumForUnity::CesiumGeoJsonObject(nullptr);
  }

  auto pObject = std::make_shared<GeoJsonObject>(_document.rootObject);
  CesiumForUnity::CesiumGeoJsonObject result;
  result.NativeImplementation().setNativeObject(std::move(pObject));
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

  _document = std::move(*result.value);
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

void CesiumGeoJsonDocumentImpl::DisposeNative(
    const CesiumForUnity::CesiumGeoJsonDocument& document) {
  _isValid = false;
}

} // namespace CesiumForUnityNative
