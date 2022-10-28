#pragma once

#include <string>

namespace DotNet::CesiumForUnity {
  class IonTokenTroubleshootingWindow;
}

namespace CesiumForUnityNative {

struct TokenState {
  std::string token;
  bool isValid;
  bool allowsAccessToAsset;
  bool isAssociatedWithUserAccount;
};

class IonTokenTroubleshootingWindowImpl {

public:
  IonTokenTroubleshootingWindowImpl(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);
  ~IonTokenTroubleshootingWindowImpl();

  void JustBeforeDelete(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);

  void GetTroubleshootingDetails(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);

private:
  TokenState _assetTokenState;
  TokenState _defaultTokenState;

  bool _assetExistsInUserAccount;
};
} // namespace CesiumForUnityNative
