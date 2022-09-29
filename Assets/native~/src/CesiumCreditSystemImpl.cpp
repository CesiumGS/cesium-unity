#include "CameraManager.h"
#include "Cesium3DTilesetImpl.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/CesiumCreditSystem.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Object.h>
#include <DotNet/UnityEngine/Resources.h>
#include <DotNet/UnityEngine/Transform.h>
#include <DotNet/UnityEngine/UI/Text.h>

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

UnityEngine::GameObject CesiumCreditSystemImpl::_creditSystemPrefab = nullptr;

CesiumCreditSystemImpl::CesiumCreditSystemImpl(
    const CesiumForUnity::CesiumCreditSystem& creditSystem)
    : _pCreditSystem(std::make_shared<CreditSystem>()),
      _lastCreditsCount(0),
      _popupText(nullptr),
      _onScreenText(nullptr) {}

CesiumCreditSystemImpl::~CesiumCreditSystemImpl() {}

void CesiumCreditSystemImpl::JustBeforeDelete(
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {}

void CesiumCreditSystemImpl::Awake(
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {
  UnityEngine::GameObject gameObject = creditSystem.gameObject();
  UnityEngine::GameObject canvasGameObject =
      gameObject.transform().GetChild(0).gameObject();

  UnityEngine::GameObject popupGameObject =
      canvasGameObject.transform().Find(System::String("Popup")).gameObject();
  UnityEngine::GameObject popupTextGameObject =
      popupGameObject.transform().GetChild(0).gameObject();
  _popupText = popupTextGameObject.GetComponent<UnityEngine::UI::Text>();

  UnityEngine::GameObject onScreenGameObject =
      canvasGameObject.transform()
          .Find(System::String("OnScreen"))
          .gameObject();
  UnityEngine::GameObject onScreenTextGameObject =
      onScreenGameObject.transform().GetChild(0).gameObject();
  _onScreenText = onScreenTextGameObject.GetComponent<UnityEngine::UI::Text>();
}

void CesiumCreditSystemImpl::Update(
    const CesiumForUnity::CesiumCreditSystem& creditSystem) {
  if (!_pCreditSystem) {
    return;
  }

  const std::vector<Cesium3DTilesSelection::Credit>& creditsToShowThisFrame =
      _pCreditSystem->getCreditsToShowThisFrame();

  // If the credit list has changed, reformat the credits
  size_t creditsCount = creditsToShowThisFrame.size();
  bool creditsUpdated =
      creditsCount != _lastCreditsCount ||
      _pCreditSystem->getCreditsToNoLongerShowThisFrame().size() > 0;

  if (creditsUpdated) {
    std::string popupCredits = "";
    // System::String onScreenCredits;
    //  TODO: mimic the Tick() code in Unreal's credit system here

    // TODO: Add a link at the bottom that says "Data Attribution."
    // clicking on it should make the popup come up.

    bool first = true;
    for (int i = 0; i < creditsCount; i++) {
      const Cesium3DTilesSelection::Credit& credit = creditsToShowThisFrame[i];
      if (i != 0) {
        popupCredits += "\n";
      }

      const std::string& html = _pCreditSystem->getHtml(credit);
      popupCredits += html;
    }

    _popupText.text(System::String(popupCredits));
    _lastCreditsCount = creditsCount;
  }

  _pCreditSystem->startNextFrame();
}

void CesiumCreditSystemImpl::OnApplicationQuit() {
  // Dereference the prefab. If this isn't done, the Editor will try to
  // use the destroyed prefab the next time it enters play mode.
  CesiumCreditSystemImpl::_creditSystemPrefab = nullptr;
}


const std::shared_ptr<Cesium3DTilesSelection::CreditSystem>&
CesiumCreditSystemImpl::getExternalCreditSystem() const {
  return _pCreditSystem;
}

CesiumForUnity::CesiumCreditSystem
CesiumCreditSystemImpl::getDefaultCreditSystem() {
  UnityEngine::GameObject defaultCreditSystemObject = nullptr;
  System::String defaultName = System::String("CesiumCreditSystemDefault");

  // Look for an existing default credit system first.
  System::Array1<CesiumForUnity::CesiumCreditSystem> creditSystems =
      UnityEngine::Object::FindObjectsOfType<
          CesiumForUnity::CesiumCreditSystem>();

  for (int32_t i = 0, len = creditSystems.Length(); i < len; i++) {
    CesiumForUnity::CesiumCreditSystem creditSystem = creditSystems[i];
    if (creditSystem.name().StartsWith(defaultName)) {
      defaultCreditSystemObject = creditSystem.gameObject();
      break;
    }
  }

  // If no default credit system was found, instantiate one.
  if (defaultCreditSystemObject == nullptr) {
    if (CesiumCreditSystemImpl::_creditSystemPrefab == nullptr) {
      CesiumCreditSystemImpl::_creditSystemPrefab =
          UnityEngine::Resources::Load<UnityEngine::GameObject>(
              System::String("CesiumCreditSystem"));
    }

    defaultCreditSystemObject =
        UnityEngine::Object::Instantiate(_creditSystemPrefab);
    defaultCreditSystemObject.name(defaultName);
  }

  return defaultCreditSystemObject
      .GetComponent<CesiumForUnity::CesiumCreditSystem>();
}

} // namespace CesiumForUnityNative
