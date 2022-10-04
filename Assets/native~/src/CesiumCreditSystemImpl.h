#pragma once

#include <Cesium3DTilesSelection/CreditSystem.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/UnityEngine/GameObject.h>

#include <memory>
#include <unordered_map>

namespace DotNet::CesiumForUnity {
class CesiumCreditSystem;
class GameObject;
} // namespace DotNet::CesiumForUnity

namespace Cesium3DTilesSelection {
class CreditSystem;
class Credit;
} // namespace Cesium3DTilesSelection

namespace CesiumForUnityNative {

class CesiumCreditSystemImpl {
public:
  CesiumCreditSystemImpl(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  ~CesiumCreditSystemImpl();

  void JustBeforeDelete(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  void Update(const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  void OnApplicationQuit(
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);

  const std::shared_ptr<Cesium3DTilesSelection::CreditSystem>&
  getExternalCreditSystem() const;

  static DotNet::CesiumForUnity::CesiumCreditSystem getDefaultCreditSystem();

private:
  // The underlying cesium-native credit system.
  std::shared_ptr<Cesium3DTilesSelection::CreditSystem> _pCreditSystem;

  // The GameObject prefab used to host the CreditSystem script and UI.
  static DotNet::UnityEngine::GameObject _creditSystemPrefab;

  const std::string convertHtmlToRtf(
      const std::string& html,
      const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem
      );

  std::unordered_map<std::string, std::string> _htmlToRtf;
  size_t _lastCreditsCount;
};

} // namespace CesiumForUnityNative
