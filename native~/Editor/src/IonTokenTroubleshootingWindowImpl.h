#pragma once

namespace DotNet::CesiumForUnity {
  class IonTokenTroubleshootingWindow;
}

namespace CesiumForUnityNative {

class IonTokenTroubleshootingWindowImpl {

public:
  IonTokenTroubleshootingWindowImpl(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);
  ~IonTokenTroubleshootingWindowImpl();

  void JustBeforeDelete(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);

  void GetTroubleshootingDetails(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);

  void AuthorizeToken(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
      DotNet::System::String token,
      bool removeAssetToken);

private:
};
} // namespace CesiumForUnityNative
