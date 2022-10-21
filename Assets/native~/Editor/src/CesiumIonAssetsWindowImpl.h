#pragma once

namespace DotNet::CesiumForUnity {
class CesiumIonAssetsWindow;
}

namespace CesiumForUnityNative {

class CesiumIonAssetsWindowImpl {

public:
  CesiumIonAssetsWindowImpl(
      const DotNet::CesiumForUnity::CesiumIonAssetsWindow& window);
  ~CesiumIonAssetsWindowImpl();

  void
  JustBeforeDelete(const DotNet::CesiumForUnity::CesiumIonAssetsWindow& window);


private:
};
} // namespace CesiumForUnityNative
