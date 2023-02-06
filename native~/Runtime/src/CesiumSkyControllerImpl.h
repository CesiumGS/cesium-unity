#pragma once

namespace DotNet::UnityEngine {
struct Vector3;
} // namespace DotNet::UnityEngine

namespace DotNet::CesiumForUnity {
class CesiumSkyController;

}

namespace CesiumForUnityNative {
class CesiumSkyControllerImpl {
public:
  CesiumSkyControllerImpl(
      const DotNet::CesiumForUnity::CesiumSkyController& skyController);
  ~CesiumSkyControllerImpl();

  void JustBeforeDelete(
      const DotNet::CesiumForUnity::CesiumSkyController& skyController);

  DotNet::UnityEngine::Vector3 CalculateSunPosition(
      const DotNet::CesiumForUnity::CesiumSkyController& skyController,
      float& time);
};

} // namespace CesiumForUnityNative
