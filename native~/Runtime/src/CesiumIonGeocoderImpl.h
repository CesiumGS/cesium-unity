#pragma once

#include "CesiumImpl.h"

#include <CesiumAsync/Future.h>

#include <DotNet/System/String.h>
#include <DotNet/System/Threading/Tasks/Task1.h>

#include <memory>

namespace DotNet::CesiumForUnity {
class CesiumIonGeocoder;
class CesiumIonGeocoderResult;
enum class CesiumIonGeocoderRequestType;
enum class CesiumIonGeocoderProviderType;
class CesiumIonServer;

} // namespace DotNet::CesiumForUnity

namespace CesiumIonClient {
class Connection;
}

namespace CesiumForUnityNative {

class CesiumIonGeocoderImpl : public CesiumImpl<CesiumIonGeocoderImpl> {
public:
  CesiumIonGeocoderImpl(
      const DotNet::CesiumForUnity::CesiumIonGeocoder& geocoder);

  DotNet::System::Threading::Tasks::Task1<
      DotNet::CesiumForUnity::CesiumIonGeocoderResult>
  Geocode(
      const DotNet::CesiumForUnity::CesiumIonGeocoder& geocoder,
      const DotNet::CesiumForUnity::CesiumIonServer& server,
      DotNet::System::String ionToken,
      DotNet::CesiumForUnity::CesiumIonGeocoderProviderType providerType,
      DotNet::CesiumForUnity::CesiumIonGeocoderRequestType requestType,
      DotNet::System::String query);

private:
  CesiumAsync::Future<std::shared_ptr<CesiumIonClient::Connection>>
  getConnection(
      const DotNet::CesiumForUnity::CesiumIonServer& server,
      DotNet::System::String ionToken);

  std::shared_ptr<CesiumIonClient::Connection> _pConnection;
};

} // namespace CesiumForUnityNative
