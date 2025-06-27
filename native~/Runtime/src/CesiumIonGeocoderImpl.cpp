#include "CesiumIonGeocoderImpl.h"

#include "UnityTilesetExternals.h"

#include <CesiumIonClient/Connection.h>
#include <CesiumIonClient/Geocoder.h>
#include <CesiumUtility/Assert.h>

#include <DotNet/CesiumForUnity/CesiumIonGeocoder.h>
#include <DotNet/CesiumForUnity/CesiumIonGeocoderAttribution.h>
#include <DotNet/CesiumForUnity/CesiumIonGeocoderFeature.h>
#include <DotNet/CesiumForUnity/CesiumIonGeocoderProviderType.h>
#include <DotNet/CesiumForUnity/CesiumIonGeocoderRequestType.h>
#include <DotNet/CesiumForUnity/CesiumIonGeocoderResult.h>
#include <DotNet/CesiumForUnity/CesiumIonServer.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Exception.h>
#include <DotNet/System/Threading/Tasks/Task1.h>
#include <DotNet/System/Threading/Tasks/TaskCompletionSource1.h>
#include <DotNet/Unity/Mathematics/double3.h>

namespace {
CesiumIonClient::GeocoderProviderType geocoderProviderTypeEnumToNative(
    DotNet::CesiumForUnity::CesiumIonGeocoderProviderType providerType) {
  switch (providerType) {
  case DotNet::CesiumForUnity::CesiumIonGeocoderProviderType::Bing:
    return CesiumIonClient::GeocoderProviderType::Bing;
  case DotNet::CesiumForUnity::CesiumIonGeocoderProviderType::Google:
    return CesiumIonClient::GeocoderProviderType::Google;
  case DotNet::CesiumForUnity::CesiumIonGeocoderProviderType::Default:
    return CesiumIonClient::GeocoderProviderType::Default;
  }

  CESIUM_ASSERT(false && "Invalid CesiumIonGeocoderProviderType value");
  return CesiumIonClient::GeocoderProviderType::Default;
}

CesiumIonClient::GeocoderRequestType geocoderRequestTypeEnumToNative(
    DotNet::CesiumForUnity::CesiumIonGeocoderRequestType requestType) {
  switch (requestType) {
  case DotNet::CesiumForUnity::CesiumIonGeocoderRequestType::Autocomplete:
    return CesiumIonClient::GeocoderRequestType::Autocomplete;
  case DotNet::CesiumForUnity::CesiumIonGeocoderRequestType::Search:
    return CesiumIonClient::GeocoderRequestType::Search;
  }

  CESIUM_ASSERT(false && "Invalid CesiumIonGeocoderRequestType value");
  return CesiumIonClient::GeocoderRequestType::Search;
}
} // namespace

namespace CesiumForUnityNative {
CesiumIonGeocoderImpl::CesiumIonGeocoderImpl(
    const DotNet::CesiumForUnity::CesiumIonGeocoder& geocoder)
    : _pConnection(nullptr) {}

DotNet::System::Threading::Tasks::Task1<
    DotNet::CesiumForUnity::CesiumIonGeocoderResult>
CesiumIonGeocoderImpl::Geocode(
    const DotNet::CesiumForUnity::CesiumIonGeocoder& /*geocoder*/,
    const DotNet::CesiumForUnity::CesiumIonServer& server,
    DotNet::System::String ionToken,
    DotNet::CesiumForUnity::CesiumIonGeocoderProviderType providerType,
    DotNet::CesiumForUnity::CesiumIonGeocoderRequestType requestType,
    DotNet::System::String query) {
  CesiumAsync::AsyncSystem asyncSystem = getAsyncSystem();

  DotNet::System::Threading::Tasks::TaskCompletionSource1<
      DotNet::CesiumForUnity::CesiumIonGeocoderResult>
      promise{};

  getConnection(asyncSystem, server, ionToken)
      .thenImmediately(
          [providerType, requestType, query](
              std::shared_ptr<CesiumIonClient::Connection>&& pConnection) {
            return pConnection->geocode(
                geocoderProviderTypeEnumToNative(providerType),
                geocoderRequestTypeEnumToNative(requestType),
                query.ToStlString());
          })
      .thenImmediately([promise](CesiumIonClient::Response<
                                  CesiumIonClient::GeocoderResult>&& response) {
        if (!response.errorCode.empty()) {
          promise.SetException(DotNet::System::Exception(DotNet::System::String(
              "Ion error code received: " + response.errorCode +
              ", message: " + response.errorMessage)));
        } else {
          DotNet::System::Array1<
              DotNet::CesiumForUnity::CesiumIonGeocoderAttribution>
              attributions(response.value->attributions.size());
          for (size_t i = 0; i < response.value->attributions.size(); i++) {
            DotNet::CesiumForUnity::CesiumIonGeocoderAttribution attribution;
            attribution.html(
                DotNet::System::String(response.value->attributions[i].html));
            attribution.showOnScreen(
                response.value->attributions[i].showOnScreen);
            attributions.Item((int32_t)i, attribution);
          }

          DotNet::System::Array1<
              DotNet::CesiumForUnity::CesiumIonGeocoderFeature>
              features(response.value->features.size());
          for (size_t i = 0; i < response.value->features.size(); i++) {
            DotNet::CesiumForUnity::CesiumIonGeocoderFeature feature;
            feature.displayName(DotNet::System::String(
                response.value->features[i].displayName));
            CesiumGeospatial::Cartographic point =
                response.value->features[i].getCartographic();
            feature.positionLlh(DotNet::Unity::Mathematics::double3(
                point.longitude,
                point.latitude,
                point.height));
            features.Item((int32_t)i, feature);
          }

          DotNet::CesiumForUnity::CesiumIonGeocoderResult geocoderResult;
          geocoderResult.attributions(attributions);
          geocoderResult.features(features);
          promise.SetResult(geocoderResult);
        }
      });

  return promise.Task();
}

CesiumAsync::Future<std::shared_ptr<CesiumIonClient::Connection>>
CesiumIonGeocoderImpl::getConnection(
    const CesiumAsync::AsyncSystem& asyncSystem,
    const DotNet::CesiumForUnity::CesiumIonServer& server,
    DotNet::System::String ionToken) {
  if (this->_pConnection != nullptr) {
    return asyncSystem.createResolvedFuture(
        std::shared_ptr(this->_pConnection));
  }

  std::shared_ptr<CesiumAsync::IAssetAccessor> pAssetAccessor =
      getAssetAccessor();

  return CesiumIonClient::Connection::appData(
             asyncSystem,
             pAssetAccessor,
             server.apiUrl().ToStlString())
      .thenImmediately(
          [ionToken, server, this, asyncSystem, pAssetAccessor](
              CesiumIonClient::Response<CesiumIonClient::ApplicationData>&&
                  response) {
            if (!response.value) {
              return std::shared_ptr<CesiumIonClient::Connection>(nullptr);
            }

            this->_connectionMutex.lock();
            if (this->_pConnection != nullptr) {
              // Another query has already created a connection before this one
              // returned.
              this->_connectionMutex.unlock();
              return this->_pConnection;
            }

            this->_pConnection = std::make_shared<CesiumIonClient::Connection>(
                asyncSystem,
                pAssetAccessor,
                DotNet::System::String::IsNullOrWhiteSpace(ionToken)
                    ? server.defaultIonAccessToken().ToStlString()
                    : ionToken.ToStlString(),
                *response.value,
                server.apiUrl().ToStlString());
            this->_connectionMutex.unlock();
            return this->_pConnection;
          });
}
} // namespace CesiumForUnityNative
