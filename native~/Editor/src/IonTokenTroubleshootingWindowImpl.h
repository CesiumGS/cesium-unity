#pragma once

#include <DotNet/System/String.h>

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
      bool isDefaultToken);

  void SelectNewDefaultToken(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);

private:
};
} // namespace CesiumForUnityNative
