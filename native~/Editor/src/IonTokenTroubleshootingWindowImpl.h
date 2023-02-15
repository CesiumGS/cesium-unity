#pragma once

#include <DotNet/System/String.h>

namespace DotNet::CesiumForUnity {
class IonTokenTroubleshootingWindow;
}

namespace CesiumForUnityNative {

class IonTokenTroubleshootingWindowImpl {

public:
  static void GetTroubleshootingDetails(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);

  static void AuthorizeToken(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window,
      DotNet::System::String token,
      bool isDefaultToken);

  static void SelectNewDefaultToken(
      const DotNet::CesiumForUnity::IonTokenTroubleshootingWindow& window);
};

} // namespace CesiumForUnityNative
