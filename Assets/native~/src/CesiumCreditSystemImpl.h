#pragma once

#include <Cesium3DTilesSelection/CreditSystem.h>

#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/UI/Text.h>

#include <memory>

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
  void Awake(const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  void Update(const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem);
  void OnApplicationQuit();

  const std::shared_ptr<Cesium3DTilesSelection::CreditSystem>&
  getExternalCreditSystem() const;

  static DotNet::CesiumForUnity::CesiumCreditSystem getDefaultCreditSystem();

private:
  std::shared_ptr<Cesium3DTilesSelection::CreditSystem> _pCreditSystem;
  static DotNet::UnityEngine::GameObject _creditSystemPrefab;

  size_t _lastCreditsCount;

  DotNet::UnityEngine::UI::Text _popupText;
  DotNet::UnityEngine::UI::Text _onScreenText;
};

} // namespace CesiumForUnityNative
