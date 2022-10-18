#pragma once

#include <CesiumAsync/AsyncSystem.h>

#include <CesiumIonClient/Token.h>

#include <DotNet/System/String.h>

#include <memory>
#include <optional>
#include <vector>

namespace DotNet::CesiumForUnity {
class IonTokenSelector;
}

namespace CesiumForUnityNative {

class IonTokenSelectorImpl {
public:
  IonTokenSelectorImpl(
      const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector);
  ~IonTokenSelectorImpl();

  void JustBeforeDelete(
      const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector);

  void
  RefreshTokens(const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector);
  void
  CreateToken(const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector);
  void UseExistingToken(
      const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector);
  void
  SpecifyToken(const DotNet::CesiumForUnity::IonTokenSelector& tokenSelector,
               DotNet::System::String token);

private:
  std::optional<CesiumAsync::Promise<std::optional<CesiumIonClient::Token>>>
      _promise;
  std::optional<
      CesiumAsync::SharedFuture<std::optional<CesiumIonClient::Token>>>
      _future;
};
} // namespace CesiumForUnityNative
