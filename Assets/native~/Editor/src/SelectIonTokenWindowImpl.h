#pragma once

#include <CesiumAsync/AsyncSystem.h>
#include <CesiumAsync/SharedFuture.h>
#include <CesiumIonClient/Response.h>
#include <CesiumIonClient/Token.h>

#include <DotNet/CesiumForUnity/IonTokenSource.h>
#include <DotNet/System/String.h>

#include <memory>
#include <optional>
#include <vector>

namespace DotNet::CesiumForUnity {
class SelectIonTokenWindow;
}

namespace CesiumForUnityNative {

class SelectIonTokenWindowImpl {

public:
  static CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>
  SelectNewToken();

  static CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
  SelectTokenIfNecessary();

  static CesiumAsync::Future<std::optional<CesiumIonClient::Token>>
  SelectAndAuthorizeToken(const std::vector<int64_t>& assetIDs);

  SelectIonTokenWindowImpl(
      const DotNet::CesiumForUnity::SelectIonTokenWindow& window);
  ~SelectIonTokenWindowImpl();

  void
  JustBeforeDelete(const DotNet::CesiumForUnity::SelectIonTokenWindow& window);

  void
  RefreshTokens(const DotNet::CesiumForUnity::SelectIonTokenWindow& window);

  void CreateToken(
      const DotNet::CesiumForUnity::SelectIonTokenWindow& window,
      DotNet::System::String name);

  void UseExistingToken(
      const DotNet::CesiumForUnity::SelectIonTokenWindow& window,
      int tokenIndex);

  void SpecifyToken(
      const DotNet::CesiumForUnity::SelectIonTokenWindow& window,
      DotNet::System::String token);

private:
  std::optional<CesiumAsync::Promise<std::optional<CesiumIonClient::Token>>>
      _promise;
  std::optional<
      CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>>
      _future;

  std::vector<std::shared_ptr<CesiumIonClient::Token>> _tokens;
};
} // namespace CesiumForUnityNative
