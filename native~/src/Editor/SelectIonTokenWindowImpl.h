#pragma once

#include "CesiumImpl.h"

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumIonClient/Token.h>

#include <DotNet/CesiumForUnity/IonTokenSource.h>
#include <DotNet/System/String.h>

#include <memory>
#include <optional>
#include <vector>

namespace DotNet::CesiumForUnity {
class SelectIonTokenWindow;
class CesiumIonServer;
} // namespace DotNet::CesiumForUnity

namespace CesiumForUnityNative {

class SelectIonTokenWindowImpl : public CesiumImpl<SelectIonTokenWindowImpl> {

public:
  static CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>
  SelectNewToken(const DotNet::CesiumForUnity::CesiumIonServer& server);

  static CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
  SelectTokenIfNecessary(const DotNet::CesiumForUnity::CesiumIonServer& server);

  static CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
  SelectAndAuthorizeToken(
      const DotNet::CesiumForUnity::CesiumIonServer& server,
      const std::vector<int64_t>& assetIDs);

  SelectIonTokenWindowImpl(
      const DotNet::CesiumForUnity::SelectIonTokenWindow& window);
  ~SelectIonTokenWindowImpl();

  void
  RefreshTokens(const DotNet::CesiumForUnity::SelectIonTokenWindow& window);

  void CreateToken(const DotNet::CesiumForUnity::SelectIonTokenWindow& window);

  void
  UseExistingToken(const DotNet::CesiumForUnity::SelectIonTokenWindow& window);

  void SpecifyToken(const DotNet::CesiumForUnity::SelectIonTokenWindow& window);

private:
  std::optional<CesiumAsync::Promise<std::optional<CesiumIonClient::Token>>>
      _promise;
  std::optional<
      CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>>
      _future;

  std::vector<std::shared_ptr<CesiumIonClient::Token>> _tokens;
};
} // namespace CesiumForUnityNative
